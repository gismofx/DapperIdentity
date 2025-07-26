using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperIdentity.Core.Models
{
    public interface IAppSettings
    {
        string ApplicationName { get; }
    }
}
