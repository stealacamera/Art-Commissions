using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class Tag
{
    [ValidateNever]
    public int Id { get; set; }

    [Required(ErrorMessage = "A name is required")]
    [StringLength(100, ErrorMessage = "The name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;
}
