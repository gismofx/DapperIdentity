using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityRole = CPE.DapperIdentity.Stores.Models.CustomIdentityRole;
using IdentityUser = CPE.DapperIdentity.Stores.Models.CustomIdentityUser;

namespace CPE.DapperIdentity.Cookies.Server.Helpers
    {
        public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
        
        private readonly SignInManager<IdentityUser> _SignInManager;

        public BasicAuthenticationHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                SignInManager<IdentityUser> signInManager)
                // ISystemClock and the four-argument base are obsolete as of .NET 8 (use
                // TimeProvider). The handler took the clock only to pass it along, so dropping it
                // costs nothing; the handler is DI-constructed, so no caller changes.
                : base(options, logger, encoder)
            {
                _SignInManager = signInManager;
            }

            protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                // skip authentication if endpoint has [AllowAnonymous] attribute
                var endpoint = Context.GetEndpoint();
                if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                    return AuthenticateResult.NoResult();

                if (!Request.Headers.ContainsKey("Authorization"))
                    return AuthenticateResult.Fail("Missing Authorization Header");


                IdentityUser user = null;
                SignInResult result = null;
            string userName = null;
                try
                {
                    var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                    userName = credentials[0].ToUpper();
                    var password = credentials[1];
                    result = await _SignInManager.CheckPasswordSignInAsync(new() { NormalizedEmail = userName }, password, lockoutOnFailure: false);
                }
                catch
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }

                if (result.IsNotAllowed)
                    return AuthenticateResult.Fail("Invalid Username or Password");

            user = await _SignInManager.UserManager.FindByNameAsync(userName);
                var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ),
                    };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
        }
    
}

