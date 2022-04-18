using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
//using ApiAuthDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

//ref https://codeburst.io/adding-basic-authentication-to-an-asp-net-core-web-api-project-5439c4cf78ee

namespace DapperIdentity.Controllers.BasicAuth
{
    public class BasicAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly string _realm;

        public BasicAuthFilter(string realm)
        {
            _realm = realm;
            if (string.IsNullOrWhiteSpace(_realm))
            {
                throw new ArgumentNullException(nameof(realm), @"Please provide a non-empty realm value.");
            }
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                string authHeader = context.HttpContext.Request.Headers["Authorization"];
                if (authHeader != null)
                {
                    var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                    if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        var credentials = Encoding.UTF8
                                            .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                                            .Split(':', 2);
                        if (credentials.Length == 2)
                        {
                            if (await IsAuthorized(context, credentials[0], credentials[1]))
                            {
                                var x = context.HttpContext.User;
                                return;
                            }
                        }
                    }
                }

                ReturnUnauthorizedResult(context);
            }
            catch (FormatException)
            {
                ReturnUnauthorizedResult(context);
            }
        }

        public async Task<bool> IsAuthorized(AuthorizationFilterContext context, string username, string password)
        {
            var signinService = context.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var user = await userManager.FindByNameAsync(username);
            if (user is null)
            {
                return false;
            }
            var result = await signinService.PasswordSignInAsync(user, password,isPersistent:false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                //we do this for basic auth.. there is no cookie and refresh. login right away.
                context.HttpContext.User = await signinService.CreateUserPrincipalAsync(user);
            }
            return result.Succeeded;
        }

        private void ReturnUnauthorizedResult(AuthorizationFilterContext context)
        {
            // Return 401 and a basic authentication challenge (causes browser to show login dialog)
            context.HttpContext.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{_realm}\"";
            context.Result = new UnauthorizedResult();
        }


    }
}