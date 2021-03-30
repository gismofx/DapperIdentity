using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace DapperIdentity.Models
{
    [Table("IdentityUser")]
    public class CustomIdentityUser
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SecurityStamp { get; set; }

        [Write(false)]
        public List<CustomIdentityRole> Roles { get; set; }

    }
}
