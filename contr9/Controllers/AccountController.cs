using contr9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace contr9.Controllers;

public class AccountController : Controller
{
    private Contr9Db _db;
    private UserManager<User> _userManager;
    private SignInManager<User> _signInManager;
    private IWebHostEnvironment _environment;


    public AccountController(Contr9Db db, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment)
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
    public async Task<IActionResult> Edit(User model, IFormFile? uploadedFile, int userId, string? password)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            string? path = null;
            if (uploadedFile!=null)
            {
                var buffer = user.Avatar.Split('=');
                var buffer2 = buffer[buffer.Length - 1].Split('.');
                string newFileName = Path.ChangeExtension($"{model.UserName.Trim()}-ProfileN={int.Parse(buffer2[0])+1}", Path.GetExtension(uploadedFile.FileName));
                path= $"/userImages/" + newFileName.Trim();
                using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
            }
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.PasswordHash = password != null ? _userManager.PasswordHasher.HashPassword(user, password) : user.PasswordHash;
            user.Avatar = path != null ? path : user.Avatar;
            _db.Users.Update(user);
            int n = await _db.SaveChangesAsync();
            if (n>0)
                return new ObjectResult(user);
            else
                return BadRequest();
        }
        ModelState.AddModelError("","Пользователь не найден");
        return NotFound();
    }
    
    public async Task<IActionResult> Register()
    {
        User? currentUser = await _userManager.GetUserAsync(User);
        bool isAdmin = false;
        if (currentUser != null)
            isAdmin = await _userManager.IsInRoleAsync(currentUser, "admin");
        if (isAdmin)
        {
            ViewData["FormName"] = "Создание нового пользователя";
            ViewData["ButtonName"] = "Создать";
            ViewBag.IsAdmin = isAdmin;
        }
        else
        {
            ViewData["FormName"] = "Регистрация";
            ViewData["ButtonName"] = "Зарегистрироваться";
            ViewBag.IsAdmin = isAdmin;
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, IFormFile? uploadedFile)
    {
        if (ModelState.IsValid)
        {
            string path = "/userImages/defProf-ProfileN=1.png";
            if (uploadedFile != null)
            {
                string newFileName = Path.ChangeExtension($"{model.UserName.Trim()}-ProfileN=1", Path.GetExtension(uploadedFile.FileName));
                path= $"/userImages/" + newFileName.Trim();
                using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
            }
            User user = new User
            {
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Avatar = path,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                User? currentUser = await _userManager.GetUserAsync(User);
                bool isAdmin = false;
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
                user = await _userManager.FindByNameAsync(model.LoginValue);
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