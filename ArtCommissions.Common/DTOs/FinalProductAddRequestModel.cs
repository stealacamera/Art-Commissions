using ArtCommissions.Common.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class FinalImageAddRequestModel
{
    [ValidateNever]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "An image is required")]
    [FormFileExtensions(".png, .jpg, .jpeg, .gif")]
    public IFormFile FinalImage { get; set; } = null!;
}
