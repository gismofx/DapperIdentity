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
using IdentityRole = DapperIdentity.Core.Models.CustomIdentityRole;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;
using System.Security.Cryptography;
//using System.IdentityModel.Tokens.Jwt; Used directly below

namespace DapperIdentity.JWT.Server;
public class TokenService
{

    private readonly ILogger<TokenService> _logger;

    private JWTSettings _JwtSettings;

    private IClaimsService _ClaimsService;



    public TokenService(ILogger<TokenService> logger, IConfiguration configuration, IClaimsService claimsService= null)
    {
        _logger = logger;
        _JwtSettings = new JWTSettings(configuration);
        _ClaimsService = claimsService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public async ValueTask<string> CreateToken(IdentityUser user, IEnumerable<string> roles)
    {
        var expiration = DateTime.UtcNow + _JwtSettings.ExpirationTime; //AddMinutes(ExpirationMinutes);
        var tokenHandler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();

        var d = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(await CreateClaims(user, roles)),
            SigningCredentials = CreateSigningCredentials(),
            Expires = expiration,
        };
        _logger.LogInformation("JWT Token created");

        return tokenHandler.CreateToken(d);// WriteToken(token);
    }



    private async ValueTask<List<Claim>> CreateClaims(IdentityUser user, IEnumerable<string> usersRoles)
    {

        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),//user is the "subject" so we store userid here
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _JwtSettings.ValidIssuer), 
                new Claim(JwtRegisteredClaimNames.Aud, _JwtSettings.ValidAudience),
                //new Claim(ClaimTypes.NameIdentifier, user.Id),//username does not work when enables.
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            foreach (var role in usersRoles)
            {
                //claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("roles", role));
            }

            if (_ClaimsService is not null) 
            {
                var userClaims = await _ClaimsService.ClaimsToAdd(user);
                claims.AddRange(userClaims);
            }

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
        var symmetricSecurityKey = _JwtSettings.SymmetricSecurityKey;

        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(symmetricSecurityKey)
            ),
            SecurityAlgorithms.HmacSha256
        );
    }

    /// <summary>
    /// Generates a random token and new expiration date for the refresh token
    /// </summary>
    /// <returns></returns>
    public (string token, DateTime expiration) GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return (Convert.ToBase64String(randomNumber), DateTime.Now + _JwtSettings.RefreshTokenLife);
        }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_JwtSettings.SymmetricSecurityKey)),
            ValidateLifetime = false,
            ValidIssuer = _JwtSettings.ValidIssuer,
            ValidAudience = _JwtSettings.ValidAudience,
            ValidAudiences = new[] {_JwtSettings.ValidAudience}
        };
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        var jwtSecurityToken = securityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken2(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.SymmetricSecurityKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;

    }
    /// <summary>
    /// Store JWT settings fron configuration
    /*
       "JwtTokenSettings": {
    "ValidIssuer": "ExampleIssuer",
    "ValidAudience": "ValidAudience",
    "SymmetricSecurityKey": "fvh8456477hth44j6wfds98bq9hp8bqh9ubq9gjig3qr0[94vj5",
    "JwtRegisteredClaimNamesSub": "345h098bb8reberbwr4vvb8945",
    "JwtExpireSeconds": 900,
    "RefreshTokenLifeDays": 4 
    }
    */
    /// </summary>
    private record JWTSettings
    {

        private IConfiguration _Configuration;


        public JWTSettings(IConfiguration config)
        {
            _Configuration = config.GetRequiredSection("JwtTokenSettings");
        }

        public string ValidIssuer => _Configuration["ValidIssuer"]!;

        public string ValidAudience => _Configuration["ValidAudience"]!;

        public string SymmetricSecurityKey => _Configuration["SymmetricSecurityKey"]!;

        public string JwtRegisteredClaimNamesSub => _Configuration["JwtRegisteredClaimNamesSub"]!;

        public TimeSpan ExpirationTime => TimeSpan.FromSeconds(double.Parse(_Configuration["JwtExpireSeconds"]!));

        public TimeSpan RefreshTokenLife => TimeSpan.FromDays(double.Parse(_Configuration["RefreshTokenLifeDays"]!));

    }
}