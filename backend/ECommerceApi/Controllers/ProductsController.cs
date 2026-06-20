using Microsoft.AspNetCore.Mvc;
using ECommerceApi.DTOs;
using ECommerceApi.Services.Interfaces;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product is null) return NotFound(new { error = $"Product with ID {id} not found." });
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = await _productService.UpdateProductAsync(id, dto);
        if (product is null) return NotFound(new { error = $"Product with ID {id} not found." });
        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted) return NotFound(new { error = $"Product with ID {id} not found." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
