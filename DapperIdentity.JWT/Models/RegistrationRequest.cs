using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
//using JwtRoleAuthentication.Enums;

namespace DapperIdentity.JWT.Models;

public class RegistrationRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    /// <summary>
    /// User's Id. If none is supplied, one will be created and returned
    /// </summary>
    public string? Id { get; set; }

    //public IEnumerable<Claim> Claims { get; set; } = Enumerable.Empty<Claim>();
    public Dictionary<string, string> Claims { get; set; } = new Dictionary<string,string>();

    //public Role Role { get; set; }
}
