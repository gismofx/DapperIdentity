using System;
using System.Reflection;
using DapperIdentity.Controllers;
using DapperIdentity.Controllers.BasicAuth;
using DapperIdentity.Stores;
using System.Data;
using DapperRepository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DapperIdentity.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;

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
            //services.AddControllers().AddControllersAsServices();
            return services;
        }

        /// <summary>
        /// Adds the Basic Auth Controller
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBasicAuthController(this IServiceCollection services)
        {
            services.TryAddDapperIdentityDatabaseStores();
            
            //var assembly = typeof(BasicAuthController).GetTypeInfo().Assembly;
            //var part = new AssemblyPart(assembly);
            //services.AddMvcCore().AddControllersAsServices().AddApplicationPart(assembly);

            /*services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            */

            // configure DI for application services
            //services.AddScoped<IUserService, UserService>();

            return services;
        }
        /// <summary>
        /// Private Extension To Add The Stores and Database Repositories
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection TryAddDapperIdentityDatabaseStores(this IServiceCollection services)
        {
            services.AddTransientRepository<IdentityUser>();
            services.AddTransientRepository<IdentityRole>();
            //services.TryAddTransient(typeof(IRepository<IdentityUser>), typeof(Repository<IdentityUser>));
            //services.TryAddTransient(typeof(IRepository<IdentityRole>), typeof(Repository<IdentityRole>));

            services.TryAddTransient<IUserStore<IdentityUser>, UserStore>();
            services.TryAddTransient<IRoleStore<IdentityRole>, RoleStore>();
            services.TryAddTransient<UserStore>();
            //, IdentityRole>();
            return services;
        }

        /// <summary>
        /// Add Dapper Identity - No UI
        /// </summary>
        /// <typeparam name="T">Your dapper compatible IDbConnection of your database</typeparam>
        /// <param name="services"></param>
        /// <param name="connectionString">database connection string</param>
        /// <param name="cookieExpiration"></param>
        /// <param name="requireConfirmedEmail"></param>
        /// <param name="slidingExpiration"></param>
        /// <returns></returns>
        public static IServiceCollection AddDapperIdentityWithCustomCookies<T>(this IServiceCollection services,
                                                                         string connectionString,
                                                                         TimeSpan cookieExpiration,
                                                                         bool requireConfirmedEmail = true,
                                                                         bool slidingExpiration = true) where T : IDbConnection
        {
            //services.AddDbConnectionInstantiatorForRepositories<T>(connectionString);
            return services.AddDapperIdentityWithCustomCookies(cookieExpiration, requireConfirmedEmail, slidingExpiration);
        }

        /// <summary>
        /// Use this method to setup Identity
        /// Assumes you have already added the dbinstantiator
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

            services.TryAddDapperIdentityDatabaseStores();
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddSignInManager<CustomSignInManager>()
                .AddRoleStore<RoleStore>()
                .AddUserStore<UserStore>(); //maybe deleet?
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
        public static IServiceCollection AddDapperIdentityWithVanillaUIAndDefaults<T>(this IServiceCollection services,
                                                                string connectionString,
                                                                bool requireConfirmedEmail = true,
                                                                bool slidingExpiration = true) where T : IDbConnection
        {
            services.AddDbConnectionInstantiatorForRepositories<T>(connectionString);
            return services.AddDapperIdentityWithVanillaUIAndDefaults(TimeSpan.FromDays(7), requireConfirmedEmail, slidingExpiration);
        }

        /// <summary>
        /// Adds Dapper Identity Stores with DEFAULT Microsoft Identity UI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="cookieExpiration"></param>
        /// <param name="requireConfirmedEmail"></param>
        /// <param name="slidingExpiration"></param>
        /// <returns></returns>
        public static IServiceCollection AddDapperIdentityWithVanillaUIAndDefaults(this IServiceCollection services,
                                                                TimeSpan cookieExpiration,
                                                                bool requireConfirmedEmail = true,
                                                                bool slidingExpiration = true)
        {
            services.TryAddDapperIdentityDatabaseStores();

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

        }

        public static IServiceCollection AddDapperIdentityForApi(this IServiceCollection services)

        {
            services.AddTransientRepository<IdentityUser>();
            services.AddTransientRepository<IdentityRole>();

            //authentication/auth
            services.AddTransient<IUserStore<IdentityUser>, UserStore>();
            services.AddTransient<IRoleStore<IdentityRole>, RoleStore>();

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();


            return services;

        }
    }
}