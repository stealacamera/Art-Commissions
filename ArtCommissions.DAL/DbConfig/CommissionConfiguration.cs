using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class CommissionConfiguration : IEntityTypeConfiguration<Commission>
    {
        public void Configure(EntityTypeBuilder<Commission> builder)
        {
            builder.ToTable("Commissions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.IsClosed)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(80);

            builder.Property(e => e.Description)
                   .HasMaxLength(2000);

            builder.Property(e => e.MinPrice)
                   .IsRequired()
                   .HasPrecision(10, 4);

            builder.HasOne(e => e.Owner)
                   .WithMany()
                   .HasForeignKey(e => e.OwnerId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
