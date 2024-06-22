namespace contr9.Models;

public class ServiceUser
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public string UserAccount { get; set; } 

    public decimal Balance { get; set; }
}