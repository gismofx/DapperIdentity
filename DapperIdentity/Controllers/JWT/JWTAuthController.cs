using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using JwtRoleAuthentication.Data;
//using JwtRoleAuthentication.Enums;
//using JwtRoleAuthentication.Models;
//using JwtRoleAuthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityRole = DapperIdentity.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;

//https://markjames.dev/blog/jwt-authorization-asp-net-core
namespace DapperIdentity.Controllers.JWT;

[ApiController]
[Route("/api/[controller]")]
internal class JWTAuthController : ControllerBase
{

    private readonly UserManager<IdentityUser> _userManager;
    //private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;

    public JWTAuthController(UserManager<IdentityUser> userManager, TokenService tokenService, ILogger<JWTAuthController> logger) //ApplicationDbContext context
    {
        _userManager = userManager;
        //_context = context;
        _tokenService = tokenService;
    }


    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userManager.CreateAsync(
            new IdentityUser { UserName = request.Username, Email = request.Email }, //, Role = Role.User },
            request.Password!
        );

        if (result.Succeeded)
        {
            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email,/* role = request.Role */}, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }


    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.FindByEmailAsync(request.Email!);
        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password!);
        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }

        var userInDb = managedUser; //why search again?//_userManager.Users.FirstOrDefault(u=>u.Email==) //_context.Users.FirstOrDefault(u => u.Email == request.Email);

        if (userInDb is null)
        {
            return Unauthorized();
        }

        var accessToken = _tokenService.CreateToken(userInDb);
        //await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Username = userInDb.UserName,
            Email = userInDb.Email,
            Token = accessToken,
        });
    }
}
