using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DapperIdentity.Models;
using IdentityRole = DapperIdentity.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

//https://jasonwatmore.com/post/2019/10/21/aspnet-core-3-basic-authentication-tutorial-with-example-api#authenticate-model-cs
namespace DapperIdentity.Controllers.BasicAuth
{
    [ApiController]
    [Route("[controller]")]
    public class BasicAuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly SignInManager<IdentityUser> _SignInManager;

        public BasicAuthController(UserManager<IdentityUser> userManager, 
                                               RoleManager<IdentityRole> roleManager,
                                               SignInManager<IdentityUser> signInManager)
        {
            _UserManager = userManager;
            _RoleManager = roleManager;
            _SignInManager = signInManager;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Spa()
        {
            return File("~/index.html", "text/html");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IActionResult> Login()//[FromBody] AuthenticateModel model)
        {
            var model = new AuthenticateModel() { Username = "asadf", Password = "asdf" };
            var result = await _SignInManager.PasswordSignInAsync(model.Username,model.Password,false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok(model);
            }
            else
            {
                return Ok(model);
                //return BadRequest(new { message = "Username or password is incorrect" });
            }
        }
        [Authorize]
        [Route("Test")]
        [HttpPost]
        public async Task<IActionResult> Test()
        {

            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }
        }


    }
    public class AuthenticateModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    //        [HttpPost("signup")]
    //        public async Task<IActionResult> SignUp(UserSignUpResource userSignUpResource)
    //        {
    //            //var user = _mapper.Map<UserSignUpResource, User>(userSignUpResource);

    //            var userCreateResult = await _UserManager.CreateAsync(user, userSignUpResource.Password);

    //            if (userCreateResult.Succeeded)
    //            {
    //                return Created(string.Empty, string.Empty);
    //            }

    //            return Problem(userCreateResult.Errors.First().Description, null, 500);
    //        }

    //        [HttpPost("SignIn")]
    //        public async Task<IActionResult> SignIn(UserLoginResource userLoginResource)
    //        {
    //            var user = _UserManager.Users.SingleOrDefault(u => u.UserName == userLoginResource.Email);
    //            if (user is null)
    //            {
    //                return NotFound("User not found");
    //            }

    //            var userSigninResult = await _UserManager.CheckPasswordAsync(user, userLoginResource.Password);

    //            if (userSigninResult)
    //            {
    //                return Ok();
    //            }

    //            return BadRequest("Email or password incorrect.");
    //        }

    //        [HttpPost("Roles")]
    //        public async Task<IActionResult> CreateRole(string roleName)
    //        {
    //            if (string.IsNullOrWhiteSpace(roleName))
    //            {
    //                return BadRequest("Role name should be provided.");
    //}

    //            var newRole = new Role
    //            {
    //                Name = roleName
    //            };

    //            var roleResult = await _RoleManager.CreateAsync(newRole);

    //            if (roleResult.Succeeded)
    //            {
    //                return Ok();
    //            }

    //            return Problem(roleResult.Errors.First().Description, null, 500);
    //        }

    //        [HttpPost("User/{userEmail}/Role")]
    //        public async Task<IActionResult> AddUserToRole(string userEmail, [FromBody] string roleName)
    //        {
    //            var user = _UserManager.Users.SingleOrDefault(u => u.UserName == userEmail);

    //            var result = await _UserManager.AddToRoleAsync(user, roleName);

    //            if (result.Succeeded)
    //            {
    //                return Ok();
    //            }

    //            return Problem(result.Errors.First().Description, null, 500);
    //        }

}
