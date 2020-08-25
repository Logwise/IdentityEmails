using System;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Identity.EntityFrameworkCore
{
    public class IdentityEmailDbContext<TUser> : IdentityEmailDbContext<TUser, IdentityRole, IdentityEmail, string>
        where TUser : IdentityUser<string>
    {
        public IdentityEmailDbContext(DbContextOptions options) : base(options) { }
    }

    public class IdentityEmailDbContext<TUser, TRole, TEmail, TKey> : IdentityDbContext<TUser, TRole, TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TEmail : IdentityEmail<TKey>
        where TKey : IEquatable<TKey>
    {
        public DbSet<TEmail> Emails { get; set; }

        public IdentityEmailDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TUser>()
                .HasMany<TEmail>()
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TEmail>()
                .ToTable("AspNetUserEmails")
                .HasOne(e => e.UserLogin)
                .WithOne()
                .HasForeignKey<TEmail>(e => new { e.LoginProvider, e.LoginProviderKey });

            builder.Entity<TEmail>(b =>
            {
                b.Property(e => e.Email).HasMaxLength(256).IsRequired();
                b.HasIndex(e => e.Email);
            });
        }
    }
}