using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class Review
{
    public AppUser Reviewer { get; set; } = null!;
    public int Rating { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Commission Commission { get; set; }
}

public class ReviewAddRequestModel
{
    [ValidateNever]
    public int CommissionId { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(80, ErrorMessage = "Title cannot exceed 80 characters")]
    public string Title { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}