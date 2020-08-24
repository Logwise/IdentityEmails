using System;

namespace Microsoft.AspNetCore.Identity
{
    public class IdentityEmail : IdentityEmail<string> { }

    /// <summary>
    /// Represents a user email and its possible associated user login.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key of the user associated with this login.</typeparam>
    public class IdentityEmail<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets or sets the identifier for this user email.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// Gets or sets the primary key of the user associated with this email.
        /// </summary>
        public virtual TKey UserId { get; set; }

        /// <summary>
        /// Gets or sets the login provider associated with this email.
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the unique provider identifier associated with this email.
        /// </summary>
        public virtual string LoginProviderKey { get; set; }
    }
}