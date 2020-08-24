using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Merge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityEmails.Examples.WebApp.Areas.Identity.Pages.Account.Manage
{
    public class MergeModel : PageModel
    {
        private readonly IdentityEmailUserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IMergeService<IdentityUser> _mergeService;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public MergeModel(
            IdentityEmailUserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IAuthenticationSchemeProvider schemeProvider,
            IMergeService<IdentityUser> mergeService,
            IAuthenticationSchemeProvider authenticationSchemeProvider
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _schemeProvider = schemeProvider;
            _mergeService = mergeService;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        [BindProperty]
        public LoginViewModel LoginModel { get; set; }
        public IEnumerable<ExternalProviderButtonModel> ExternalProvidersModel;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            await GetMergeLoginViewModel();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(LoginModel.Username);

                if (user != null && await _userManager.CheckPasswordAsync(user, LoginModel.Password))
                {
                    return await HandleMerge(user, Url.Page("MergeConfirmation"));
                }

                ModelState.AddModelError(string.Empty, "Invalid credentials");
            }

            await GetMergeLoginViewModel();
            return Page();
        }

        public IActionResult OnGetChallenge(string provider)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Page(
                    "Merge",
                    pageHandler: "Callback"
                ),
                Items =
                    {
                        { "returnUrl", Url.Page("MergeConfirmation") },
                        { "scheme", provider }
                    }
            };

            return Challenge(props, provider);
        }
        
        public async Task<IActionResult> OnGetCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
            var returnUrl = result.Properties.Items["returnUrl"];

            if (user == null)
            {
                var email = result.Principal.Claims.FindEmail();

                if (email != null && (user = await _userManager.FindByEmailAsync(email)) == null)
                {
                    // no user found, just add the login to the current user
                    var currentUser = await _userManager.GetUserAsync(User);
                    await TryConnectExternalLoginToUser(currentUser, providerUserId, result);

                    return ReturnToUrl(returnUrl);
                }
            }

            // either we have:
            // * identified a user with the remote login or
            // * the remote login has an email that matches a local user record or
            // * we have not found any matching user because missing email claim
            return await HandleMerge(user, returnUrl);
        }

        private async Task<IActionResult> HandleMerge(IdentityUser user, string returnUrl)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (user == null || currentUser == null)
            {
                StatusMessage = "An error occured.";
                return RedirectToPage("Merge");
            }

            if (user.Id == currentUser.Id)
            {
                StatusMessage = "That login is already associated with this account!";
                return RedirectToPage("Merge");
            }

            await _mergeService.MergeUsers(currentUser, user);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var updatedUser = await _userManager.GetUserAsync(User);
            await _signInManager.SignInAsync(updatedUser, isPersistent: false);

            return ReturnToUrl(returnUrl);
        }

        private async Task<(IdentityUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst("sub") ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
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

        private async Task GetMergeLoginViewModel()
        {
            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            LoginModel = new LoginViewModel()
            {
                ExternalProviders = providers.ToList()
            };

            ExternalProvidersModel = LoginModel.VisibleExternalProviders.Select(provider => new ExternalProviderButtonModel(
                provider.AuthenticationScheme,
                provider.DisplayName,
                "Continue with",
                Url.Page(
                    "Merge",
                    pageHandler: "Challenge",
                    values: new Dictionary<string, string>() {
                        { "provider", provider.AuthenticationScheme }
                    }
                )
            ));
        }

        private IActionResult ReturnToUrl(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }
    }

    public class LoginViewModel : LoginInputModel
    {
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));
    }

    public class LoginInputModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
    }

    public class ExternalProvider
    {
        public string DisplayName { get; set; }
        public string AuthenticationScheme { get; set; }
    }

    public class ExternalProviderButtonModel
    {
        public string AuthenticationScheme { get; private set; }
        public string ProviderDisplayName { get; private set; }
        public string TextPrefix { get; private set; }
        public string Url { get; private set; }

        public ExternalProviderButtonModel(string scheme, string displayName, string textPrefix, string url)
        {
            AuthenticationScheme = scheme.ToLowerInvariant();
            ProviderDisplayName = displayName;
            TextPrefix = textPrefix;
            Url = url;
        }
    }
}
