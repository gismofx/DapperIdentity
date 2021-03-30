using Dapper.Contrib.Extensions;

namespace DapperIdentity.Models
{
    public class CustomIdentityRole
    {
        [ExplicitKey]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}