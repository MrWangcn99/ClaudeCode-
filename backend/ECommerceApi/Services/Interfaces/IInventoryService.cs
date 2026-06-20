using ECommerceApi.DTOs;

namespace ECommerceApi.Services.Interfaces;

public interface IInventoryService
{
    Task<List<ProductDto>> GetAllInventoryAsync();
    Task<ProductDto?> UpdateStockAsync(int productId, InventoryUpdateDto dto);
}
