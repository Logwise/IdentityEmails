using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityEmails.Examples.WebApp.Areas.Identity.Pages.Account.Manage
{
    public class EmailsModel : PageModel
    {
        private readonly IdentityEmailUserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public EmailsModel(
            IdentityEmailUserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IEnumerable<IdentityEmailInfo> Emails { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Emails = _userManager.GetEmails(user);

            return Page();
        }
    }
}
