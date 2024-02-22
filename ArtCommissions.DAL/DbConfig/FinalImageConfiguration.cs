using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig;

internal class FinalImageConfiguration : IEntityTypeConfiguration<FinalImage>
{
    public void Configure(EntityTypeBuilder<FinalImage> builder)
    {
        builder.ToTable("FinalImages");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ImagePath)
               .HasMaxLength(40)
               .IsRequired();

        builder.HasIndex(e => e.ImagePath)
               .IsUnique();

        builder.HasOne<Order>()
               .WithOne()
               .IsRequired()
               .HasForeignKey<FinalImage>(e => e.OrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
