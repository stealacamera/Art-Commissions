using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ArtCommissions.DAL.Entities;
using ArtCommissions.Common.Enums;

namespace ArtCommissions.DAL.DbConfig;

internal class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
{
    public void Configure(EntityTypeBuilder<UserReport> builder)
    {
        builder.ToTable("UserReports");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
               .IsRequired()
               .HasDefaultValue(ReportStatus.PENDING);

        builder.Property(e => e.Reason)
               .IsRequired()
               .HasMaxLength(1000);

        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(e => e.ReportedUserId)
               .IsRequired();
    }
}