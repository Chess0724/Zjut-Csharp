using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Constant;
using OnlineLibrary.Dto;
using OnlineLibrary.Model;
using OnlineLibrary.Model.DatabaseContext;
using System.Security.Claims;

namespace OnlineLibrary.Controller;

[Route("bookcomment")]
[ApiController]
public class BookCommentController(
    ILogger<BookCommentController> logger,
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<List<CommentUnit>>> GetBookComments(int bookId)
    {
        var book = await context.Books.FindAsync(bookId);
        if (book == null)
            return new ResultDto<List<CommentUnit>>
            {
                Code = 404,
                Message = "Book not found.",
                Data = null
            };

        var commentsList = await context.BookComments
            .Where(x => x.BookId == bookId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreateTime)
            .ToListAsync();

        var comments = commentsList
            .Select(comment => new CommentUnit
            {
                Id = comment.Id,
                BookId = comment.BookId,
                UserId = comment.UserId,
                UserName = comment.User.UserName,
                Avatar = comment.User.Avatar,
                Content = comment.Content,
                Rating = comment.Rating,
                CreateTime = comment.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToList();
        
        logger.LogInformation("Get {Count} comments for book {BookId}", comments.Count, bookId);

        return new ResultDto<List<CommentUnit>>
        {
            Code = 0,
            Message = "OK",
            Data = comments
        };
    }

    [Authorize]
    [HttpPost]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ResultDto<object>> AddComment(BookCommentRequestDto request)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new ResultDto<object>
            {
                Code = 401,
                Message = "Unauthorized",
                Data = null
            };
        }

        var book = await context.Books.FindAsync(request.BookId);
        if (book == null)
        {
            return new ResultDto<object>
            {
                Code = 404,
                Message = "Book not found",
                Data = null
            };
        }

        var comment = new BookComment
        {
            UserId = userId,
            BookId = request.BookId,
            Content = request.Content,
            Rating = request.Rating,
            RefCommentId = request.RefCommentId,
            CreateTime = DateTime.Now
        };

        context.BookComments.Add(comment);
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} added comment for book {BookId}", userId, request.BookId);

        return new ResultDto<object>
        {
            Code = 0,
            Message = "OK",
            Data = null
        };
    }
}