using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperIdentity.JWT.Models;
public class AuthRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

