using Microsoft.AspNetCore.Identity;

namespace contr9.Models;

public class User : IdentityUser<int>
{
    public string Account { get; set; }
    public decimal Balance { get; set; }
    public ICollection<Transaction>? Transactions { get; set; }
}