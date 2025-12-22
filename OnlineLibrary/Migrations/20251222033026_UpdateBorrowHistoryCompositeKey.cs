using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBorrowHistoryCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BorrowHistories",
                table: "BorrowHistories");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "BookComments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BorrowHistories",
                table: "BorrowHistories",
                columns: new[] { "BookId", "UserId", "BorrowDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BorrowHistories",
                table: "BorrowHistories");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "BookComments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BorrowHistories",
                table: "BorrowHistories",
                columns: new[] { "BookId", "UserId" });
        }
    }
}
