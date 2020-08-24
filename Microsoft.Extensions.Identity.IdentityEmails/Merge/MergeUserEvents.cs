using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.Merge
{
    public interface IMergeUserEvents<TUser>
    {
        Task MergeUser(TUser primaryUser, TUser mergedUser);
    }

    public class DefaultMergeUserEvents : IMergeUserEvents<IdentityUser>
    {
        public Task MergeUser(IdentityUser primaryUser, IdentityUser mergedUser)
        {
            return Task.CompletedTask;
        }
    }
}