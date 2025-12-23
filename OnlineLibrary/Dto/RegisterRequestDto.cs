using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Dto;

public record RegisterRequestDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "验证码不能为空")]
    public required string VerificationCode { get; set; }
}