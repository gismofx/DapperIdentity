using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperIdentity.Controllers.JWT;

public class AuthResponse
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}
