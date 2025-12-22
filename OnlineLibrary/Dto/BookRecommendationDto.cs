namespace OnlineLibrary.Dto;

/// <summary>
/// 书籍推荐响应 DTO
/// </summary>
public record BookRecommendationDto
{
    public int Id { get; init; }

    public string Title { get; init; } = default!;

    public string Author { get; init; } = default!;

    public string Publisher { get; init; } = default!;

    public string PublishedDate { get; init; } = default!;

    public string Identifier { get; init; } = default!;

    /// <summary>
    /// 分类名称（中图分类）
    /// </summary>
    public string CategoryName { get; init; } = default!;

    public decimal Price { get; init; }

    public decimal? OriginalPrice { get; init; }

    public uint Inventory { get; init; }

    /// <summary>
    /// 推荐理由
    /// </summary>
    public string? RecommendReason { get; init; }
}

/// <summary>
/// 用户购买统计 DTO
/// </summary>
public record UserPurchaseStatsDto
{
    /// <summary>
    /// 分类代码 (如 "A", "I")
    /// </summary>
    public string CategoryCode { get; init; } = default!;

    /// <summary>
    /// 分类名称
    /// </summary>
    public string CategoryName { get; init; } = default!;

    /// <summary>
    /// 购买数量
    /// </summary>
    public int PurchaseCount { get; init; }

    /// <summary>
    /// 偏好得分（归一化后）
    /// </summary>
    public double PreferenceScore { get; init; }
}
