using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs;

public class UpdateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
