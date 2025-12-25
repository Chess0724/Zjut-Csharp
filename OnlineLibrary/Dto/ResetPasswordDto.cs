using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Dto;

/// <summary>
/// 密码重置请求 DTO
/// </summary>
public record ResetPasswordDto(
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    string Email,
    
    [Required(ErrorMessage = "验证码不能为空")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "验证码必须是6位")]
    string VerificationCode,
    
    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
    string NewPassword
);
