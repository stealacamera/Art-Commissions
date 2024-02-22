using ArtCommissions.Common.Enums;

namespace ArtCommissions.DAL.Entities;

public class Invoice : BaseEntity
{
    public int OrderId { get; set; }
    public decimal Price { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PayedAt { get; set;}
}
