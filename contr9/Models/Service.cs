namespace contr9.Models;

public class Service
{
    public int Id { get; set; }
    public string OrganizationName { get; set; }
    public ICollection<ServiceUser> ServiceUsers { get; set; }

}