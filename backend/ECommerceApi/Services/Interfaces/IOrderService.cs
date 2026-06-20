using ECommerceApi.DTOs;

namespace ECommerceApi.Services.Interfaces;

public interface IOrderService
{
    /// <summary>
    /// Creates an order with concurrency-safe stock deduction.
    /// Throws InvalidOperationException if stock is insufficient.
    /// Throws TimeoutException if lock acquisition times out.
    /// </summary>
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);

    Task<List<OrderDto>> GetOrdersAsync(OrderFilterDto? filter = null);
    Task<OrderDto?> GetOrderByIdAsync(int id);
}
