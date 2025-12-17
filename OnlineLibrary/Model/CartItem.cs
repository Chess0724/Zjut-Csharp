using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Model;

/// <summary>
/// 购物车项
/// </summary>
public class CartItem
{
    [Required]
    [ForeignKey("User")]
    public required string UserId { get; set; }

    [Required]
    [ForeignKey("Book")]
    public int BookId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public DateTime AddedDate { get; set; }

    // 导航属性（EF Core 自动填充，不需要 Required）
    public Book Book { get; set; } = default!;

    public ApiUser User { get; set; } = default!;
}
