using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityEmails.Examples.WebApp.Areas.Identity.Pages.Account.Manage
{
    public class MergeConfirmationModel : PageModel
    {
        public IActionResult OnGetMergeConfirmation()
        {
            return Page();
        }
    }
}
