using Microsoft.Extensions.Caching.Memory;

namespace OnlineLibrary.Service;

/// <summary>
/// 验证码管理服务
/// </summary>
public class VerificationCodeService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<VerificationCodeService> _logger;
    private readonly TimeSpan _codeExpiration = TimeSpan.FromMinutes(5);

    public VerificationCodeService(IMemoryCache cache, ILogger<VerificationCodeService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 生成6位数字验证码
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <returns>验证码</returns>
    public string GenerateCode(string email)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = GetCacheKey(email);
        
        _cache.Set(cacheKey, code, _codeExpiration);
        _logger.LogInformation("已为邮箱 {Email} 生成验证码", email);
        
        return code;
    }

    /// <summary>
    /// 验证验证码
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="code">验证码</param>
    /// <returns>是否验证通过</returns>
    public bool ValidateCode(string email, string code)
    {
        var cacheKey = GetCacheKey(email);
        
        if (_cache.TryGetValue(cacheKey, out string? storedCode))
        {
            if (storedCode == code)
            {
                // 验证成功后删除验证码，防止重复使用
                _cache.Remove(cacheKey);
                _logger.LogInformation("邮箱 {Email} 验证码验证成功", email);
                return true;
            }
        }
        
        _logger.LogWarning("邮箱 {Email} 验证码验证失败", email);
        return false;
    }

    /// <summary>
    /// 检查是否可以发送验证码（防止频繁发送）
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <returns>是否可以发送</returns>
    public bool CanSendCode(string email)
    {
        var cooldownKey = GetCooldownKey(email);
        return !_cache.TryGetValue(cooldownKey, out _);
    }

    /// <summary>
    /// 设置发送冷却时间
    /// </summary>
    /// <param name="email">邮箱地址</param>
    public void SetCooldown(string email)
    {
        var cooldownKey = GetCooldownKey(email);
        _cache.Set(cooldownKey, true, TimeSpan.FromSeconds(60));
    }

    private static string GetCacheKey(string email) => $"VerificationCode:{email.ToLowerInvariant()}";
    private static string GetCooldownKey(string email) => $"VerificationCooldown:{email.ToLowerInvariant()}";
}
