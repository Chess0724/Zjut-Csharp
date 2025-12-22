using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Model;
using OnlineLibrary.Model.DatabaseContext;

namespace OnlineLibrary.Service;

/// <summary>
/// 用户偏好向量：记录用户对各书籍分类的偏好得分
/// </summary>
public class UserPreference
{
    public string UserId { get; set; } = default!;
    public string? UserName { get; set; }
    /// <summary>
    /// Key: 书籍分类ID (如 "A", "B", "I" 等中图分类号首字母)
    /// Value: 偏好得分（基于购买数量计算）
    /// </summary>
    public Dictionary<string, double> CategoryScores { get; set; } = new();
}

/// <summary>
/// 基于余弦相似度的书籍推荐引擎
/// </summary>
public class BookRecommendationService(
    ApplicationDbContext context,
    ILogger<BookRecommendationService> logger)
{
    /// <summary>
    /// 从书籍的 Identifier (中图分类号) 中提取分类代码
    /// 例如: "A41/2-1=2" -> "A", "I247.5/1" -> "I"
    /// </summary>
    private static string ExtractCategoryCode(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return "Z"; // 默认归为综合类

        // 处理可能包含多个分类号的情况（用逗号分隔）
        var firstPart = identifier.Split(',')[0].Trim();

        // 提取首字母作为分类代码
        foreach (var c in firstPart)
        {
            if (char.IsLetter(c))
                return c.ToString().ToUpperInvariant();
        }

        return "Z"; // 默认归为综合类
    }

    /// <summary>
    /// 计算两个用户之间的余弦相似度
    /// </summary>
    /// <param name="userA">用户A的偏好向量</param>
    /// <param name="userB">用户B的偏好向量</param>
    /// <returns>余弦相似度值 (0-1之间，越接近1表示越相似)</returns>
    public double CalculateCosineSimilarity(UserPreference userA, UserPreference userB)
    {
        // 1. 找出两个用户所有涉及到的分类（取并集）
        var allCategories = userA.CategoryScores.Keys
            .Union(userB.CategoryScores.Keys)
            .ToList();

        double dotProduct = 0.0;  // 分子：点积
        double magnitudeA = 0.0;  // 分母一部分：向量A的模
        double magnitudeB = 0.0;  // 分母一部分：向量B的模

        foreach (var catId in allCategories)
        {
            double scoreA = userA.CategoryScores.TryGetValue(catId, out var valA) ? valA : 0;
            double scoreB = userB.CategoryScores.TryGetValue(catId, out var valB) ? valB : 0;

            dotProduct += scoreA * scoreB;
            magnitudeA += Math.Pow(scoreA, 2);
            magnitudeB += Math.Pow(scoreB, 2);
        }

        // 防止除以0
        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        // 余弦相似度公式 = (A·B) / (|A| * |B|)
        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    /// <summary>
    /// 获取指定用户的偏好向量（基于已完成订单的购买记录）
    /// </summary>
    public async Task<UserPreference> GetUserPreferenceAsync(string userId)
    {
        var preference = new UserPreference { UserId = userId };

        // 查询用户所有已完成订单中的书籍
        var purchasedItems = await context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Book)
            .Where(oi => oi.Order.UserId == userId &&
                        (oi.Order.Status == OrderStatus.Completed ||
                         oi.Order.Status == OrderStatus.Delivered ||
                         oi.Order.Status == OrderStatus.Paid))
            .Select(oi => new
            {
                oi.BookId,
                oi.Quantity,
                oi.Book.Identifier
            })
            .ToListAsync();

        // 按分类统计购买数量
        foreach (var item in purchasedItems)
        {
            var categoryCode = ExtractCategoryCode(item.Identifier);
            if (preference.CategoryScores.ContainsKey(categoryCode))
                preference.CategoryScores[categoryCode] += item.Quantity;
            else
                preference.CategoryScores[categoryCode] = item.Quantity;
        }

        // 对分数进行归一化处理（可选，使不同用户的向量可比）
        var maxScore = preference.CategoryScores.Values.DefaultIfEmpty(1).Max();
        if (maxScore > 0)
        {
            foreach (var key in preference.CategoryScores.Keys.ToList())
            {
                preference.CategoryScores[key] /= maxScore;
            }
        }

        return preference;
    }

    /// <summary>
    /// 获取所有有购买记录的用户的偏好向量
    /// </summary>
    public async Task<List<UserPreference>> GetAllUserPreferencesAsync()
    {
        // 获取所有有订单的用户ID
        var userIds = await context.Orders
            .Where(o => o.Status == OrderStatus.Completed ||
                       o.Status == OrderStatus.Delivered ||
                       o.Status == OrderStatus.Paid)
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync();

        var preferences = new List<UserPreference>();
        foreach (var userId in userIds)
        {
            preferences.Add(await GetUserPreferenceAsync(userId));
        }

        return preferences;
    }

    /// <summary>
    /// 为目标用户推荐书籍
    /// </summary>
    /// <param name="userId">目标用户ID</param>
    /// <param name="count">推荐数量</param>
    /// <param name="similarityThreshold">相似度阈值，低于此值则使用默认推荐</param>
    /// <returns>推荐的书籍列表</returns>
    public async Task<List<Book>> RecommendBooksAsync(
        string userId,
        int count = 10,
        double similarityThreshold = 0.3)
    {
        var targetUser = await GetUserPreferenceAsync(userId);
        var allUsers = await GetAllUserPreferencesAsync();

        logger.LogInformation(
            "为用户 {UserId} 生成推荐，用户偏好分类: {Categories}",
            userId,
            string.Join(", ", targetUser.CategoryScores.Keys));

        // 1. 如果目标用户没有购买记录，返回热门书籍
        if (targetUser.CategoryScores.Count == 0)
        {
            logger.LogInformation("用户 {UserId} 无购买记录，返回热门书籍", userId);
            return await GetPopularBooksAsync(count);
        }

        // 2. 寻找最相似的用户（排除自己）
        var similarUsers = allUsers
            .Where(u => u.UserId != userId)
            .Select(u => new
            {
                User = u,
                Similarity = CalculateCosineSimilarity(targetUser, u)
            })
            .Where(x => x.Similarity >= similarityThreshold)
            .OrderByDescending(x => x.Similarity)
            .Take(5)  // 取前5个最相似的用户
            .ToList();

        logger.LogInformation(
            "找到 {Count} 个相似用户，最高相似度: {MaxSimilarity:F3}",
            similarUsers.Count,
            similarUsers.FirstOrDefault()?.Similarity ?? 0);

        // 3. 如果没有找到相似用户，基于用户自己的偏好推荐
        if (similarUsers.Count == 0)
        {
            logger.LogInformation("未找到相似用户，基于用户自身偏好推荐");
            return await GetBooksFromPreferredCategoriesAsync(targetUser, count);
        }

        // 4. 从相似用户喜欢但目标用户还没买过的分类中推荐书籍
        var recommendedCategoryScores = new Dictionary<string, double>();

        foreach (var similar in similarUsers)
        {
            foreach (var kvp in similar.User.CategoryScores)
            {
                // 给分类加权：相似度 * 该分类的得分
                var weightedScore = similar.Similarity * kvp.Value;
                if (recommendedCategoryScores.ContainsKey(kvp.Key))
                    recommendedCategoryScores[kvp.Key] += weightedScore;
                else
                    recommendedCategoryScores[kvp.Key] = weightedScore;
            }
        }

        // 5. 优先推荐用户未涉及的分类，但也考虑用户已喜欢的分类
        var sortedCategories = recommendedCategoryScores
            .OrderByDescending(kvp =>
                targetUser.CategoryScores.ContainsKey(kvp.Key)
                    ? kvp.Value * 0.5  // 已购买过的分类权重降低
                    : kvp.Value)       // 未购买过的分类保持原权重
            .Select(kvp => kvp.Key)
            .ToList();

        // 6. 获取用户已购买的书籍ID，避免重复推荐
        var purchasedBookIds = await context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => oi.BookId)
            .Distinct()
            .ToListAsync();

        // 7. 从推荐分类中获取书籍
        var recommendedBooks = new List<Book>();

        foreach (var category in sortedCategories)
        {
            if (recommendedBooks.Count >= count)
                break;

            var booksInCategory = await context.Books
                .Where(b => b.Identifier.StartsWith(category) &&
                           !purchasedBookIds.Contains(b.Id) &&
                           b.Inventory > 0)
                .OrderByDescending(b => b.Borrowed)  // 按借阅/购买次数排序
                .ThenByDescending(b => b.InboundDate)
                .Take(count - recommendedBooks.Count)
                .ToListAsync();

            recommendedBooks.AddRange(booksInCategory);
        }

        // 8. 如果推荐数量不足，用热门书籍补充
        if (recommendedBooks.Count < count)
        {
            var additionalBooks = await context.Books
                .Where(b => !purchasedBookIds.Contains(b.Id) &&
                           !recommendedBooks.Select(rb => rb.Id).Contains(b.Id) &&
                           b.Inventory > 0)
                .OrderByDescending(b => b.Borrowed)
                .Take(count - recommendedBooks.Count)
                .ToListAsync();

            recommendedBooks.AddRange(additionalBooks);
        }

        logger.LogInformation(
            "为用户 {UserId} 推荐了 {Count} 本书籍",
            userId,
            recommendedBooks.Count);

        return recommendedBooks;
    }

    /// <summary>
    /// 获取热门书籍（按借阅/购买次数排序）
    /// </summary>
    private async Task<List<Book>> GetPopularBooksAsync(int count)
    {
        return await context.Books
            .Where(b => b.Inventory > 0)
            .OrderByDescending(b => b.Borrowed)
            .ThenByDescending(b => b.InboundDate)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// 根据用户偏好分类获取书籍
    /// </summary>
    private async Task<List<Book>> GetBooksFromPreferredCategoriesAsync(
        UserPreference preference,
        int count)
    {
        var sortedCategories = preference.CategoryScores
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var books = new List<Book>();

        foreach (var category in sortedCategories)
        {
            if (books.Count >= count)
                break;

            var booksInCategory = await context.Books
                .Where(b => b.Identifier.StartsWith(category) && b.Inventory > 0)
                .OrderByDescending(b => b.Borrowed)
                .Take(count - books.Count)
                .ToListAsync();

            books.AddRange(booksInCategory);
        }

        // 不足时用热门书籍补充
        if (books.Count < count)
        {
            var additionalBooks = await context.Books
                .Where(b => !books.Select(existing => existing.Id).Contains(b.Id) &&
                           b.Inventory > 0)
                .OrderByDescending(b => b.Borrowed)
                .Take(count - books.Count)
                .ToListAsync();

            books.AddRange(additionalBooks);
        }

        return books;
    }

    /// <summary>
    /// 获取用户购买统计（用于调试和分析）
    /// </summary>
    public async Task<Dictionary<string, int>> GetUserPurchaseStatsAsync(string userId)
    {
        var stats = new Dictionary<string, int>();

        var purchasedItems = await context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Book)
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => new
            {
                oi.Quantity,
                oi.Book.Identifier
            })
            .ToListAsync();

        foreach (var item in purchasedItems)
        {
            var categoryCode = ExtractCategoryCode(item.Identifier);
            if (stats.ContainsKey(categoryCode))
                stats[categoryCode] += item.Quantity;
            else
                stats[categoryCode] = item.Quantity;
        }

        return stats;
    }
}
