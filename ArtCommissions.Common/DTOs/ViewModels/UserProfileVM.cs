namespace ArtCommissions.Common.DTOs.ViewModels;

public class UserProfileVM
{
    public AppUser User { get; set; }
    public PaginatedList<Commission> Commissions { get; set; }
    public decimal? OverallReviewScore { get; set; }
}
