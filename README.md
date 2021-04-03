# DapperIdentity
Microsoft Identity Framework with Custom User/Role Stores using Identity

This is geared toward using Identity Framework for Blazor Server applications where you are using Dapper for data access.
The UserStore and RoleStore data access layer classes have been customized to use Dapper. They are in no-way complete. All of the underlying Identity Logic is unchanged, only they way Identity accesses the data in a database.

The `UserStore` and `RoleStore` classes use the Generic Repository here:
https://github.com/gismofx/DapperRepository and also on NuGet: https://www.nuget.org/packages/TheDapperRepository/.

## Pull Requests Welcomed!

More Examples coming soon.

## Getting Started
In `startup.cs` add the following
```c#
using DapperIdentity.Services;
using DapperRepository;
```

In `ConfigureServices` method, add the following:
```c#
var connString = Configuration.GetConnectionString("DefaultConnection");
services.AddDbConnectionInstantiatorForRepositories<MySqlConnection>(connString);

//To use DEFAULT MS Identity UI Razor Pages Add Vanilla
//services.AddDapperIdentityWithVanillaUIAndDefaults();
//or only Identity Middle and Back-End use this:
services.AddDapperIdentityWithCustomCookies(TimeSpan.FromMinutes(10));//Or however long you want login cookie to last

 
services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<DapperIdentity.Models.CustomIdentityUser>>();
```

Logging in requires an HTTP POST so the cookies can created.
Here's a simple HTML form section which you can use for logging in which you can put on any page in Blazor:

```html
    <form action="Identity/Login" method="post"><!--cookie-->
        <input name="name" type="text" />
        <input name="password" type="password" />
        <input type="submit" />
    </form> 
```



To Do:
* Make a better/generic login razor component. 
* Handle invalid user/pass. 
* Add remember me checkbox
* Handle Email Confirmation(Add Controller Action)
* NuGet Package
