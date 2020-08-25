# IdentityEmails
IdentityEmails extends ASP&#46;NET Identity's user manager and stores to support:
- multiple emails per user
- multiple logins from the same provider tracking the email for each login
- merge user accounts keeping roles, claims, logins and emails intact

## Entity Framework Core bindings
The IdentityEmails.EntityFrameworkCore project includes a entity framework backed identity email store.

## Getting started
### ASP&#46;NET Core application
1. Add identity emails support in Startup.ConfigureServices by using the IdentityBuilder.AddIdentityEmail extension method:
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddDefaultIdentity<IdentityUser>()
            .AddDefaultUI(UIFramework.Bootstrap4)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            // Adds identity email support
            .AddIdentityEmail<IdentityUser, ApplicationDbContext>(options => {});

        services.AddAuthentication();
        services.AddMvc();
    }
}
```

2. Setup db context by inheriting from IdentityEmailDbContext:
```csharp
    public class ApplicationDbContext : IdentityEmailDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
```

3. Add EF Core migration:
```
dotnet ef migrations add AddIdentityEmails
```

4. Apply migration to db (example projects runs migrations on startup):
```
dotnet ef database update
```

## Examples
Working examples of using IdentityEmail together with ASP&#46;NET Identity and Entity Framework can be found in the [examples](https://github.com/Logwise/IdentityEmails/tree/master/Examples) folder.

## Contribution & funding
Developed by Logwise with financial support from VINNOVA ([Sweden's Innovation Agency](https://www.vinnova.se/)).

## License
This project is licensed under the [MIT](https://github.com/Logwise/IdentityEmails/blob/master/LICENSE) license.