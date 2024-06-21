using Microsoft.AspNetCore.Identity;

namespace contr9.Models;

public class User : IdentityUser<int>
{
    public string Avatar { get; set; }
}