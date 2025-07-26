using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DapperIdentity.Core.Models
{
    [Table("IdentityUserClaim")]
    public class CustomIdentityUserClaim
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string UserId{ get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

        public Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }

        public void InitializeFromClaim(Claim claim)
        {
            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }

        //public override string Id { get => base.Id; set => base.Id = value; }
        //public protected override string Id { get; set; }
    }
}
