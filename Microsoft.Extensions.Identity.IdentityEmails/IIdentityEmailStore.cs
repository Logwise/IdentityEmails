using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.AspNetCore.Identity
{
    public interface IIdentityEmailStore<TUser>
    {
        Task AddEmailAsync(TUser user, string email, string loginProvider = null, string loginProviderKey = null, CancellationToken cancellationToken = default(CancellationToken));
        Task AddEmailAsync(TUser user, string email, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken));
        Task RemoveEmailAsync(TUser user, string loginProvider, string providerKey);
        Task<TUser> FindByEmailAsync(string email);
        IEnumerable<IdentityEmailInfo> GetEmails(TUser user);
        IEnumerable<IdentityEmailInfo> GetEmails(TUser user, string email);
    }
}