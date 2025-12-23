using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace OnlineLibrary.Service;

/// <summary>
/// 邮件发送服务
/// </summary>
public class EmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// 发送验证码邮件
    /// </summary>
    /// <param name="toEmail">收件人邮箱</param>
    /// <param name="code">验证码</param>
    /// <returns>是否发送成功</returns>
    public async Task<bool> SendVerificationCodeAsync(string toEmail, string code)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("网上图书馆", _settings.Sender));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "【网上图书馆】注册验证码";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333; text-align: center;'>网上图书馆 - 邮箱验证</h2>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px; text-align: center;'>
                        <p style='color: #fff; font-size: 16px; margin-bottom: 20px;'>您的注册验证码是：</p>
                        <div style='background: #fff; display: inline-block; padding: 15px 40px; border-radius: 8px;'>
                            <span style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px;'>{code}</span>
                        </div>
                        <p style='color: #fff; font-size: 14px; margin-top: 20px;'>验证码有效期为 5 分钟，请尽快完成验证。</p>
                    </div>
                    <p style='color: #999; font-size: 12px; text-align: center; margin-top: 20px;'>
                        如果这不是您的操作，请忽略此邮件。
                    </p>
                </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // 使用 SSL 连接
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_settings.Sender, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("验证码邮件已发送至 {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送验证码邮件失败: {Email}", toEmail);
            return false;
        }
    }
}
