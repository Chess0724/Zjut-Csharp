using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Constant;
using OnlineLibrary.Dto;
using OnlineLibrary.Model;
using OnlineLibrary.Model.DatabaseContext;
using System.Security.Claims;

namespace OnlineLibrary.Controller;

[Route("/order")]
[ApiController]
[Authorize]
public class OrderController(
    ILogger<OrderController> logger,
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    private string GetUserId()
    {
        if (httpContextAccessor.HttpContext != null)
        {
            var claim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
                return claim.Value;
        }
        throw new UnauthorizedAccessException("用户未登录");
    }

    /// <summary>
    /// 生成订单号
    /// </summary>
    private static string GenerateOrderNo()
    {
        return $"XH{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    /// <summary>
    /// 获取当前用户的订单列表
    /// </summary>
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<OrderResponseDto[]>> GetOrders(
        int pageIndex = 0,
        int pageSize = 10,
        string? status = null)
    {
        var userId = GetUserId();
        var query = context.Orders
            .Include(x => x.OrderItems)
            .Where(x => x.UserId == userId);

        // 按状态筛选
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            query = query.Where(x => x.Status == orderStatus);
        }

        var recordCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(x => x.CreateTime)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(x => new OrderResponseDto
            {
                Id = x.Id,
                OrderNo = x.OrderNo,
                Status = x.Status.ToString(),
                TotalAmount = x.TotalAmount,
                CreateTime = x.CreateTime,
                PayTime = x.PayTime,
                ShipTime = x.ShipTime,
                CompleteTime = x.CompleteTime,
                ShippingAddress = x.ShippingAddress,
                ReceiverName = x.ReceiverName,
                ReceiverPhone = x.ReceiverPhone,
                Remark = x.Remark,
                Items = x.OrderItems.Select(i => new OrderItemDto
                {
                    BookId = i.BookId,
                    Title = i.Title,
                    Author = i.Author,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToArrayAsync();

        return new ResultDto<OrderResponseDto[]>
        {
            Code = 0,
            Message = "OK",
            Data = orders,
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = recordCount
        };
    }

    /// <summary>
    /// 获取订单详情
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<OrderResponseDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        var order = await context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order == null)
        {
            return new ResultDto<OrderResponseDto> { Code = 1, Message = "订单不存在" };
        }

        var dto = new OrderResponseDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreateTime = order.CreateTime,
            PayTime = order.PayTime,
            ShipTime = order.ShipTime,
            CompleteTime = order.CompleteTime,
            ShippingAddress = order.ShippingAddress,
            ReceiverName = order.ReceiverName,
            ReceiverPhone = order.ReceiverPhone,
            Remark = order.Remark,
            Items = order.OrderItems.Select(i => new OrderItemDto
            {
                BookId = i.BookId,
                Title = i.Title,
                Author = i.Author,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        return new ResultDto<OrderResponseDto> { Code = 0, Message = "OK", Data = dto };
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    [HttpPost]
    public async Task<ResultDto<OrderResponseDto>> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        var userId = GetUserId();

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            List<(Book book, int quantity)> orderItems;

            if (request.Items != null && request.Items.Count > 0)
            {
                // 使用指定的商品
                orderItems = new List<(Book, int)>();
                foreach (var item in request.Items)
                {
                    var book = await context.Books.FindAsync(item.BookId);
                    if (book == null)
                    {
                        return new ResultDto<OrderResponseDto> { Code = 1, Message = $"书籍ID {item.BookId} 不存在" };
                    }

                    // 检查销售库存（购买使用 SaleInventory）
                    if (item.Quantity > book.SaleInventory)
                    {
                        return new ResultDto<OrderResponseDto> { Code = 2, Message = $"《{book.Title}》销售库存不足" };
                    }

                    orderItems.Add((book, item.Quantity));
                }
            }
            else
            {
                // 使用购物车中的商品
                var cartItems = await context.CartItems
                    .Include(x => x.Book)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                if (cartItems.Count == 0)
                {
                    return new ResultDto<OrderResponseDto> { Code = 3, Message = "购物车为空" };
                }

                // 检查库存
                foreach (var item in cartItems)
                {
                    // 检查销售库存（购买使用 SaleInventory）
                    if (item.Quantity > item.Book.SaleInventory)
                    {
                        return new ResultDto<OrderResponseDto> { Code = 2, Message = $"《{item.Book.Title}》销售库存不足" };
                    }
                }

                orderItems = cartItems.Select(x => (x.Book, x.Quantity)).ToList();

                // 清空购物车
                context.CartItems.RemoveRange(cartItems);
            }

            // 计算总金额
            var totalAmount = orderItems.Sum(x => x.book.Price * x.quantity);

            // 创建订单
            var order = new Order
            {
                OrderNo = GenerateOrderNo(),
                UserId = userId,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                CreateTime = DateTime.Now,
                ShippingAddress = request.ShippingAddress,
                ReceiverName = request.ReceiverName,
                ReceiverPhone = request.ReceiverPhone,
                Remark = request.Remark
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // 创建订单项并减少销售库存
            foreach (var (book, quantity) in orderItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    Quantity = quantity
                };
                context.OrderItems.Add(orderItem);

                // 减少销售库存（购买的书从销售库存中扣除）
                book.SaleInventory -= (uint)quantity;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("用户 {UserId} 创建订单 {OrderNo}，总金额：{Amount}", userId, order.OrderNo, totalAmount);

            // 返回订单信息
            var dto = new OrderResponseDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CreateTime = order.CreateTime,
                ShippingAddress = order.ShippingAddress,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                Remark = order.Remark,
                Items = orderItems.Select(x => new OrderItemDto
                {
                    BookId = x.book.Id,
                    Title = x.book.Title,
                    Author = x.book.Author,
                    Price = x.book.Price,
                    Quantity = x.quantity
                }).ToList()
            };

            return new ResultDto<OrderResponseDto> { Code = 0, Message = "订单创建成功", Data = dto };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "创建订单失败");
            return new ResultDto<OrderResponseDto> { Code = 500, Message = "创建订单失败" };
        }
    }

    /// <summary>
    /// 支付订单（模拟）
    /// </summary>
    [HttpPost("{id}/pay")]
    public async Task<ResultDto<string>> PayOrder(int id)
    {
        var userId = GetUserId();
        var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order == null)
        {
            return new ResultDto<string> { Code = 1, Message = "订单不存在" };
        }

        if (order.Status != OrderStatus.Pending)
        {
            return new ResultDto<string> { Code = 2, Message = "订单状态不正确" };
        }

        order.Status = OrderStatus.Paid;
        order.PayTime = DateTime.Now;
        await context.SaveChangesAsync();

        logger.LogInformation("订单 {OrderNo} 已支付", order.OrderNo);
        return new ResultDto<string> { Code = 0, Message = "支付成功" };
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ResultDto<string>> CancelOrder(int id)
    {
        var userId = GetUserId();
        var order = await context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order == null)
        {
            return new ResultDto<string> { Code = 1, Message = "订单不存在" };
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Paid)
        {
            return new ResultDto<string> { Code = 2, Message = "该订单状态无法取消" };
        }

        // 恢复销售库存
        foreach (var item in order.OrderItems)
        {
            var book = await context.Books.FindAsync(item.BookId);
            if (book != null)
            {
                book.SaleInventory += (uint)item.Quantity;
            }
        }

        order.Status = OrderStatus.Cancelled;
        await context.SaveChangesAsync();

        logger.LogInformation("订单 {OrderNo} 已取消", order.OrderNo);
        return new ResultDto<string> { Code = 0, Message = "取消成功" };
    }

    /// <summary>
    /// 确认收货
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<ResultDto<string>> CompleteOrder(int id)
    {
        var userId = GetUserId();
        var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (order == null)
        {
            return new ResultDto<string> { Code = 1, Message = "订单不存在" };
        }

        if (order.Status != OrderStatus.Shipped && order.Status != OrderStatus.Delivered)
        {
            return new ResultDto<string> { Code = 2, Message = "订单状态不正确" };
        }

        order.Status = OrderStatus.Completed;
        order.CompleteTime = DateTime.Now;
        await context.SaveChangesAsync();

        logger.LogInformation("订单 {OrderNo} 已完成", order.OrderNo);
        return new ResultDto<string> { Code = 0, Message = "确认收货成功" };
    }

    /// <summary>
    /// 获取订单统计（管理员）
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = $"{RoleNames.Moderator},{RoleNames.Admin}")]
    public async Task<ResultDto<OrderStatisticsDto>> GetStatistics()
    {
        var today = DateTime.Today;
        var orders = await context.Orders.ToListAsync();

        var stats = new OrderStatisticsDto
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(x => x.Status == OrderStatus.Pending),
            PaidOrders = orders.Count(x => x.Status == OrderStatus.Paid),
            ShippedOrders = orders.Count(x => x.Status == OrderStatus.Shipped),
            CompletedOrders = orders.Count(x => x.Status == OrderStatus.Completed),
            CancelledOrders = orders.Count(x => x.Status == OrderStatus.Cancelled),
            TotalSales = orders.Where(x => x.Status != OrderStatus.Cancelled).Sum(x => x.TotalAmount),
            TodaySales = orders.Where(x => x.Status != OrderStatus.Cancelled && x.CreateTime.Date == today).Sum(x => x.TotalAmount)
        };

        return new ResultDto<OrderStatisticsDto> { Code = 0, Message = "OK", Data = stats };
    }

    /// <summary>
    /// 获取所有订单（管理员）
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = $"{RoleNames.Moderator},{RoleNames.Admin}")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<OrderResponseDto[]>> GetAllOrders(
        int pageIndex = 0,
        int pageSize = 15,
        string? status = null,
        string? filterQuery = null)
    {
        var query = context.Orders
            .Include(x => x.OrderItems)
            .Include(x => x.User)
            .AsQueryable();

        // 按状态筛选
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            query = query.Where(x => x.Status == orderStatus);
        }

        // 搜索
        if (!string.IsNullOrEmpty(filterQuery))
        {
            query = query.Where(x =>
                x.OrderNo.Contains(filterQuery) ||
                x.User.UserName!.Contains(filterQuery) ||
                x.ReceiverName!.Contains(filterQuery));
        }

        var recordCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(x => x.CreateTime)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(x => new OrderResponseDto
            {
                Id = x.Id,
                OrderNo = x.OrderNo,
                Status = x.Status.ToString(),
                TotalAmount = x.TotalAmount,
                CreateTime = x.CreateTime,
                PayTime = x.PayTime,
                ShipTime = x.ShipTime,
                CompleteTime = x.CompleteTime,
                ShippingAddress = x.ShippingAddress,
                ReceiverName = x.ReceiverName ?? x.User.UserName,
                ReceiverPhone = x.ReceiverPhone,
                Remark = x.Remark,
                Items = x.OrderItems.Select(i => new OrderItemDto
                {
                    BookId = i.BookId,
                    Title = i.Title,
                    Author = i.Author,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToArrayAsync();

        return new ResultDto<OrderResponseDto[]>
        {
            Code = 0,
            Message = "OK",
            Data = orders,
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = recordCount
        };
    }

    /// <summary>
    /// 发货（管理员）
    /// </summary>
    [HttpPost("{id}/ship")]
    [Authorize(Roles = $"{RoleNames.Moderator},{RoleNames.Admin}")]
    public async Task<ResultDto<string>> ShipOrder(int id)
    {
        var order = await context.Orders.FindAsync(id);

        if (order == null)
        {
            return new ResultDto<string> { Code = 1, Message = "订单不存在" };
        }

        if (order.Status != OrderStatus.Paid)
        {
            return new ResultDto<string> { Code = 2, Message = "只有已支付的订单才能发货" };
        }

        order.Status = OrderStatus.Shipped;
        order.ShipTime = DateTime.Now;
        await context.SaveChangesAsync();

        logger.LogInformation("订单 {OrderNo} 已发货", order.OrderNo);
        return new ResultDto<string> { Code = 0, Message = "发货成功" };
    }
}
