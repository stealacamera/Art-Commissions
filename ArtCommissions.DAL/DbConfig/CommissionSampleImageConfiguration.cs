using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class CommissionSampleImageConfiguration : IEntityTypeConfiguration<CommissionSampleImage>
    {
        public void Configure(EntityTypeBuilder<CommissionSampleImage> builder)
        {
            builder.ToTable("CommissionSampleImages");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.ImagePath)
                   .HasMaxLength(40)
                   .IsRequired();

            builder.HasIndex(e => e.ImagePath)
                   .IsUnique();

            builder.HasOne<Commission>()
                   .WithMany()
                   .HasForeignKey(e => e.CommissionId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
