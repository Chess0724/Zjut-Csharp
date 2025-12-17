namespace OnlineLibrary.Dto;

/// <summary>
/// 购物车项响应DTO
/// </summary>
public class CartItemDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Publisher { get; set; } = default!;
    public string Identifier { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public uint Inventory { get; set; }
    public DateTime AddedDate { get; set; }
}

/// <summary>
/// 添加购物车请求DTO
/// </summary>
public class AddToCartRequestDto
{
    public int BookId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// 更新购物车数量请求DTO
/// </summary>
public class UpdateCartQuantityRequestDto
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
