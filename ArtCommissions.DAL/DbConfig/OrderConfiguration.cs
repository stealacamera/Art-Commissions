using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(1500);

            builder.Property(e => e.Status)
                   .IsRequired()
                   .HasDefaultValue(OrderStatus.REQUEST);

            builder.HasOne(e => e.Commission)
                   .WithMany()
                   .HasForeignKey(e => e.CommissionId);

            builder.Property(e => e.CommissionId)
                   .HasDefaultValue(0);

            builder.HasOne<AppUser>()
                   .WithMany()
                   .HasForeignKey(e => e.ClientId);

            builder.Property(e => e.ClientId)
                   .HasDefaultValue(0);
        }
    }
}
