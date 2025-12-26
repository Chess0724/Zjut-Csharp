namespace OnlineLibrary.Dto;

/// <summary>
/// 图书导入 DTO
/// </summary>
public class BookImportDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string PublishedDate { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public int Inventory { get; set; }
    public int SaleInventory { get; set; } = 100;
    public decimal Price { get; set; } = 39.90m;
    public decimal? OriginalPrice { get; set; }
}

/// <summary>
/// 图书导入结果 DTO
/// </summary>
public class BookImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = new();
}

/// <summary>
/// 导入错误详情
/// </summary>
public class ImportErrorDto
{
    public int Row { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
