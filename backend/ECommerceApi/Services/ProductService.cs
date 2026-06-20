using ECommerceApi.DTOs;
using ECommerceApi.Models;
using ECommerceApi.Repositories.Interfaces;
using ECommerceApi.Services.Interfaces;

namespace ECommerceApi.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _productRepository.AddAsync(product);
        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null) return null;

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.Description = dto.Description;
        product.ImageUrl = dto.ImageUrl;

        await _productRepository.UpdateAsync(product);
        return MapToDto(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        if (!await _productRepository.ExistsAsync(id)) return false;
        await _productRepository.DeleteAsync(id);
        return true;
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Stock = p.Stock,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
