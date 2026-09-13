using Dapper.Contrib.Extensions;

namespace CPE.DapperIdentity.Core.Models
{
    [Table("IdentityRole")]
    public class CustomIdentityRole
    {
        [ExplicitKey]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}