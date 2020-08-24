using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Identity
{
    public class IdentityEmailUserManager<TUser> : UserManager<TUser>
        where TUser : class
    {
        public IdentityEmailUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators, IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        { }

        public async Task<IdentityResult> AddEmailAsync(TUser user, string email, UserLoginInfo login = null)
        {
            ThrowIfDisposed();

            var emailStore = GetEmailsStore();
            await emailStore.AddEmailAsync(user, email, login, CancellationToken);

            return await UpdateUserAsync(user);
        }

        public async Task<IdentityResult> AddLoginAsync(TUser user, UserLoginInfo login, string email)
        {
            ThrowIfDisposed();

            var emailStore = GetEmailsStore();
            if (email != null)
            {
                await emailStore.AddEmailAsync(user, email, login, CancellationToken);
            }

            return await AddLoginAsync(user, login);
        }

        public IEnumerable<IdentityEmailInfo> GetEmails(TUser user)
        {
            ThrowIfDisposed();
            
            var emailStore = GetEmailsStore();
            var emails = emailStore.GetEmails(user);

            return emails;
        }

        public async Task<IEnumerable<IdentityEmailInfo>> GetLoginsEmailInfoAsync(TUser user)
        {
            ThrowIfDisposed();

            var logins = await GetLoginsAsync(user);
            var emailStore = GetEmailsStore();
            var emails = emailStore.GetEmails(user);

            var emailInfos = logins.Select(l => ToEmailInfo(l, emails));

            return emailInfos;
        }

        private IdentityEmailInfo ToEmailInfo(UserLoginInfo login, IEnumerable<IdentityEmailInfo> emails)
        {
            var matchingEmail = emails.FirstOrDefault(e => login.LoginProvider == e.UserLoginInfo?.LoginProvider && login.ProviderKey == e.UserLoginInfo?.ProviderKey);

            if (matchingEmail != null) return matchingEmail;

            return new IdentityEmailInfo
            {
                Email = null,
                UserLoginInfo = login
            };
        }

        public override async Task<TUser> FindByEmailAsync(string email)
        {
            var res = await base.FindByEmailAsync(email);

            if (res != null) return res;

            var emailStore = GetEmailsStore();
            var user = await emailStore.FindByEmailAsync(email);

            return user;
        }

        public override async Task<IdentityResult> RemoveLoginAsync(TUser user, string loginProvider, string providerKey)
        {
            ThrowIfDisposed();

            var emailStore = GetEmailsStore();
            await emailStore.RemoveEmailAsync(user, loginProvider, providerKey);

            return await base.RemoveLoginAsync(user, loginProvider, providerKey);
        }

        public override async Task<IdentityResult> ChangeEmailAsync(TUser user, string newEmail, string token)
        {
            ThrowIfDisposed();

            await AddCurrentUserEmailAsIdentityEmail(user);

            return await base.ChangeEmailAsync(user, newEmail, token);
        }

        public override async Task<IdentityResult> SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();

            await AddCurrentUserEmailAsIdentityEmail(user);

            return await base.SetEmailAsync(user, email);
        }

        private async Task AddCurrentUserEmailAsIdentityEmail(TUser user)
        {
            var currentEmail = await GetEmailAsync(user);
            var isCurrentEmailConfirmed = await IsEmailConfirmedAsync(user);
            var emailStore = GetEmailsStore();
            var identityEmails = emailStore.GetEmails(user, currentEmail);

            if (!identityEmails.Any() && isCurrentEmailConfirmed)
            {
                await emailStore.AddEmailAsync(user, currentEmail);
            }
        }

        private IIdentityEmailStore<TUser> GetEmailsStore()
        {
            return GetStore<IIdentityEmailStore<TUser>>();
        }

        private T GetStore<T>() where T : class
        {
            var cast = Store as T;
            if (cast == null)
            {
                throw new NotSupportedException();
            }
            return cast;
        }
    }
}