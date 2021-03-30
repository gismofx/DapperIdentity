using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;

namespace DapperIdentity.Controllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        //private readonly ILogger<LoginModel> _logger;

        public IdentityController(SignInManager<IdentityUser> signInManager,
                                UserManager<IdentityUser> userManager)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            //_logger = logger;
        }

        /// <summary>
        /// This is only for testing
        /// </summary>
        /// <returns></returns>
        //public async Task<ActionResult> Index()
        //{
        //    return Redirect("/");
        //}

        //[Route("~/Identity/Login")]
        //Todo: Handle Incorrect Password or just redirect back to login page
        [HttpPost]
        public async Task<ActionResult> Login([FromForm] string name, [FromForm] string password)
        {
            /*
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, name)
                }, "auth");
            ClaimsPrincipal claims = new ClaimsPrincipal(claimsIdentity);
            */
            var result = await _SignInManager.PasswordSignInAsync(name, password, true, false);
            //await HttpContext.SignInAsync(claims);
            if (result.Succeeded)
            {
                return Redirect("/");
            }
            else
            {
                return null;
            }

        }

        //Identity/Logout
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<ActionResult> Logout()
        {
            if (_SignInManager.IsSignedIn(User))
            {
                await _SignInManager.SignOutAsync();
            }
            return Redirect("/");
        }
    }
}
