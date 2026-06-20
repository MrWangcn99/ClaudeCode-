using Microsoft.AspNetCore.Mvc;
using ECommerceApi.DTOs;
using ECommerceApi.Services.Interfaces;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var inventory = await _inventoryService.GetAllInventoryAsync();
        return Ok(inventory);
    }

    [HttpPut("{productId:int}/stock")]
    public async Task<ActionResult<ProductDto>> UpdateStock(int productId, [FromBody] InventoryUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _inventoryService.UpdateStockAsync(productId, dto);
        if (result is null) return NotFound(new { error = $"Product with ID {productId} not found." });
        return Ok(result);
    }
}
