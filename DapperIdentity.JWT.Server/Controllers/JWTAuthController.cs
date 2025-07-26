using DapperIdentity.Core.Models;
using DapperIdentity.JWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ForgotPasswordRequest = DapperIdentity.JWT.Models.ForgotPasswordRequest;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;
using ResetPasswordRequest = DapperIdentity.JWT.Models.ResetPasswordRequest;




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

    private readonly IEmailSender _EmailSender;

    private readonly ILogger<JWTAuthController> _logger;

    private readonly IAppSettings _AppSettings;

    public JWTAuthController(UserManager<IdentityUser> userManager,
                             TokenService tokenService,
                             IEmailSender emailSender,
                             ILogger<JWTAuthController> logger,
                             IAppSettings appSettings)//Todo: Add options, IOptions<JWTControllerOptions> options) //ApplicationDbContext context
    {
        _userManager = userManager;
        //_context = context;
        _EmailSender = emailSender;
        _tokenService = tokenService;
        _logger = logger;
        _AppSettings = appSettings;
    }

    [Authorize]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userManager.CreateAsync(
            new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsEnabled = true
            },
            request.Password!
        );

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            request.Id = user.Id;
            request.Password = "";

            //add claims
            List<Claim> claims = new List<Claim>();
            foreach (var item in request.Claims)
            {
                claims.Add(new Claim(item.Key, item.Value));
            }
            await _userManager.AddClaimsAsync(user, claims);

            //send email
            await SendRegistrationEmail(user, $"{_AppSettings.ApplicationName}: Welcome! User Registration", $"Welcome! You have been registered to access {_AppSettings.ApplicationName}! Complete your registration by");

            //return
            return CreatedAtAction(nameof(Register), new { email = request.Email,/* role = request.Role */}, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }

    private async Task SendRegistrationEmail(IdentityUser user, string subject, string bodyPrefix)
    {

        var referrer = HttpContext.Request.Headers.Referer;

        // For more information on how to enable account confirmation and password reset please 
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = QueryHelpers.AddQueryString($"{referrer}Account/PasswordReset", "code", code);

        //Send Email With Callback URL to reset the password
        await _EmailSender.SendEmailAsync(
            user.Email,
            subject,
            $"{bodyPrefix} <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

    }


    /// <summary>
    /// Forgot Password Endpoint To Initiate a Password Reset
    /// </summary>
    /// <param name="forgotPasswordRequest"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("Forgot")]
    [Route("ForgotPassword")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(forgotPasswordRequest.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or is not confirmed
            return Ok();
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        try
        {
            await SendRegistrationEmail(user, $"{_AppSettings.ApplicationName}: Reset Password", "Please reset your password by");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in API endpoint 'ForgotPassword'");
        }

        return Ok();

    }

    /// <summary>
    /// This endpoints is called to reset the password
    /// </summary>
    /// <param name="passwordResetRequest"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("ResetPassword")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest passwordResetRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(passwordResetRequest.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var codeB = WebEncoders.Base64UrlDecode(passwordResetRequest.Code);

        var code = Encoding.UTF8.GetString(codeB);

        var result = await _userManager.ResetPasswordAsync(user, code, passwordResetRequest.Password);
        if (result.Succeeded)
        {
            await _EmailSender.SendEmailAsync(passwordResetRequest.Email, "Password Reset Successful", "Your password was successfully reset. If you did not change your password, please contact application administrator.");
            return Ok("Password Reset Successful");
            //return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            _logger.LogError($"Error in API 'ResetPassword: User:{passwordResetRequest.Email} {error.Code} - {error.Description}");
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return BadRequest(ModelState);
    }


    /// <summary>
    /// Login Method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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

        var username = principal.Identity!.Name; //do we need to null check on Identity?
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

        return Ok(new AuthResponse() { Email = user.Email, RefreshToken = user.RefreshToken, Token = token, Username = user.UserName });
    }

}
