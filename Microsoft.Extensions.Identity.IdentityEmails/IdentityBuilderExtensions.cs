using Microsoft.AspNetCore.Identity;
using System;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddIdentityEmail<TUser>(this IdentityBuilder builder, Action<IdentityEmailsOptions> options = null)
           where TUser : class
        {
            if (options != null)
            {
                builder.Services.Configure(options);
            }

            return builder.AddUserManager<IdentityEmailUserManager<TUser>>();
        }
    }
}