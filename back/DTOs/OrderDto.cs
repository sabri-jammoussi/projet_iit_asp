namespace back.DTOs;

/// <summary>
/// DTO for returning Order data.
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public IList<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
}

/// <summary>
/// DTO for creating a new Order.
/// </summary>
public class CreateOrderDto
{
    public string? ShippingAddress { get; set; }
    public IList<CreateOrderDetailDto> OrderDetails { get; set; } = new List<CreateOrderDetailDto>();
}

/// <summary>
/// DTO for OrderDetail.
/// </summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// DTO for creating OrderDetail.
/// </summary>
public class CreateOrderDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
