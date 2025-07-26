using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DapperIdentity.Core.Models;
using IdentityRole = DapperIdentity.Core.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;
using System.Data;

namespace DapperIdentity.Cookies.Server.Controllers
{
    [ApiController]
    class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly JwtSettings _JwtSettings;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, JwtSettings jwtSettings)
        {
            _UserManager = userManager;
            _RoleManager = roleManager;
            _JwtSettings = jwtSettings;
        }
        /*
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(UserSignUpResource userSignUpResource)
        {
            //var user = _mapper.Map<UserSignUpResource, User>(userSignUpResource);

            var userCreateResult = await _UserManager.CreateAsync(user, userSignUpResource.Password);

            if (userCreateResult.Succeeded)
            {
                return Created(string.Empty, string.Empty);
            }

            return Problem(userCreateResult.Errors.First().Description, null, 500);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(UserLoginResource userLoginResource)
        {
            var user = _UserManager.Users.SingleOrDefault(u => u.UserName == userLoginResource.Email);
            if (user is null)
            {
                return NotFound("User not found");
            }

            var userSigninResult = await _UserManager.CheckPasswordAsync(user, userLoginResource.Password);

            if (userSigninResult)
            {
                return Ok();
            }

            return BadRequest("Email or password incorrect.");
        }

        [HttpPost("Roles")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name should be provided.");
}

            var newRole = new Role
            {
                Name = roleName
            };

            var roleResult = await _RoleManager.CreateAsync(newRole);

            if (roleResult.Succeeded)
            {
                return Ok();
            }

            return Problem(roleResult.Errors.First().Description, null, 500);
        }

        [HttpPost("User/{userEmail}/Role")]
        public async Task<IActionResult> AddUserToRole(string userEmail, [FromBody] string roleName)
        {
            var user = _UserManager.Users.SingleOrDefault(u => u.UserName == userEmail);

            var result = await _UserManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Ok();
            }

            return Problem(result.Errors.First().Description, null, 500);
        }
        */
    }
}
