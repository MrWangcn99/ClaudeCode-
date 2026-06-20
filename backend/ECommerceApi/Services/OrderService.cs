using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Models;
using ECommerceApi.Models.Enums;
using ECommerceApi.Repositories.Interfaces;
using ECommerceApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Services;

/// <summary>
/// Order service with integrated concurrency control.
///
/// Multi-threading and concurrency strategy (3 layers of protection against overselling):
///
/// Layer 1 — Per-product SemaphoreSlim (StockConcurrencyManager, Singleton):
///   Serializes access to each product's stock. Only ONE thread can deduct
///   stock for a given product at any moment. Products are sorted by ID
///   before lock acquisition to prevent deadlocks.
///
/// Layer 2 — Database Transaction:
///   Ensures stock deduction and order creation are atomic. If anything
///   fails mid-way, ALL changes roll back.
///
/// Layer 3 — Stock Check Inside Locked Region:
///   The guard `product.Stock < quantity` executes while holding the
///   semaphore — no other thread can slip in between check and decrement.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly StockConcurrencyManager _lockManager;
    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        StockConcurrencyManager lockManager,
        AppDbContext context,
        ILogger<OrderService> logger)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _lockManager = lockManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates an order with full concurrency protection.
    ///
    /// Flow:
    /// 1. Extract distinct product IDs, sort to prevent deadlocks
    /// 2. Acquire per-product SemaphoreSlim locks via StockConcurrencyManager
    /// 3. Begin database transaction
    /// 4. Inside lock + transaction: validate stock, deduct, create order
    /// 5. Commit transaction
    /// 6. Release locks (via `await using` disposal)
    /// 7. Return OrderDto
    /// </summary>
    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

        _logger.LogInformation(
            "Order creation requested for {ItemCount} items across {ProductCount} products: [{ProductIds}]",
            dto.Items.Count, productIds.Count, string.Join(", ", productIds));

        // ===== LAYER 1: Acquire per-product locks =====
        // This serializes access to the products in this order.
        // Other orders for DIFFERENT products proceed concurrently.
        // Other orders for the SAME products wait here.
        await using var lockReleaser = await _lockManager.AcquireLocksAsync(productIds);

        _logger.LogDebug("Acquired locks for products: [{ProductIds}]", string.Join(", ", productIds));

        // ===== LAYER 2: Database transaction for atomicity =====
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Load all products (we already hold the locks)
            var products = new Dictionary<int, Product>();
            foreach (var productId in productIds)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product is null)
                    throw new InvalidOperationException($"Product with ID {productId} not found.");
                products[productId] = product;
            }

            // ===== LAYER 3: Stock validation + deduction inside lock =====
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in dto.Items)
            {
                var product = products[itemDto.ProductId];

                // Guard: check stock while we hold the exclusive lock
                if (product.Stock < itemDto.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Requested: {itemDto.Quantity}, Available: {product.Stock}");
                }

                // Deduct stock — safe because we hold the per-product lock
                product.Stock -= itemDto.Quantity;

                var subtotal = product.Price * itemDto.Quantity;
                totalAmount += subtotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    SubTotal = subtotal
                });

                // Persist the stock deduction
                await _productRepository.UpdateAsync(product);

                _logger.LogDebug(
                    "Deducted {Qty} from product '{ProductName}' (remaining: {Remaining})",
                    itemDto.Quantity, product.Name, product.Stock);
            }

            // Create the order entity
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                Status = OrderStatus.Completed,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            var createdOrder = await _orderRepository.AddAsync(order);

            // Commit: stock deduction + order creation are atomic
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Order {OrderNumber} created successfully. Total: {Total:C}, Items: {ItemCount}",
                createdOrder.OrderNumber, createdOrder.TotalAmount, createdOrder.OrderItems.Count);

            return MapToOrderDto(createdOrder);
        }
        catch
        {
            await transaction.RollbackAsync();
            _logger.LogWarning("Order creation failed, transaction rolled back");
            throw;
        }
        // LAYER 1 cleanup: lockReleaser is disposed here by `await using`
        // This releases all semaphores so other threads can proceed
    }

    public async Task<List<OrderDto>> GetOrdersAsync(OrderFilterDto? filter = null)
    {
        OrderStatus? statusFilter = null;
        if (filter?.Status != null &&
            Enum.TryParse<OrderStatus>(filter.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        var orders = await _orderRepository.GetAllAsync(statusFilter);
        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt
        }).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id);
        if (order is null) return null;
        return MapToOrderDto(order);
    }

    /// <summary>
    /// Generates a unique order number: ORD-{yyyyMMdd}-{6 random hex chars}
    /// </summary>
    private static string GenerateOrderNumber()
    {
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{randomPart}";
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Unknown",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                SubTotal = oi.SubTotal
            }).ToList()
        };
    }
}
