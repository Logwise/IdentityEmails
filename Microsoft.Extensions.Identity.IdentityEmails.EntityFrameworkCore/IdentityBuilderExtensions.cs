using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddIdentityEmail<TUser, TContext>(this IdentityBuilder builder, Action<IdentityEmailsOptions> options = null)
           where TUser : IdentityUser<string>
           where TContext : DbContext
        {
            return builder.AddIdentityEmail<TUser, IdentityRole, AspNetCore.Identity.EntityFrameworkCore.IdentityEmail, string, TContext>(options);
        }

        public static IdentityBuilder AddIdentityEmail<TUser, TRole, TEmail, TKey, TContext>(this IdentityBuilder builder, Action<IdentityEmailsOptions> options = null)
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TEmail : AspNetCore.Identity.EntityFrameworkCore.IdentityEmail<TKey>, new()
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            builder.AddIdentityEmail<TUser>(options);
            builder.AddUserStore<IdentityEmailStore<TUser, TRole, TEmail, TContext, TKey>>();

            return builder;
        }
    }
}