using contr9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace contr9.Controllers;

public class AccountController : Controller
{
    private WalletDb _db;
    private UserManager<User> _userManager;
    private SignInManager<User> _signInManager;
    private IWebHostEnvironment _environment;


    public AccountController(WalletDb db, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    
    [Authorize]
    public async Task<IActionResult> Profile(int? id)
    {
        User user = new User();
        if (!id.HasValue)
            user = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        else
            user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var adminUser =await _userManager.IsInRoleAsync(currentUser, "admin");
            var isOwner = currentUser.Id == user.Id;
            ViewBag.EditAccess = adminUser || isOwner;
            ViewBag.DeleteAccess = adminUser;
            ViewBag.IsAdmin = adminUser && isOwner;
            ViewBag.RoleIdent = await _userManager.IsInRoleAsync(user, "user");
            return View(user);
        }
        return NotFound();
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(User model, int userId, string? currentPassword, string? password)
    {
        User? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            if (password != null && currentPassword != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, currentPassword, password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return NotFound();
                }
            }
            await _signInManager.RefreshSignInAsync(user);
            await _userManager.UpdateAsync(user);
            return new ObjectResult(user);
        }
        ModelState.AddModelError("","Пользователь не найден");
        return NotFound();
    }
    
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        User? currentUser = await _userManager.GetUserAsync(User);
        bool isAdmin = false;
        if (currentUser != null)
            isAdmin = await _userManager.IsInRoleAsync(currentUser, "admin");
        if (currentUser == null || !isAdmin)
        {
            ViewData["FormName"] = "Регистрация";
            ViewData["ButtonName"] = "Зарегистрироваться";
            ViewBag.IsAdmin = false;
        }
        else
        {
            ViewData["FormName"] = "Создание нового пользователя";
            ViewData["ButtonName"] = "Создать";
            ViewBag.IsAdmin = true;
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
       
        Random random = new Random();
        bool isAdmin = false;
        int uniqueNumber;
        do
        {
            uniqueNumber = random.Next(100000, 1000000); 
        } while (_db.Users.Any(u => u.Account.Equals(uniqueNumber.ToString())));
        if (ModelState.IsValid)
        {
            User user = new User
            {
                Email = model.Email,
                UserName = model.UserName,
                Account = uniqueNumber.ToString(),
                Balance = 100000,
                PhoneNumber = model.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                User? currentUser = await _userManager.GetUserAsync(User);
                
                if (currentUser != null)
                    isAdmin = await _userManager.IsInRoleAsync(currentUser, "admin");
                if (isAdmin)
                {
                    await _userManager.AddToRoleAsync(user, "user");
                    return RedirectToAction("Profile", "Account", new {id = user.Id});
                }
                else
                {
                    await _userManager.AddToRoleAsync(user,"user");
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        ModelState.AddModelError("", "Something went wrong! Please check all info");
        ViewBag.IsAdmin = isAdmin;
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel(){ReturnUrl = returnUrl});
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _userManager.FindByEmailAsync(model.LoginValue);
            if (user == null)
                user = await _db.Users.FirstOrDefaultAsync(u => u.Account.Equals(model.LoginValue));
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("LoginValue", "Invalid email, login or password!");
        }
        ModelState.AddModelError("", "Invalid provided form!");
        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}