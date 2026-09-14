using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPE.DapperIdentity.Abstractions.Models;

public class AuthResponse
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }

    public string RefreshToken { get; set; }    
}
