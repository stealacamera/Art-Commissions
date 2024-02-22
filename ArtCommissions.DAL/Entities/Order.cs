using ArtCommissions.Common.Enums;

namespace ArtCommissions.DAL.Entities;

public class Order : BaseEntity
{
    public int ClientId { get; set; }
    public int CommissionId { get; set; }
    internal Commission Commission { get; set; }
    public OrderStatus Status { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
}
