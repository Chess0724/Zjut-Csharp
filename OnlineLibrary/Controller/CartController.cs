using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Dto;
using OnlineLibrary.Model;
using OnlineLibrary.Model.DatabaseContext;
using System.Security.Claims;

namespace OnlineLibrary.Controller;

[Route("/cart")]
[ApiController]
[Authorize]
public class CartController(
    ILogger<CartController> logger,
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
    /// 获取当前用户的购物车
    /// </summary>
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<CartItemDto[]>> GetCart()
    {
        var userId = GetUserId();
        var cartItems = await context.CartItems
            .Include(x => x.Book)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.AddedDate)
            .Select(x => new CartItemDto
            {
                BookId = x.Book.Id,
                Title = x.Book.Title,
                Author = x.Book.Author,
                Publisher = x.Book.Publisher,
                Identifier = x.Book.Identifier,
                Price = x.Book.Price,
                OriginalPrice = x.Book.OriginalPrice,
                Quantity = x.Quantity,
                Inventory = x.Book.Inventory > x.Book.Borrowed ? x.Book.Inventory - x.Book.Borrowed : 0,
                AddedDate = x.AddedDate
            })
            .ToArrayAsync();

        return new ResultDto<CartItemDto[]>
        {
            Code = 0,
            Message = "OK",
            Data = cartItems,
            RecordCount = cartItems.Length
        };
    }

    /// <summary>
    /// 添加商品到购物车
    /// </summary>
    [HttpPost]
    public async Task<ResultDto<string>> AddToCart([FromBody] AddToCartRequestDto request)
    {
        try
        {
            var userId = GetUserId();

            // 检查书籍是否存在
            var book = await context.Books.FindAsync(request.BookId);
            if (book == null)
            {
                return new ResultDto<string> { Code = 1, Message = "书籍不存在" };
            }

            // 检查库存（Inventory 就是当前可用库存）
            if (book.Inventory < request.Quantity)
            {
                return new ResultDto<string> { Code = 2, Message = "库存不足" };
            }

            // 检查是否已在购物车中
            var existingItem = await context.CartItems
                .FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == request.BookId);

            if (existingItem != null)
            {
                // 更新数量
                var newQuantity = existingItem.Quantity + request.Quantity;
                if (newQuantity > book.Inventory)
                {
                    return new ResultDto<string> { Code = 2, Message = "库存不足，无法添加更多" };
                }
                existingItem.Quantity = newQuantity;
                existingItem.AddedDate = DateTime.Now;
            }
            else
            {
                // 添加新项
                var cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = request.BookId,
                    Quantity = request.Quantity,
                    AddedDate = DateTime.Now
                };
                context.CartItems.Add(cartItem);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("用户 {UserId} 添加书籍 {BookId} 到购物车，数量：{Quantity}", userId, request.BookId, request.Quantity);

            return new ResultDto<string> { Code = 0, Message = "添加成功" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "添加购物车失败: {Message}", ex.Message);
            return new ResultDto<string> { Code = 500, Message = $"服务器内部错误: {ex.Message}" };
        }
    }

    /// <summary>
    /// 更新购物车商品数量
    /// </summary>
    [HttpPut]
    public async Task<ResultDto<string>> UpdateQuantity([FromBody] UpdateCartQuantityRequestDto request)
    {
        var userId = GetUserId();

        var cartItem = await context.CartItems
            .Include(x => x.Book)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == request.BookId);

        if (cartItem == null)
        {
            return new ResultDto<string> { Code = 1, Message = "购物车中没有该商品" };
        }

        if (request.Quantity <= 0)
        {
            // 删除该项
            context.CartItems.Remove(cartItem);
        }
        else
        {
            // 检查库存（Inventory 就是当前可用库存）
            if (request.Quantity > cartItem.Book.Inventory)
            {
                return new ResultDto<string> { Code = 2, Message = "库存不足" };
            }
            cartItem.Quantity = request.Quantity;
        }

        await context.SaveChangesAsync();
        return new ResultDto<string> { Code = 0, Message = "更新成功" };
    }

    /// <summary>
    /// 从购物车移除商品
    /// </summary>
    [HttpDelete("{bookId}")]
    public async Task<ResultDto<string>> RemoveFromCart(int bookId)
    {
        var userId = GetUserId();

        var cartItem = await context.CartItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == bookId);

        if (cartItem == null)
        {
            return new ResultDto<string> { Code = 1, Message = "购物车中没有该商品" };
        }

        context.CartItems.Remove(cartItem);
        await context.SaveChangesAsync();

        logger.LogInformation("用户 {UserId} 从购物车移除书籍 {BookId}", userId, bookId);
        return new ResultDto<string> { Code = 0, Message = "移除成功" };
    }

    /// <summary>
    /// 清空购物车
    /// </summary>
    [HttpDelete]
    public async Task<ResultDto<string>> ClearCart()
    {
        var userId = GetUserId();

        var cartItems = await context.CartItems
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return new ResultDto<string> { Code = 0, Message = "购物车已经是空的" };
        }

        context.CartItems.RemoveRange(cartItems);
        await context.SaveChangesAsync();

        logger.LogInformation("用户 {UserId} 清空购物车，共移除 {Count} 项", userId, cartItems.Count);
        return new ResultDto<string> { Code = 0, Message = "清空成功" };
    }
}
