using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Dto;
using OnlineLibrary.Model;
using OnlineLibrary.Model.DatabaseContext;

namespace OnlineLibrary.Service;

/// <summary>
/// AI 服务 - 调用通义千问大模型
/// </summary>
public class AIService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;

    // 通义千问配置
    private const string ApiBaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1";
    private const string ModelName = "qwen-flash";
    private const string VisionModelName = "qwen-vl-plus";  // 视觉模型
    private const string ApiKey = "sk-f68c591f97414695859088920ceb43a4";

    public AIService(
        ApplicationDbContext context,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<AIService> logger)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// 调用通义千问 API
    /// </summary>
    private async Task<string> CallQwenAsync(string systemPrompt, string userMessage, double temperature = 0.7)
    {
        try
        {
            var requestBody = new
            {
                model = ModelName,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                temperature = temperature,
                max_tokens = 1500
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/chat/completions")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            _logger.LogInformation("调用通义千问 API，模型: {Model}", ModelName);

            // 设置较长的超时时间（60秒）
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var response = await _httpClient.SendAsync(request, cts.Token);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("通义千问 API 响应状态: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("通义千问 API 调用失败: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                return $"AI 服务暂时不可用（错误码: {response.StatusCode}，{responseContent}）";
            }

            var jsonResponse = JsonDocument.Parse(responseContent);
            var content = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            _logger.LogInformation("通义千问 API 调用成功");
            return content ?? "无法获取 AI 响应";
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("通义千问 API 调用超时");
            return "AI 服务响应超时，请稍后重试";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "通义千问 API 网络错误");
            return $"网络连接失败: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通义千问 API 调用异常");
            return $"AI 服务出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 生成图书智能简介
    /// </summary>
    public async Task<BookAISummaryDto> GenerateBookSummary(int bookId, string? userId = null)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            return new BookAISummaryDto { Success = false, Error = "书籍不存在" };
        }

        // 获取用户借阅和购买历史（如果有登录）
        var userContext = "";

        if (!string.IsNullOrEmpty(userId))
        {
            var userHistory = await GetUserReadingHistory(userId);
            if (!string.IsNullOrEmpty(userHistory))
            {
                userContext = $"\n\n用户的阅读历史：\n{userHistory}";
            }
        }

        var systemPrompt = @"你是一位专业的图书馆员和书评专家。请根据书籍信息生成一份详细的图书介绍。

要求：
1. 生成一段吸引人的书籍简介（150-200字），介绍书籍的主要内容和特点
2. 列出3个本书的亮点或推荐理由
3. 说明适合的读者群体
4. 给出阅读建议

如果提供了用户的阅读历史，请额外分析：
- 这本书是否适合该用户
- 与用户之前阅读的书籍有什么关联
- 给出个性化的推荐理由

请用清晰的格式输出，使用 Markdown 格式。";

        var userMessage = $@"书籍信息：
- 书名：《{book.Title}》
- 作者：{book.Author ?? "未知"}
- 出版社：{book.Publisher ?? "未知"}
- 出版日期：{book.PublishedDate ?? "未知"}
- 分类号：{book.Identifier ?? "未知"}
- 价格：¥{book.Price}
- 库存：{book.Inventory} 本{userContext}

请为这本书生成智能简介。";

        var aiResponse = await CallQwenAsync(systemPrompt, userMessage, 0.7);

        return new BookAISummaryDto
        {
            Success = true,
            BookId = bookId,
            BookTitle = book.Title,
            Summary = aiResponse,
            GeneratedAt = DateTime.Now,
            HasUserContext = !string.IsNullOrEmpty(userId)
        };
    }

    /// <summary>
    /// 获取用户阅读历史
    /// </summary>
    private async Task<string> GetUserReadingHistory(string userId)
    {
        var borrowHistory = new List<string>();

        // 获取借阅历史
        var borrows = await _context.BorrowHistories
            .Include(b => b.Book)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BorrowDate)
            .Take(10)
            .ToListAsync();

        foreach (var borrow in borrows)
        {
            if (borrow.Book != null)
            {
                borrowHistory.Add($"- 借阅：《{borrow.Book.Title}》（{borrow.Book.Identifier}）");
            }
        }

        // 获取购买历史
        var purchases = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Book)
            .Where(oi => oi.Order.UserId == userId &&
                        (oi.Order.Status == OrderStatus.Completed ||
                         oi.Order.Status == OrderStatus.Delivered ||
                         oi.Order.Status == OrderStatus.Paid))
            .OrderByDescending(oi => oi.Order.CreateTime)
            .Take(10)
            .ToListAsync();

        foreach (var purchase in purchases)
        {
            if (purchase.Book != null)
            {
                borrowHistory.Add($"- 购买：《{purchase.Book.Title}》（{purchase.Book.Identifier}）");
            }
        }

        if (!borrowHistory.Any())
        {
            return "";
        }

        return string.Join("\n", borrowHistory.Distinct().Take(15));
    }

    /// <summary>
    /// 分析书籍是否适合用户
    /// </summary>
    public async Task<BookSuitabilityDto> AnalyzeBookSuitability(int bookId, string userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            return new BookSuitabilityDto { Success = false, Error = "书籍不存在" };
        }

        var userHistory = await GetUserReadingHistory(userId);

        if (string.IsNullOrEmpty(userHistory))
        {
            return new BookSuitabilityDto
            {
                Success = true,
                BookId = bookId,
                BookTitle = book.Title,
                SuitabilityScore = 70,
                Analysis = "您还没有借阅或购买记录，我们暂时无法为您进行个性化分析。这本书可能适合您，建议您查看书籍详情了解更多。",
                Recommendation = "建议尝试",
                Reasons = new List<string> { "作为新用户的首次尝试", "可以帮助我们了解您的阅读偏好" }
            };
        }

        var systemPrompt = @"你是一位专业的图书推荐专家。请根据用户的阅读历史，分析一本书是否适合该用户。

请返回 JSON 格式（严格遵循此格式）：
{
  ""suitabilityScore"": 85,
  ""recommendation"": ""强烈推荐"",
  ""analysis"": ""分析内容..."",
  ""reasons"": [""原因1"", ""原因2"", ""原因3""]
}

其中：
- suitabilityScore: 适合度评分（0-100）
- recommendation: 推荐等级（强烈推荐/推荐/一般/不太推荐）
- analysis: 详细分析（100字左右）
- reasons: 推荐或不推荐的理由（3条）";

        var userMessage = $@"目标书籍：
- 书名：《{book.Title}》
- 作者：{book.Author ?? "未知"}
- 分类号：{book.Identifier ?? "未知"}

用户阅读历史：
{userHistory}

请分析这本书是否适合该用户。";

        var aiResponse = await CallQwenAsync(systemPrompt, userMessage, 0.3);

        try
        {
            // 尝试解析 JSON 响应
            // 处理可能包含 markdown 代码块的情况
            var jsonStr = aiResponse;
            if (jsonStr.Contains("```json"))
            {
                jsonStr = jsonStr.Split("```json")[1].Split("```")[0].Trim();
            }
            else if (jsonStr.Contains("```"))
            {
                jsonStr = jsonStr.Split("```")[1].Split("```")[0].Trim();
            }

            var result = JsonSerializer.Deserialize<BookSuitabilityDto>(jsonStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
            {
                result.Success = true;
                result.BookId = bookId;
                result.BookTitle = book.Title;
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析 AI 响应 JSON 失败，返回原始响应");
        }

        // JSON 解析失败，返回原始响应
        return new BookSuitabilityDto
        {
            Success = true,
            BookId = bookId,
            BookTitle = book.Title,
            SuitabilityScore = 70,
            Recommendation = "推荐",
            Analysis = aiResponse,
            Reasons = new List<string>()
        };
    }

    /// <summary>
    /// 从图片中提取图书信息（使用视觉模型）
    /// </summary>
    public async Task<BookInfoFromImageDto> ExtractBookInfoFromImageAsync(string base64Image)
    {
        try
        {
            // 移除 data:image/xxx;base64, 前缀
            var imageData = base64Image;
            if (base64Image.Contains(","))
            {
                imageData = base64Image.Split(',')[1];
            }

            var systemPrompt = @"你是一个图书信息提取助手。请仔细观察图片中的图书封面或版权页，提取以下信息并以JSON格式返回：

{
  ""title"": ""书名"",
  ""author"": ""作者"",
  ""publisher"": ""出版社"",
  ""publishedDate"": ""出版日期（格式：YYYY-MM 或 YYYY）"",
  ""isbn"": ""ISBN号"",
  ""price"": ""定价（纯数字，如 59.00）""
}

注意：
1. 如果某项信息无法识别，请填写 null
2. 价格只需要数字，不需要货币符号
3. 严格返回JSON格式，不要添加其他文字";

            var requestBody = new
            {
                model = VisionModelName,
                messages = new object[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = systemPrompt },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = $"data:image/jpeg;base64,{imageData}" }
                            }
                        }
                    }
                },
                max_tokens = 1000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/chat/completions")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            _logger.LogInformation("调用视觉模型 API: {Model}", VisionModelName);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var response = await _httpClient.SendAsync(request, cts.Token);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("视觉模型 API 响应状态: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("视觉模型 API 调用失败: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                return new BookInfoFromImageDto
                {
                    Success = false,
                    Error = $"AI 服务调用失败（错误码: {response.StatusCode}）"
                };
            }

            var jsonResponse = JsonDocument.Parse(responseContent);
            var content = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            _logger.LogInformation("视觉模型返回内容: {Content}", content);

            if (string.IsNullOrEmpty(content))
            {
                return new BookInfoFromImageDto
                {
                    Success = false,
                    Error = "无法识别图片内容"
                };
            }

            // 尝试解析 JSON
            try
            {
                // 处理可能包含 markdown 代码块的情况
                var jsonStr = content;
                if (jsonStr.Contains("```json"))
                {
                    jsonStr = jsonStr.Split("```json")[1].Split("```")[0].Trim();
                }
                else if (jsonStr.Contains("```"))
                {
                    jsonStr = jsonStr.Split("```")[1].Split("```")[0].Trim();
                }

                var bookInfo = JsonSerializer.Deserialize<JsonElement>(jsonStr);

                return new BookInfoFromImageDto
                {
                    Success = true,
                    Title = bookInfo.TryGetProperty("title", out var t) && t.ValueKind != JsonValueKind.Null ? t.GetString() : null,
                    Author = bookInfo.TryGetProperty("author", out var a) && a.ValueKind != JsonValueKind.Null ? a.GetString() : null,
                    Publisher = bookInfo.TryGetProperty("publisher", out var p) && p.ValueKind != JsonValueKind.Null ? p.GetString() : null,
                    PublishedDate = bookInfo.TryGetProperty("publishedDate", out var pd) && pd.ValueKind != JsonValueKind.Null ? pd.GetString() : null,
                    ISBN = bookInfo.TryGetProperty("isbn", out var i) && i.ValueKind != JsonValueKind.Null ? i.GetString() : null,
                    Price = bookInfo.TryGetProperty("price", out var pr) && pr.ValueKind != JsonValueKind.Null
                        ? (decimal.TryParse(pr.ToString(), out var price) ? price : null)
                        : null,
                    RawText = content
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析视觉模型返回的 JSON 失败");
                return new BookInfoFromImageDto
                {
                    Success = true,
                    RawText = content,
                    Error = "JSON 解析失败，请查看原始识别结果手动输入"
                };
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("视觉模型 API 调用超时");
            return new BookInfoFromImageDto
            {
                Success = false,
                Error = "AI 服务响应超时，请稍后重试"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "视觉模型 API 调用异常");
            return new BookInfoFromImageDto
            {
                Success = false,
                Error = $"AI 服务出错: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// AI 生成的书籍简介
/// </summary>
public class BookAISummaryDto
{
    public bool Success { get; set; }
    public int BookId { get; set; }
    public string? BookTitle { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public bool HasUserContext { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// 书籍适合度分析
/// </summary>
public class BookSuitabilityDto
{
    public bool Success { get; set; }
    public int BookId { get; set; }
    public string? BookTitle { get; set; }
    public int SuitabilityScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public List<string> Reasons { get; set; } = new();
    public string? Error { get; set; }
}
