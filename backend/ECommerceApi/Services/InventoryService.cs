using ECommerceApi.DTOs;
using ECommerceApi.Repositories.Interfaces;
using ECommerceApi.Services.Interfaces;

namespace ECommerceApi.Services;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;

    public InventoryService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetAllInventoryAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }

    public async Task<ProductDto?> UpdateStockAsync(int productId, InventoryUpdateDto dto)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null) return null;

        product.Stock = dto.Stock;
        await _productRepository.UpdateAsync(product);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
