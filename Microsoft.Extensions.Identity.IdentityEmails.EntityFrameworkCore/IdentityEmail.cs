using System;

namespace Microsoft.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityEmail : IdentityEmail<string> { }

    /// <summary>
    /// Represents a user email and its possible associated user login.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key of the user associated with this login.</typeparam>
    public class IdentityEmail<TKey> : Identity.IdentityEmail<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Navigation property for this email's possible connection to a user login.
        /// </summary>
        public virtual IdentityUserLogin<TKey> UserLogin { get; set; }
    }
}