using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.Repositories.Interfaces;

namespace ECommerceApi.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
        => await _context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products.FindAsync(id);

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Products.AnyAsync(p => p.Id == id);
}
