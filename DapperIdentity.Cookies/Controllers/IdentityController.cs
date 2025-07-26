using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;

namespace DapperIdentity.Cookies.Server.Controllers
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

            //return a message?
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(password))
            {
                return Redirect("/"); 
            }

            var result = await _SignInManager.PasswordSignInAsync(name, password, true, false);
            //await HttpContext.SignInAsync(claims);
            if (result.Succeeded)
            {
                return Redirect("/");
            }
            if (result.RequiresTwoFactor)
            {
                //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                //_logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Redirect("/");
                //return Page();
            }
            else
            {
                return Redirect("/");
                //return null;
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
        
        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _UserManager.ConfirmEmailAsync(user, code);
            var StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return RedirectToPage("/");
        }

        /*
        [HttpPost]
        public async Task<ActionResult> CreateUser()
        {
            var user = new IdentityUser()
            {
                Email = "email@email.com",
                EmailConfirmed = true,
                FirstName = "user",
                UserName = "email@email.com"
            };
            var result = await _UserManager.CreateAsync(user,"StrongP@ssword1");
            if (result.Succeeded)
                return Ok("User Created");
            else
                return StatusCode(StatusCodes.Status500InternalServerError,$"Error Creating User: {String.Join(",",result.Errors.Select(x=>x.Code).ToList())}");
        }
        */
    }
}
