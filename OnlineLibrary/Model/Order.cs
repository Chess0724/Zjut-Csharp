using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLibrary.Model;

/// <summary>
/// 订单状态枚举
/// </summary>
public enum OrderStatus
{
    Pending,     // 待支付
    Paid,        // 已支付
    Shipped,     // 已发货
    Delivered,   // 已送达
    Completed,   // 已完成
    Cancelled    // 已取消
}

/// <summary>
/// 订单
/// </summary>
public class Order
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Required]
    [MaxLength(50)]
    public string OrderNo { get; set; } = default!;

    [Required]
    [ForeignKey("User")]
    public required string UserId { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    public DateTime CreateTime { get; set; }

    public DateTime? PayTime { get; set; }

    public DateTime? ShipTime { get; set; }

    public DateTime? CompleteTime { get; set; }

    [MaxLength(200)]
    public string? ShippingAddress { get; set; }

    [MaxLength(50)]
    public string? ReceiverName { get; set; }

    [MaxLength(20)]
    public string? ReceiverPhone { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }

    // 导航属性（EF Core 自动填充，不需要 Required）
    public ApiUser User { get; set; } = default!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
