using CPE.DapperIdentity.Abstractions;
using CPE.DapperIdentity.Stores.Models;
using CPE.DapperIdentity.Jwt.Server.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CPE.DapperIdentity.Stores;
using DapperRepository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using IdentityRole = CPE.DapperIdentity.Stores.Models.CustomIdentityRole;
using IdentityUser = CPE.DapperIdentity.Stores.Models.CustomIdentityUser;
// This class sits in Microsoft.Extensions.DependencyInjection so AddJwtIdentity is discoverable
// from a consumer's Program.cs with no using at all, so the library's own types need importing.
using CPE.DapperIdentity.Jwt.Server;

namespace Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="validIssuer"></param>
    /// <param name="validAudience"></param>
    /// <param name="symmetricSecurityKey"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtIdentity(this IServiceCollection services,
                                                    IConfiguration configuration)
    {
        services.TryAddDapperIdentityDatabaseStores();
        services.AddScoped<TokenService>();
        var assembly = typeof(JwtAuthController).GetTypeInfo().Assembly;
        var part = new AssemblyPart(assembly);
        services.AddControllers().PartManager.ApplicationParts.Add(part);
        //services.AddMvcCore().AddControllersAsServices().AddApplicationPart(assembly);



        services.AddIdentity<IdentityUser, IdentityRole>(
         options =>
         {
             options.SignIn.RequireConfirmedAccount = false;
             options.User.RequireUniqueEmail = true;
             options.Password.RequireDigit = false;
             options.Password.RequiredLength = 6;
             options.Password.RequireNonAlphanumeric = false;
             options.Password.RequireUppercase = false;
         })
        .AddSignInManager<CustomSignInManager>()
        .AddRoleStore<RoleStore>()
        .AddUserStore<UserStore>()
        .AddDefaultTokenProviders(); 
        

        //Microsoft.AspNetCore.Authentication.JwtBearer.


        services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Microsoft.AspNetCore.Authentication.AuthenticationSchemeBuilder( .Jw JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration.GetSection("JwtTokenSettings")["ValidIssuer"],
                ValidAudience = configuration.GetSection("JwtTokenSettings")["ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("JwtTokenSettings")["SymmetricSecurityKey"]!)
                ),
                RoleClaimType = ClaimTypes.Role,
                
            };
        });


        return services;
    }

    public static IServiceCollection AddIAppSettings(this IServiceCollection services, IAppSettings appSettings)
    {
        services.AddSingleton<IAppSettings>(appSettings);
        return services;
    }


}