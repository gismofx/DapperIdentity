using System;
using Microsoft.AspNetCore.Mvc;

//ref https://codeburst.io/adding-basic-authentication-to-an-asp-net-core-web-api-project-5439c4cf78ee

namespace DapperIdentity.Controllers.BasicAuth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthAttribute : TypeFilterAttribute
    {
        public BasicAuthAttribute(string realm = @"My Realm") : base(typeof(BasicAuthFilter))
        {
            Arguments = new object[] { realm };
        }
    }
}