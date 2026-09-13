using CPE.DapperIdentity.Core.Models;
using CPE.DapperIdentity.JWT.Server.Server.Controllers;
using CPE.DapperIdentity.Services;
using CPE.DapperIdentity.Services;
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
using IdentityRole = CPE.DapperIdentity.Core.Models.CustomIdentityRole;
using IdentityUser = CPE.DapperIdentity.Core.Models.CustomIdentityUser;

namespace CPE.DapperIdentity.JWT.Server.Services;
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
    public static IServiceCollection AddJWTIdentity(this IServiceCollection services,
                                                    IConfiguration configuration)
    {
        services.TryAddDapperIdentityDatabaseStores();
        services.AddScoped<TokenService>();
        var assembly = typeof(JWTAuthController).GetTypeInfo().Assembly;
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