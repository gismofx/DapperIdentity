using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


//using JwtRoleAuthentication.Models;

using IdentityRole = DapperIdentity.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;

namespace DapperIdentity.Controllers.JWT;
public class TokenService
{
    // Specify how long until the token expires
    private const int ExpirationMinutes = 30;
    private readonly ILogger<TokenService> _logger;

    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
    }

    public string CreateToken(IdentityUser user)
    {
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        //var token = CreateJwtToken(
        //    CreateClaims(user),
        //    CreateSigningCredentials(),
        //    expiration
        //);
        var tokenHandler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();


        var d = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(CreateClaims(user)),
            SigningCredentials = CreateSigningCredentials(),
            Expires = expiration,
        };
        _logger.LogInformation("JWT Token created");

        return tokenHandler.CreateToken(d);// WriteToken(token);
    }



    private List<Claim> CreateClaims(IdentityUser user)
    {
        //var jwtSub = new ConfigurationBuilder()
        //var jwtSub = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["JwtRegisteredClaimNamesSub"];
        //IConfigurationBuilder builder = new ConfigurationBuilder()
        //
        var jwtSub = "345h098bb8reberbwr4vvb8945";
        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwtSub),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                //new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            return claims;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var k = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my_secret_key"));

        //var symmetricSecurityKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["SymmetricSecurityKey"];
        var symmetricSecurityKey = "fvh8456477hth44j6wfds98bq9hp8bqh9ubq9gjig3qr0[94vj5";

        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(symmetricSecurityKey)
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}
