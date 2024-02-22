using ArtCommissions.Common.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class Commission
{
    public int Id { get; set; }
    public AppUser Owner { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool IsClosed { get; set; }
    public string? Description { get; set; }
    public decimal MinPrice { get; set; }
    public List<CommissionSampleImage> SampleImages { get; set; } = new List<CommissionSampleImage>();
}

public abstract class CommissionUpsertRequestModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "Title has to be 4-80 characters long")]
    public string Title { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters")]
    public string? Description { get; set; }

    public bool IsClosed { get; set; } = false;

    [Required(ErrorMessage = "Price is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Price has to be a positive value")]
    public decimal MinPrice { get; set; }
}

public class CommissionAddRequestModel : CommissionUpsertRequestModel
{
    [Required(ErrorMessage = "Sample images are required")]
    [FormFileExtensions(".png, .jpg, .jpeg, .gif")]
    public IFormFileCollection SampleImages { get; set; } = null!;

    public List<int> Tags { get; set; } = new ();
}

public class CommissionEditRequestModel : CommissionUpsertRequestModel
{
    [FormFileExtensions(".png, .jpg, .jpeg, .gif")]
    public IFormFileCollection? SampleImages { get; set; }

    public List<CommissionSampleImageUpdateRequestModel> ExistingSampleImages { get; set; } = new ();

    public List<int> Tags { get; set; } = new ();
}