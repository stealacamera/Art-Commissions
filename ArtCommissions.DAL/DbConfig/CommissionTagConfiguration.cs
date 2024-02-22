using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig;

internal class CommissionTagConfiguration : IEntityTypeConfiguration<CommissionTag>
{
    public void Configure(EntityTypeBuilder<CommissionTag> builder)
    {
        builder.ToTable("CommissionTags");

        builder.HasKey(e => new { e.TagId, e.CommissionId });

        builder.HasOne<Commission>()
               .WithMany(e => e.Tags)
               .HasForeignKey(e => e.CommissionId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tag>()
               .WithMany()
               .HasForeignKey(e => e.TagId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}