using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.Models.Enums;
using ECommerceApi.Repositories.Interfaces;

namespace ECommerceApi.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllAsync(OrderStatus? statusFilter = null)
    {
        var query = _context.Orders.AsNoTracking();
        if (statusFilter.HasValue)
            query = query.Where(o => o.Status == statusFilter.Value);
        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
        => await _context.Orders.FindAsync(id);

    public async Task<Order?> GetByIdWithItemsAsync(int id)
        => await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}
