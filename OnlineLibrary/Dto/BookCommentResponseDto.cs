namespace OnlineLibrary.Dto;

public class CommentUnit
{
    public required int Id { get; init; }

    public required int BookId { get; set; }

    public required string UserId { get; set; } = default!;

    public required string UserName { get; set; } = default!;

    public string? Avatar { get; set; }

    public required string Content { get; set; } = default!;

    public int Rating { get; set; }

    public required string CreateTime { get; set; } = default!;
}

public class BookCommentResponseDto
{
    public required int BookId { get; set; }

    public required List<CommentUnit> Comments { get; set; } = default!;
}