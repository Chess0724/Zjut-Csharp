namespace OnlineLibrary.Dto;

public record BookDto
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string Author { get; set; } = default!;

    public string Publisher { get; set; } = default!;

    public string PublishedDate { get; set; } = default!;

    public string Identifier { get; set; } = default!;

    /// <summary>
    /// 借阅库存
    /// </summary>
    public uint Inventory { get; set; }

    /// <summary>
    /// 销售库存
    /// </summary>
    public uint SaleInventory { get; set; } = 100;

    public decimal Price { get; set; } = 39.90m;

    public decimal? OriginalPrice { get; set; }
}