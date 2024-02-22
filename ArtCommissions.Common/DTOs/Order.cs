using ArtCommissions.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class Order
{
    public int Id { get; set; }
    public AppUser Client { get; set; } = null!;
    public Commission Commission { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class OrderAddRequestModel
{

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(1500, ErrorMessage = "Description cannot be longer than 1500 characters")]
    public string Description { get; set; } = null!;
}