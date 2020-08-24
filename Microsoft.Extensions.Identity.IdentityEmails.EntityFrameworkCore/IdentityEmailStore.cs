using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityEmailStore<TUser, TRole, TEmail, TContext, TKey> : UserStore<TUser, TRole, TContext, TKey>, IIdentityEmailStore<TUser>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TEmail : IdentityEmail<TKey>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        private DbSet<TEmail> Emails { get { return Context.Set<TEmail>(); } }
        private DbSet<TUser> UsersSet { get { return Context.Set<TUser>(); } }
        private DbSet<IdentityUserLogin<TKey>> UserLogins { get { return Context.Set<IdentityUserLogin<TKey>>(); } }

        public IdentityEmailStore(TContext dbContext) : base(dbContext) { }

        public virtual async Task<TUser> FindByEmailAsync(string email)
        {
            var emails = Emails.Where(e => e.Email.Equals(email));

            if (emails.Any())
            {
                return await UsersSet.FindAsync(emails.First().UserId);
            }

            return null;
        }

        public virtual Task AddEmailAsync(TUser user, string email, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            return AddEmailAsync(user, email, login?.LoginProvider, login?.ProviderKey, cancellationToken);
        }

        public virtual async Task AddEmailAsync(TUser user, string email, string loginProvider = null, string loginProviderKey = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var existingEmail = FindIdentityEmail(user, email, loginProvider);
            if (existingEmail != null)
            {
                existingEmail.LoginProvider = loginProvider;
                existingEmail.LoginProviderKey = loginProviderKey;

                Emails.Update(existingEmail);
                return;
            }
            
            var identityEmail = CreateIdentityEmail(user, email, loginProvider, loginProviderKey);
            await Emails.AddAsync(identityEmail, cancellationToken);
        }

        private TEmail FindIdentityEmail(TUser user, string email, string loginProvider)
        {
            return Emails
                .Include(e => e.UserLogin)
                .FirstOrDefault(e => e.Email.Equals(email) && e.UserId.Equals(user.Id) && e.UserLogin.LoginProvider.Equals(loginProvider));
        }

        protected virtual TEmail CreateIdentityEmail(TUser user, string email, string loginProvider = null, string loginProviderKey = null)
        {
            return new TEmail()
            {
                UserId = user.Id,
                Email = email,
                LoginProvider = loginProvider,
                LoginProviderKey = loginProviderKey
            };
        }

        public virtual async Task RemoveEmailAsync(TUser user, string loginProvider, string loginProviderKey)
        {
            var identityEmail = await Find(loginProvider, loginProviderKey);

            if (identityEmail == null) return;

            Emails.Remove(identityEmail);
        }

        private async Task<TEmail> Find(string loginProvider, string loginProviderKey)
        {
            return await Emails.FirstOrDefaultAsync(e => e.LoginProvider == loginProvider && e.LoginProviderKey == loginProviderKey);
        }

        public virtual IEnumerable<IdentityEmailInfo> GetEmails(TUser user)
        {
            return Emails
                .Include(e => e.UserLogin)
                .Where(e => e.UserId.Equals(user.Id))
                .Select(ToEmailInfo)
                .ToList();
        }

        public virtual IEnumerable<IdentityEmailInfo> GetEmails(TUser user, string email)
        {
            return Emails
                .Include(e => e.UserLogin)
                .Where(e => e.UserId.Equals(user.Id) && e.Email == email)
                .Select(ToEmailInfo)
                .ToList();
        }

        protected IdentityEmailInfo ToEmailInfo(TEmail identityEmail)
        {
            return new IdentityEmailInfo
            {
                Email = identityEmail.Email,
                UserLoginInfo = identityEmail.UserLogin != null
                    ? new UserLoginInfo(identityEmail.UserLogin.LoginProvider, identityEmail.UserLogin.ProviderKey, identityEmail.UserLogin.ProviderDisplayName)
                    : null
            };
        }
    }
}