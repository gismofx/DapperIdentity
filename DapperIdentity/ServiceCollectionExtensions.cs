using System;
using System.Reflection;
using DapperIdentity.Controllers;
using DapperIdentity.Stores;
using System.Data;
using DapperRepository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using IdentityRole = DapperIdentity.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;

namespace DapperIdentity.Services
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Added because docs say so, but this is NOT required. Maybe because some magic in blazor project? or Net 5?
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityControllers(this IServiceCollection services)
        {
            /*
            var assembly = typeof(IdentityController).Assembly;
            services.AddControllersWithViews()
                .AddApplicationPart(assembly)
                .AddRazorRuntimeCompilation();
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(assembly));
            });
            */

            var assembly = typeof(IdentityController).GetTypeInfo().Assembly;
            var part = new AssemblyPart(assembly);
            services.AddMvcCore().AddControllersAsServices().AddApplicationPart(assembly);// ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));

            return services;
        }

        public static IServiceCollection AddDapperIdentityWithCustomCookies<T>(this IServiceCollection services,
                                                                         string connectionString,
                                                                         TimeSpan cookieExpiration,
                                                                         bool requireConfirmedEmail = true,
                                                                         bool slidingExpiration = true) where T : IDbConnection
        {
            services.AddDbConnectionInstantiatorForRepositories<T>(connectionString);
            return services.AddDapperIdentityWithCustomCookies(cookieExpiration, requireConfirmedEmail, slidingExpiration);
        }

        /// <summary>
        /// Use this method to setup Identity
        /// </summary>
        /// <param name="services"></param>
        /// <param name="cookieExpiration">e.g. TimeSpan.FromDays(7) or TimeSpan.FromMinutes(30);</param>
        /// <param name="requireConfirmedEmail"></param>
        /// <param name="slidingExpiration"></param>
        /// <returns></returns>
        public static IServiceCollection AddDapperIdentityWithCustomCookies(this IServiceCollection services,
                                                                         TimeSpan cookieExpiration,
                                                                         bool requireConfirmedEmail = true,
                                                                         bool slidingExpiration = true)
        {
            services.AddTransient(typeof(IRepository<IdentityUser>), typeof(Repository<IdentityUser>));
            services.AddTransient(typeof(IRepository<IdentityRole>), typeof(Repository<IdentityRole>));

            services.AddTransient<IUserStore<IdentityUser>, UserStore>();
            services.AddTransient<IRoleStore<IdentityRole>, RoleStore>();
            services.AddIdentity<IdentityUser, IdentityRole>(); //, IdentityRole>();

            services.Configure<IdentityOptions>(opts =>
            {
                opts.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                opts.User.RequireUniqueEmail = true;
            }
            );

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => o.LoginPath = new PathString("/Identity/Account/Login"));

            services.Configure<SecurityStampValidatorOptions>(o =>
                o.ValidationInterval = TimeSpan.FromMinutes(1));

            //timespan
            services.ConfigureApplicationCookie(o =>
            {
                o.ExpireTimeSpan = cookieExpiration;
                o.SlidingExpiration = slidingExpiration;
            }
            );

            return services;
        }

        /// <summary>
        /// Adds vanilla Identity. Assumes you have scaffolded or are using Identity UI/Razor Pages Area
        /// </summary>
        /// <param name="services"></param>
        /// <param name="requireConfirmedEmail"></param>
        /// <param name="slidingExpiration"></param>
        /// <returns></returns>
        public static IServiceCollection AddVanillaIdentityDefaults(this IServiceCollection services,
                                                                bool requireConfirmedEmail = true,
                                                                bool slidingExpiration = true)
        {
            return services.AddVanillaIdentityDefaults(TimeSpan.FromDays(7), requireConfirmedEmail, slidingExpiration);
        }

        /// <summary>
        /// Adds Dapper Identity Stores with DEFAULT Microsoft Identity UI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="cookieExpiration"></param>
        /// <param name="requireConfirmedEmail"></param>
        /// <param name="slidingExpiration"></param>
        /// <returns></returns>
        public static IServiceCollection AddVanillaIdentityDefaults(this IServiceCollection services,
                                                                TimeSpan cookieExpiration,
                                                                bool requireConfirmedEmail = true,
                                                                bool slidingExpiration = true)
        {
            services.AddTransient(typeof(IRepository<IdentityUser>), typeof(Repository<IdentityUser>));
            services.AddTransient(typeof(IRepository<IdentityRole>), typeof(Repository<IdentityRole>));

            //authentication/auth
            services.AddTransient<IUserStore<IdentityUser>, UserStore>();
            services.AddTransient<IRoleStore<IdentityRole>, RoleStore>();
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(opts =>
            {
                opts.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
                opts.User.RequireUniqueEmail = true;
            }
            );

            services.Configure<SecurityStampValidatorOptions>(o =>
                o.ValidationInterval = TimeSpan.FromMinutes(1));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => o.LoginPath = new PathString("/Identity/Account/Login"));

            //timespan
            services.ConfigureApplicationCookie(o =>
            {
                o.ExpireTimeSpan = cookieExpiration; //TimeSpan.FromDays(7);//FromMinutes(30);
                o.SlidingExpiration = slidingExpiration;
            }
            );

            return services;

            /*
/*.AddUserStore<UserStore>()
.AddRoleStore<RoleStore>()
.AddSignInManager<SignInManager<CPEIdentityUser>>()
.AddPasswordValidator<PasswordValidator<CPEIdentityUser>>()
.AddUserManager<UserManager<CPEIdentityUser>>()
.AddRoleManager<RoleManager<CPEIdentityRole>>() //maybe not necessary
.AddRoles<CPEIdentityRole>() //maybe not necessary
;
*/
        }
    }
}