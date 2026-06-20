using Microsoft.AspNetCore.Mvc;
using ECommerceApi.DTOs;
using ECommerceApi.Services.Interfaces;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Create an order with concurrency-safe stock deduction.
    /// Returns 201 on success, 400 on insufficient stock / invalid input,
    /// 503 on lock timeout (system under heavy load).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var order = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (TimeoutException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }

    /// <summary>
    /// List orders, optionally filtered by status.
    /// Query: ?status=Pending|Processing|Completed|Cancelled
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAll([FromQuery] string? status = null)
    {
        var filter = new OrderFilterDto { Status = status };
        var orders = await _orderService.GetOrdersAsync(filter);
        return Ok(orders);
    }

    /// <summary>
    /// Get a single order with its line items.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order is null) return NotFound(new { error = $"Order with ID {id} not found." });
        return Ok(order);
    }
}
