using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            builder.ToTable("AppRoles");

            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

            builder.HasData(new AppRole
            {
                Id = 1,
                ConcurrencyStamp = "1",
                Name = "Admin",
                NormalizedName = "ADMIN"
            });

            builder.HasData(new AppRole
            {
                Id = 2,
                ConcurrencyStamp = "2",
                Name = "User",
                NormalizedName = "USER"
            });
        }
    }
}
