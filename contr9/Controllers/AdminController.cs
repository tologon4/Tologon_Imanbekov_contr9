using contr9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace contr9.Controllers;
[Authorize(Roles = "admin")]
[Authorize]
public class AdminController : Controller
{
    private Contr9Db _db;
    private UserManager<User> _userManager;
    private SignInManager<User> _signInManager;
    private IWebHostEnvironment _environment;


    public AdminController(Contr9Db db, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    [Authorize]
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int? id)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        var currentUser = await _userManager.GetUserAsync(User);
        var adminUser = await _userManager.IsInRoleAsync(currentUser, "admin");
        if (user!=null && adminUser)
        {
            _db.Users.Remove(user);
            int n = await _db.SaveChangesAsync();
            if (n>0)
                return Ok(true);
        }
        return Ok(false);
    }
}