using contr9.Models;
using Microsoft.EntityFrameworkCore;

namespace contr9.Services;

public class ServiceInitializer
{
    private readonly ModelBuilder _modelBuilder;

    public ServiceInitializer(ModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
    }
    public void Seed()
    {
        _modelBuilder.Entity<Service>().HasData(
            new Service { Id = 1, OrganizationName = "Aknet" },
            new Service { Id = 2, OrganizationName = "Sever Electrocity" },
            new Service { Id = 3, OrganizationName = "Gov Fines" }
            );
        _modelBuilder.Entity<ServiceUser>().HasData(
            new ServiceUser { Id = 1, ServiceId = 1, UserAccount = "18092003", Balance = 1800 },
            new ServiceUser { Id = 2, ServiceId = 2, UserAccount = "14061963", Balance = 6300 },
            new ServiceUser { Id = 3, ServiceId = 3, UserAccount = "27072003", Balance = 2700 });
    }
}