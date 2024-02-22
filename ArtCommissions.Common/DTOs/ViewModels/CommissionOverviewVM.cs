namespace ArtCommissions.Common.DTOs.ViewModels;

public class CommissionOverviewVM
{
    public Commission Commission { get; set; } = null!;
    public List<Tag> Tags { get; set; } = new List<Tag>();
    public decimal? OverallReviewsScore { get; set; }
    public PaginatedList<Review> Reviews { get; set; } = new PaginatedList<Review>();
}
