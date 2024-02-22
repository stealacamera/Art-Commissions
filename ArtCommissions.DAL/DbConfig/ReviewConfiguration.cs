using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(e => new { e.ReviewerId, e.CommissionId });

            builder.HasOne<AppUser>()
                   .WithMany()
                   .HasForeignKey(e => e.ReviewerId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Commission)
                   .WithMany()
                   .HasForeignKey(e => e.CommissionId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Rating)
                   .IsRequired();

            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(80);

            builder.Property(e => e.Description)
                   .HasMaxLength(500);
        }
    }
}
