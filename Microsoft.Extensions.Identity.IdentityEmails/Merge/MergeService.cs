using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Microsoft.AspNetCore.Identity.Merge
{
    public interface IMergeService<TUser>
    {
        Task MergeUsers(TUser currentUser, TUser user);
    }

    public class MergeService<TUser> : IMergeService<TUser>
        where TUser : class
    {
        private readonly IdentityEmailUserManager<TUser> _userManager;
        private readonly IMergeUserEvents<TUser> _mergeUserEvents;
        private readonly IOptions<IdentityEmailsOptions> _options;

        public MergeService(
            IdentityEmailUserManager<TUser> userManager,
            IMergeUserEvents<TUser> mergeUserEvents,
            IOptions<IdentityEmailsOptions> options
        )
        {
            _userManager = userManager;
            _mergeUserEvents = mergeUserEvents;
            _options = options;
        }

        public async Task MergeUsers(TUser currentUser, TUser user)
        {
            var observer = new IdentityResultObserver();

            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.DefaultTimeout
                },
                TransactionScopeAsyncFlowOption.Enabled)
            )
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userLogins = await _userManager.GetLoginsEmailInfoAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var emails = _userManager.GetEmails(user);

                var existingRoles = await _userManager.GetRolesAsync(currentUser);
                var missingRoles = roles.Where(role => !existingRoles.Contains(role));
                await observer.Observe(async () => await _userManager.AddToRolesAsync(currentUser, missingRoles));

                foreach (var login in userLogins)
                {
                    await observer.Observe(async () =>
                    {
                        await _userManager.RemoveLoginAsync(user, login.UserLoginInfo.LoginProvider, login.UserLoginInfo.ProviderKey);
                        return await _userManager.AddLoginAsync(currentUser, login.UserLoginInfo, login.Email);
                    });                    
                }

                foreach (var email in emails.Where(e => !userLogins.Any(l => l.Email.Equals(e.Email, StringComparison.InvariantCultureIgnoreCase))))
                {
                    await observer.Observe(async () => await _userManager.AddEmailAsync(currentUser, email.Email, email.UserLoginInfo));
                }

                if (_options.Value.MergeUnconfirmedEmails || await _userManager.IsEmailConfirmedAsync(user))
                {
                    var email = await _userManager.GetEmailAsync(user);
                    await observer.Observe(async () => await _userManager.AddEmailAsync(currentUser, email));
                }

                var existingClaims = await _userManager.GetClaimsAsync(currentUser);
                var missingClaims = claims.Where(c =>
                    !existingClaims.Any(ec =>
                        c.Type.Equals(ec.Type, StringComparison.InvariantCultureIgnoreCase) &&
                        c.Value.Equals(ec.Value, StringComparison.InvariantCultureIgnoreCase)
                ));
                await observer.Observe(async () => await _userManager.AddClaimsAsync(currentUser, missingClaims));

                await observer.Observe(async () => await _userManager.DeleteAsync(user));

                await _mergeUserEvents.MergeUser(currentUser, user);

                transaction.Complete();
            }
        }
    }
}