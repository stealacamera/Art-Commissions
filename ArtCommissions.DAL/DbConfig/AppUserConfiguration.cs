using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("AppUsers");

            builder.Property(e => e.StripeCustomerId)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(e => e.StripeAccountId)
                   .HasMaxLength(200)
                   .IsRequired();
        }
    }
}
