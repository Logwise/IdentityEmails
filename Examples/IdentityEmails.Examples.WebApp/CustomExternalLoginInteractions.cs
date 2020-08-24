using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityEmails.Examples
{
    public class CustomExternalLoginInteractions
    {
        private readonly IdentityEmailUserManager<IdentityUser> _userManager;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public CustomExternalLoginInteractions(IdentityEmailUserManager<IdentityUser> userManager, IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _userManager = userManager;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public async Task<IdentityUser> TryConnectExternalLoginToExistingUser(string providerUserId, AuthenticateResult result)
        {
            var user = await FindUser(result);

            if (user != null)
            {
                // mark email as confirmed -- this is saved with the operation below
                user.EmailConfirmed = true;
                await TryConnectExternalLoginToUser(user, providerUserId, result);
                return user;
            }

            return null;
        }

        public async Task TryConnectExternalLoginToUser(IdentityUser user, string providerUserId, AuthenticateResult result)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var email = result.Principal.Claims.FindEmail();
            var provider = result.Properties.Items["scheme"];
            var providerDisplayName = await _authenticationSchemeProvider.GetProviderDisplayName(provider);

            var identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, providerDisplayName), email);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
        }

        private async Task<IdentityUser> FindUser(AuthenticateResult result)
        {
            // For create flow when we have user-id present
            if (result.Properties.Items.TryGetValue("userId", out var userId) && userId != null)
            {
                return await _userManager.FindByIdAsync(userId);
            }
            
            // For login flow when we do not have a user-id
            var email = result.Principal.Claims.FindEmail();

            if (email != null)
            {
                return await _userManager.FindByEmailAsync(email);
            }

            return null;
        }
    }
}
