namespace OnlineLibrary.Service;

/// <summary>
/// 邮箱配置
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// 发件人邮箱
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱授权码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// SMTP服务器地址
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP端口
    /// </summary>
    public int Port { get; set; } = 465;
}
