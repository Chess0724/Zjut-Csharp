using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Constant;
using OnlineLibrary.Dto;
using OnlineLibrary.Service;
using System.Security.Claims;

namespace OnlineLibrary.Controller;

/// <summary>
/// AI 功能控制器
/// </summary>
[ApiController]
[Route("[controller]")]
public class AIController : ControllerBase
{
    private readonly AIService _aiService;
    private readonly ILogger<AIController> _logger;

    public AIController(AIService aiService, ILogger<AIController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// 生成 AI 图书简介
    /// </summary>
    /// <param name="bookId">书籍ID</param>
    /// <returns>AI 生成的简介</returns>
    [HttpGet("summary/{bookId}")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> GenerateBookSummary(int bookId)
    {
        try
        {
            // 尝试获取当前用户ID（可选登录）
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("生成图书简介请求: BookId={BookId}, UserId={UserId}",
                bookId, userId ?? "匿名用户");

            var result = await _aiService.GenerateBookSummary(bookId, userId);

            if (!result.Success)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成图书简介失败: BookId={BookId}", bookId);
            return StatusCode(500, new { error = "AI 服务暂时不可用，请稍后重试" });
        }
    }

    /// <summary>
    /// 分析书籍是否适合当前用户
    /// </summary>
    /// <param name="bookId">书籍ID</param>
    /// <returns>适合度分析结果</returns>
    [HttpGet("suitability/{bookId}")]
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> AnalyzeBookSuitability(int bookId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "请先登录" });
            }

            _logger.LogInformation("分析书籍适合度: BookId={BookId}, UserId={UserId}",
                bookId, userId);

            var result = await _aiService.AnalyzeBookSuitability(bookId, userId);

            if (!result.Success)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析书籍适合度失败: BookId={BookId}", bookId);
            return StatusCode(500, new { error = "AI 服务暂时不可用，请稍后重试" });
        }
    }

    /// <summary>
    /// 从图片中提取图书信息
    /// </summary>
    /// <param name="request">包含 Base64 编码图片的请求</param>
    /// <returns>识别出的图书信息</returns>
    [HttpPost("extract-book")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Moderator}")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ActionResult<BookInfoFromImageDto>> ExtractBookFromImage([FromBody] ImageUploadRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Base64Image))
            {
                return BadRequest(new BookInfoFromImageDto
                {
                    Success = false,
                    Error = "请上传图片"
                });
            }

            _logger.LogInformation("开始从图片中提取图书信息");

            var result = await _aiService.ExtractBookInfoFromImageAsync(request.Base64Image);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从图片提取图书信息失败");
            return StatusCode(500, new BookInfoFromImageDto
            {
                Success = false,
                Error = "AI 服务暂时不可用，请稍后重试"
            });
        }
    }
}

