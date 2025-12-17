using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Model;

/// <summary>
/// 订单项
/// </summary>
public class OrderItem
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Required]
    [ForeignKey("Order")]
    public int OrderId { get; set; }

    [Required]
    [ForeignKey("Book")]
    public int BookId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = default!;

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // 导航属性（EF Core 自动填充，不需要 Required）
    public Order Order { get; set; } = default!;

    public Book Book { get; set; } = default!;
}
