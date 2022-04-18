using System;
using Microsoft.AspNetCore.Mvc;

//ref https://codeburst.io/adding-basic-authentication-to-an-asp-net-core-web-api-project-5439c4cf78ee

namespace DapperIdentity.Controllers.DigestAuth
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DigestAuthAttribute : TypeFilterAttribute
    {
        public DigestAuthAttribute(string realm = @"My Realm") : base(typeof(DigestAuthFilter))
        {
            Arguments = new object[] { realm };
        }
    }
}