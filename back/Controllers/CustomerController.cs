using back.DTOs;
using back.Services.Customers;
using Back.Data.Infrastructure.EF.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Client)}")]
    public async Task<ActionResult<IList<CustomerDto>>> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Client)}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {Id} not found", id);
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }
        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)}")]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _customerService.CreateAsync(dto);
            _logger.LogInformation("Customer created with ID {Id}", created.Id);
            return Ok("Customer created" );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create customer");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
	[Authorize(Roles = $"{nameof(UserRole.Admin)}")]
	public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _customerService.UpdateAsync(id, dto);
        if (updated == null)
        {
            _logger.LogWarning("Customer with ID {Id} not found for update", id);
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }

        _logger.LogInformation("Customer with ID {Id} updated", id);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
	[Authorize(Roles = $"{nameof(UserRole.Admin)}")]
	public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Customer with ID {Id} not found for deletion", id);
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }

        _logger.LogInformation("Customer with ID {Id} deleted", id);
        return NoContent();
    }
}
