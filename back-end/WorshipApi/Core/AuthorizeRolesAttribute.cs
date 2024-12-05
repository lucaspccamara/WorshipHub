using Microsoft.AspNetCore.Authorization;
using System.Data;
using WorshipDomain.Enums;

namespace WorshipApi.Core
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params Role[] roles)
        {
            Roles = string.Join(",", roles.Select(r => r.ToString()));
        }
    }
}
