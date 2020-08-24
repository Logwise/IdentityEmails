using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityEmails.Examples
{
    public static class AuthenticationSchemeProviderExtensions
    {
        public static async Task<string> GetProviderDisplayName(this IAuthenticationSchemeProvider schemeProvider, string provider)
        {
            var providerDisplayName = (await schemeProvider.GetAllSchemesAsync())
                .FirstOrDefault(scheme => scheme.Name == provider && !string.IsNullOrEmpty(scheme.DisplayName))?.DisplayName ?? provider;

            return providerDisplayName;
        }
    }
}
