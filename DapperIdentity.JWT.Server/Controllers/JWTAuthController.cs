using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

using IdentityRole = DapperIdentity.Core.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;
using DapperIdentity.JWT.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;



//https://markjames.dev/blog/jwt-authorization-asp-net-core
//https://www.c-sharpcorner.com/article/jwt-authentication-with-refresh-tokens-in-net-6-0/
namespace DapperIdentity.JWT.Server.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
//[Route("/[controller]/[action]")]
public class JWTAuthController : ControllerBase
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

    [Authorize]
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
            var user = await _userManager.FindByEmailAsync(request.Email);
            request.Id = user.Id;
            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email,/* role = request.Role */}, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }

    [AllowAnonymous]
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

        var roles = await _userManager.GetRolesAsync(userInDb);
        var accessToken = await _tokenService.CreateToken(userInDb, roles);

        var refreshTokenInfo = _tokenService.GenerateRefreshToken();
        userInDb.RefreshToken = refreshTokenInfo.token;
        userInDb.RefreshTokenExpireTime = refreshTokenInfo.expiration;
        await _userManager.UpdateAsync(userInDb);//persist the refresh token and expiration in database each time we login

        return Ok(new AuthResponse
        {
            Username = userInDb.UserName,
            Email = userInDb.Email,
            Token = accessToken,
            RefreshToken = userInDb.RefreshToken
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenDto tokenDto)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        /*if (tokenDto is null)
        {
            return BadRequest(new AuthResponseDto { IsAuthSuccessful = false, ErrorMessage = "Invalid client request" });
        }
        */
        
        var principal = _tokenService.GetPrincipalFromExpiredToken2(tokenDto.Token);
        if (principal is null)
        {
            return BadRequest("Invalid access token or refresh token");
        }

        var username = principal.Identity.Name;
        var user = await _userManager.FindByNameAsync(username);// EmailAsync(username);
        if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpireTime <= DateTime.Now)
            return BadRequest("Invalid access token or refresh token");//return BadRequest(new AuthResponseDto { IsAuthSuccessful = false, ErrorMessage = "Invalid client request" });
        
        var roles = await _userManager.GetRolesAsync(user);
        
        //Create a new Token
        var token = await _tokenService.CreateToken(user, roles);

        //Refresh Token
        var tokenInfo = _tokenService.GenerateRefreshToken();
        user.RefreshToken = tokenInfo.token;
        user.RefreshTokenExpireTime = tokenInfo.expiration;
        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponse() { Email = user.Email, RefreshToken = user.RefreshToken, Token = token, Username = user.UserName});
    }

    //public async Task

}
