using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Dto;

/// <summary>
/// 发送验证码请求
/// </summary>
public record SendCodeRequestDto
{
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public required string Email { get; set; }
}
