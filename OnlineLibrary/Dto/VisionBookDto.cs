namespace OnlineLibrary.Dto;

/// <summary>
/// 从图片中提取的图书信息
/// </summary>
public class BookInfoFromImageDto
{
    public bool Success { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? PublishedDate { get; set; }
    public string? ISBN { get; set; }
    public decimal? Price { get; set; }
    public string? RawText { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// 图片上传请求
/// </summary>
public class ImageUploadRequest
{
    public string Base64Image { get; set; } = string.Empty;
}
