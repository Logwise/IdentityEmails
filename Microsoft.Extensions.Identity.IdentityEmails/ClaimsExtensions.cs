using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Identity
{
    public static class ClaimsExtensions
    {
        public static string FindEmail(this IEnumerable<Claim> claims)
        {
            var emailClaim =
                claims.FirstOrDefault(x => x.Type == "email") ??                // standard
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Email) ??       // standard
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Upn) ??         // msft business
                claims.FirstOrDefault(x => x.Type == "preferred_username");     // msft live

            return emailClaim?.Value;
        }
    }
}