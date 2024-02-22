using ArtCommissions.Common.Enums;
using ArtCommissions.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtCommissions.DAL.DbConfig
{
    internal class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Price)
                   .IsRequired()
                   .HasPrecision(18, 4);

            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(200);
            
            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(1500);

            builder.Property(e => e.Status)
                   .IsRequired()
                   .HasDefaultValue(InvoiceStatus.WAITING_PAYMENT);

            builder.Property(e => e.CreatedAt)
                   .IsRequired();

            builder.HasOne<Order>()
                   .WithMany()
                   .HasForeignKey(e => e.OrderId)
                   .IsRequired();

            builder.Property(e => e.OrderId)
                   .HasDefaultValue(0);
        }
    }
}
