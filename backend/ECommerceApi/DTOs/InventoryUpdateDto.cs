using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs;

public class InventoryUpdateDto
{
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}
