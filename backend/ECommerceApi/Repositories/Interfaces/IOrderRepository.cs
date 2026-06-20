using ECommerceApi.Models;
using ECommerceApi.Models.Enums;

namespace ECommerceApi.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync(OrderStatus? statusFilter = null);
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdWithItemsAsync(int id);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
}
