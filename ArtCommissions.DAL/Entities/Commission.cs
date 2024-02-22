namespace ArtCommissions.DAL.Entities;

public class Commission : BaseEntity
{
    public int OwnerId { get; set; }
    internal AppUser Owner { get; set; }
    public bool IsClosed { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }

    private decimal _minPrice;
    public decimal MinPrice
    {
        get => _minPrice;
        set
        {
            if (value <= 0) throw new Exception("The minimum price cannot be a nonpositive value");
            _minPrice = value;
        }
    }

    internal IEnumerable<CommissionTag> Tags { get; set; } = new List<CommissionTag>();
}
