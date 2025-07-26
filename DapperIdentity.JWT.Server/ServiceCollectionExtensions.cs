using System;
using System.Reflection;

using System.Data;
using DapperRepository;
using DapperIdentity.Stores;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DapperIdentity.Services;

using IdentityRole = DapperIdentity.Core.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;

using Microsoft.IdentityModel.Tokens;
using DapperIdentity.Services;
using DapperIdentity.JWT.Server.Server.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DapperIdentity.Core.Models;

namespace DapperIdentity.JWT.Server.Services;
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
                                                string validIssuer,
                                                string validAudience,
                                                string symmetricSecurityKey)
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
                ValidIssuer = validIssuer,
                ValidAudience = validAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(symmetricSecurityKey)
                ),
                
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