using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperIdentity.Controllers.JWT;
public class AuthRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

