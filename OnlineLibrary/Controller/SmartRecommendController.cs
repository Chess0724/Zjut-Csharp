using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Constant;
using OnlineLibrary.Dto;
using OnlineLibrary.Model;
using OnlineLibrary.Service;

namespace OnlineLibrary.Controller;

/// <summary>
/// 智能书籍推荐控制器 - 基于余弦相似度算法
/// </summary>
[Route("[controller]")]
[ApiController]
public class SmartRecommendController(
    BookRecommendationService recommendationService,
    UserManager<ApiUser> userManager,
    ILogger<SmartRecommendController> logger)
    : ControllerBase
{
    private static readonly ZhongTuClassification Classification = new();

    /// <summary>
    /// 获取个性化书籍推荐（猜你喜欢）
    /// </summary>
    /// <param name="count">推荐数量，默认10本</param>
    /// <returns>推荐的书籍列表</returns>
    [Authorize]
    [HttpGet]
    [ResponseCache(Duration = 300, VaryByHeader = "Authorization")] // 缓存5分钟
    public async Task<ResultDto<BookRecommendationDto[]>> GetRecommendations(int count = 10)
    {
        try
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return new ResultDto<BookRecommendationDto[]>
                {
                    Code = 1,
                    Message = "用户未登录",
                    Data = null
                };
            }

            logger.LogInformation("为用户 {UserName} ({UserId}) 生成个性化推荐", user.UserName, user.Id);

            var books = await recommendationService.RecommendBooksAsync(user.Id, count);

            var dto = books.Select(b => new BookRecommendationDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Publisher = b.Publisher,
                PublishedDate = b.PublishedDate,
                Identifier = b.Identifier,
                CategoryName = GetCategoryName(b.Identifier),
                Price = b.Price,
                OriginalPrice = b.OriginalPrice,
                Inventory = b.Inventory,
                RecommendReason = "根据您的购买历史推荐"
            }).ToArray();

            return new ResultDto<BookRecommendationDto[]>
            {
                Code = 0,
                Message = "OK",
                Data = dto,
                RecordCount = dto.Length
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "生成推荐时发生错误");
            return new ResultDto<BookRecommendationDto[]>
            {
                Code = 2,
                Message = "生成推荐失败: " + ex.Message,
                Data = null
            };
        }
    }

    /// <summary>
    /// 获取用户购买偏好统计
    /// </summary>
    [Authorize]
    [HttpGet("stats")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<UserPurchaseStatsDto[]>> GetUserStats()
    {
        try
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return new ResultDto<UserPurchaseStatsDto[]>
                {
                    Code = 1,
                    Message = "用户未登录",
                    Data = null
                };
            }

            var stats = await recommendationService.GetUserPurchaseStatsAsync(user.Id);
            var preference = await recommendationService.GetUserPreferenceAsync(user.Id);

            var dto = stats.Select(kvp => new UserPurchaseStatsDto
            {
                CategoryCode = kvp.Key,
                CategoryName = GetCategoryNameByCode(kvp.Key),
                PurchaseCount = kvp.Value,
                PreferenceScore = preference.CategoryScores.TryGetValue(kvp.Key, out var score) ? score : 0
            })
            .OrderByDescending(x => x.PurchaseCount)
            .ToArray();

            return new ResultDto<UserPurchaseStatsDto[]>
            {
                Code = 0,
                Message = "OK",
                Data = dto,
                RecordCount = dto.Length
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取用户统计时发生错误");
            return new ResultDto<UserPurchaseStatsDto[]>
            {
                Code = 2,
                Message = "获取统计失败: " + ex.Message,
                Data = null
            };
        }
    }

    /// <summary>
    /// 根据书籍 Identifier 获取分类名称
    /// </summary>
    private static string GetCategoryName(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return "综合性图书";

        var firstPart = identifier.Split(',')[0].Trim();
        foreach (var c in firstPart)
        {
            if (char.IsLetter(c))
            {
                return GetCategoryNameByCode(c.ToString().ToUpperInvariant());
            }
        }

        return "综合性图书";
    }

    /// <summary>
    /// 根据分类代码获取分类名称
    /// </summary>
    private static string GetCategoryNameByCode(string code)
    {
        try
        {
            return Classification.GetClassificationName(code);
        }
        catch
        {
            return "综合性图书";
        }
    }
}
