using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Data;
//using JwtRoleAuthentication.Enums;

namespace DapperIdentity.JWT.Models;

public class RegistrationRequest
{
    [Required]
    public string? Email { get; set; }

    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }

    public string? Id { get; set; }
    
    //public Role Role { get; set; }
}
