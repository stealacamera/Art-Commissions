namespace ArtCommissions.Common.DTOs.ViewModels;

public class OrderOverviewVM
{
    public Order Order { get; set; } = null!;
    public PaginatedList<Invoice> Invoices { get; set; } = null!;
    public string? FinalImagePath { get; set; }
}
