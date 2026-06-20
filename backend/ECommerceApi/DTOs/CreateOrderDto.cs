using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs;

public class CreateOrderDto
{
    [Required, MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
