using OnlineLibrary.Model;

namespace OnlineLibrary.Dto;

/// <summary>
/// 订单项DTO
/// </summary>
public class OrderItemDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal => Price * Quantity;
}

/// <summary>
/// 订单响应DTO
/// </summary>
public class OrderResponseDto
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = default!;
    public string Status { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? PayTime { get; set; }
    public DateTime? ShipTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? Remark { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// 创建订单请求DTO
/// </summary>
public class CreateOrderRequestDto
{
    /// <summary>
    /// 要购买的商品列表（BookId和数量）
    /// 如果为空，则使用购物车中的所有商品
    /// </summary>
    public List<OrderItemRequestDto>? Items { get; set; }

    public string? ShippingAddress { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 订单商品请求DTO
/// </summary>
public class OrderItemRequestDto
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// 订单统计DTO
/// </summary>
public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TodaySales { get; set; }
}
