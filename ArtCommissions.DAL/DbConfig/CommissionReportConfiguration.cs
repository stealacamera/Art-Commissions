using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ArtCommissions.DAL.Entities;
using ArtCommissions.Common.Enums;

namespace ArtCommissions.DAL.DbConfig;

internal class CommissionReportConfiguration : IEntityTypeConfiguration<CommissionReport>
{
    public void Configure(EntityTypeBuilder<CommissionReport> builder)
    {
        builder.ToTable("CommissionReports");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
               .IsRequired()
               .HasDefaultValue(ReportStatus.PENDING);

        builder.Property(e => e.Reason)
               .IsRequired()
               .HasMaxLength(1000);

        builder.HasOne<Commission>()
               .WithMany()
               .HasForeignKey(e => e.ReportedCommissionId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}