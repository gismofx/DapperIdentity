using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityUser = DapperIdentity.Core.Models.CustomIdentityUser;

namespace DapperIdentity.JWT.Server
{
    public interface IClaimsService
    {
        Task<IEnumerable<Claim>> ClaimsToAdd(IdentityUser user);
    }

    //public class ClaimsService : IClaimsService
    //{
    //    private DbContext _context;

    //    public ClaimsService(DbContext context)
    //    {
    //        _context = context;
    //    }

    //    public Task AddClaims(Token token)
    //    {
    //        var user = await _context.Users.FindAsync(token.UserId);
    //        if (user.IsAdmin)
    //            token.Claims.Add("IsAdmin", "true");
    //        // add other claims
    //    }
    //}

//    // Manual DI:
//    var claimsService = new ClaimsService(_context);
//    var tokenService = new TokenService(claimsService);

//    // Container DI:
//    services.AddScoped<IClaimsService, ClaimsService>();
//services.AddScoped<ITokenService, TokenService>();
}
