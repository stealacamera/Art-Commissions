using ArtCommissions.Common.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class Invoice
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public Order Order{ get; set; }
    public InvoiceStatus Status { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime PayedAt { get; set; }
}

public class InvoiceUpsertRequestModel
{
    [ValidateNever]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1500, ErrorMessage = "Description cannot be longer than 1500 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Price is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Price has to be a positive value")]
    public decimal Price { get; set; }

    [ValidateNever]
    public int OrderId { get; set; }
}