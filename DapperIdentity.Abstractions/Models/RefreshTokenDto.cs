using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPE.DapperIdentity.Abstractions.Models
{
    public class RefreshTokenDto
    {
            public string Token { get; set; }
            public string RefreshToken { get; set; }
    }
}
