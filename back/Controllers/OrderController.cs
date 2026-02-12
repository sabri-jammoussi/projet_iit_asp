using back.DTOs;
using back.Services.Orders;
using Back.Data.Infrastructure.EF.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IList<OrderDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Client)}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        // Client can only see their own orders
        if (IsClient())
        {
            var myOrders = await _orderService.GetByAccountIdAsync();
            if (!myOrders.Any(o => o.Id == id))
            {
                return Forbid();
            }
        }

        return Ok(order);
    }


    [HttpGet("my-orders")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<ActionResult<IList<OrderDto>>> GetMyOrders()
    {
        var orders = await _orderService.GetByAccountIdAsync();
        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _orderService.CreateByAccountIdAsync(dto);
            _logger.LogInformation("Order created with ID {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create order");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new { message = "Status is required." });
        }

        var updated = await _orderService.UpdateStatusAsync(id, dto.Status);
        if (updated == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        _logger.LogInformation("Order {Id} status updated to {Status}", id, dto.Status);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        _logger.LogInformation("Order with ID {Id} deleted", id);
        return NoContent();
    }

    private bool IsClient()
    {
        return User.IsInRole(nameof(UserRole.Client));
    }

}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
