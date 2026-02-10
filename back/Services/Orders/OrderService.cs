using back.DTOs;
using Back.Data.Infrastructure.EF.Models;
using back.Repositories.Order;
using back.Repositories.Product;
using back.Repositories.Customer;
using Back.Commun.Account;

namespace back.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICurrentUserProvider _currentUserProvider;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        ILogger<OrderService> logger,
        IHttpClientFactory httpClientFactory,
        ICurrentUserProvider currentUserProvider)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<IList<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order with ID {Id} not found", id);
            return null;
        }
        return MapToDto(order);
    }

    public async Task<IList<OrderDto>> GetByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<IList<OrderDto>> GetByAccountIdAsync()
    {
        var user = await _currentUserProvider.Get();
		// Find customer by account ID
		var customer = await _customerRepository.GetByAccountIdAsync(user.Id);
        if (customer == null)
        {
            _logger.LogWarning("No customer found for Account {AccountId}", user.Id);
            return new List<OrderDto>();
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id);
        return orders.Select(MapToDto).ToList();
    }


    public async Task<OrderDto> CreateByAccountIdAsync( CreateOrderDto dto)
    {
        var user = await _currentUserProvider.Get();
        // Find customer by account ID
        var customer = await _customerRepository.GetByAccountIdAsync(user.Id);
        if (customer == null)
        {
            throw new InvalidOperationException($"No customer profile found for this account. Please contact support.");
        }

        // Validate order details
        if (dto.OrderDetails == null || dto.OrderDetails.Count == 0)
        {
            throw new InvalidOperationException("Order must contain at least one item.");
        }

        // Calculate total and create order details
        var orderDetails = new List<OrderDetailDao>();
        decimal totalAmount = 0;

        foreach (var item in dto.OrderDetails)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {item.Quantity}");
            }

            var lineTotal = product.Price * item.Quantity;
            totalAmount += lineTotal;

            orderDetails.Add(new OrderDetailDao
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            });

            // Update product stock
            product.Stock -= item.Quantity;
            await _productRepository.UpdateAsync(product);
        }

        var order = new OrderDao
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = "Pending",
            ShippingAddress = dto.ShippingAddress ?? customer.Address,
            OrderDetails = orderDetails
        };

        var created = await _orderRepository.AddAsync(order);
        _logger.LogInformation("Order created with ID {Id} for Customer {CustomerId} (Account {AccountId})", 
            created.Id, customer.Id, user.Id);

        // Send notification to Notification service
        await SendOrderNotificationAsync(created, customer);

        return MapToDto(created);
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, string status)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order with ID {Id} not found for status update", id);
            return null;
        }

        order.Status = status;
        var updated = await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Order {Id} status updated to {Status}", id, status);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _orderRepository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Order deleted with ID {Id}", id);
        }
        return result;
    }

    private async Task SendOrderNotificationAsync(OrderDao order, CustomerDao customer)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationService");
            var notification = new
            {
                Type = "NewOrder",
                Title = "Nouvelle commande",
                Message = $"Commande #{order.Id} créée par {customer.FirstName} {customer.LastName} pour un montant de {order.TotalAmount:C}",
                OrderId = order.Id,
                CustomerId = customer.Id,
                CustomerEmail = customer.Email,
                CreatedAt = DateTime.UtcNow
            };

            var response = await client.PostAsJsonAsync("/api/nf/notifications", notification);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notification sent for Order {OrderId}", order.Id);
            }
            else
            {
                _logger.LogWarning("Failed to send notification for Order {OrderId}. Status: {Status}", order.Id, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification for Order {OrderId}", order.Id);
        }
    }

    private static OrderDto MapToDto(OrderDao order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : null,
            OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDto
            {
                Id = od.Id,
                ProductId = od.ProductId,
                ProductName = od.Product?.Name,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice,
                LineTotal = od.LineTotal
            }).ToList() ?? new List<OrderDetailDto>()
        };
    }
}
