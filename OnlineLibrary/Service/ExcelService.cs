using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Dto;
using OnlineLibrary.Model;
using OnlineLibrary.Model.DatabaseContext;

namespace OnlineLibrary.Service;

/// <summary>
/// Excel 导入导出服务
/// </summary>
public class ExcelService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApiUser> _userManager;
    private readonly ILogger<ExcelService> _logger;

    public ExcelService(
        ApplicationDbContext context,
        UserManager<ApiUser> userManager,
        ILogger<ExcelService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region 导出功能

    /// <summary>
    /// 导出图书数据到 Excel
    /// </summary>
    public async Task<byte[]> ExportBooksAsync()
    {
        var books = await _context.Books
            .OrderBy(b => b.Id)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("图书列表");

        // 设置表头
        var headers = new[] { "ID", "书名", "作者", "出版社", "出版日期", "分类号", "入库日期", "借阅库存", "已借出", "销售库存", "售价", "原价" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        // 设置表头样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // 填充数据
        for (int i = 0; i < books.Count; i++)
        {
            var book = books[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = book.Id;
            worksheet.Cell(row, 2).Value = book.Title;
            worksheet.Cell(row, 3).Value = book.Author;
            worksheet.Cell(row, 4).Value = book.Publisher;
            worksheet.Cell(row, 5).Value = book.PublishedDate;
            worksheet.Cell(row, 6).Value = book.Identifier;
            worksheet.Cell(row, 7).Value = book.InboundDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 8).Value = book.Inventory;
            worksheet.Cell(row, 9).Value = book.Borrowed;
            worksheet.Cell(row, 10).Value = book.SaleInventory;
            worksheet.Cell(row, 11).Value = book.Price;
            worksheet.Cell(row, 12).Value = book.OriginalPrice ?? 0;
        }

        // 自动调整列宽
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 导出用户数据到 Excel
    /// </summary>
    public async Task<byte[]> ExportUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("用户列表");

        // 设置表头
        var headers = new[] { "用户ID", "用户名", "邮箱", "手机号", "角色" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        // 设置表头样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGreen;

        // 填充数据
        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            var roles = await _userManager.GetRolesAsync(user);
            var row = i + 2;
            worksheet.Cell(row, 1).Value = user.Id;
            worksheet.Cell(row, 2).Value = user.UserName ?? "";
            worksheet.Cell(row, 3).Value = user.Email ?? "";
            worksheet.Cell(row, 4).Value = user.PhoneNumber ?? "";
            worksheet.Cell(row, 5).Value = string.Join(", ", roles);
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 导出订单数据到 Excel
    /// </summary>
    public async Task<byte[]> ExportOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Book)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreateTime)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("订单列表");

        // 设置表头
        var headers = new[] { "订单ID", "订单号", "用户名", "订单总额", "状态", "收货地址", "收货人", "联系电话", "创建时间", "支付时间", "发货时间", "商品列表" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightYellow;

        for (int i = 0; i < orders.Count; i++)
        {
            var order = orders[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = order.Id;
            worksheet.Cell(row, 2).Value = order.OrderNo;
            worksheet.Cell(row, 3).Value = order.User?.UserName ?? "";
            worksheet.Cell(row, 4).Value = order.TotalAmount;
            worksheet.Cell(row, 5).Value = order.Status.ToString();
            worksheet.Cell(row, 6).Value = order.ShippingAddress ?? "";
            worksheet.Cell(row, 7).Value = order.ReceiverName ?? "";
            worksheet.Cell(row, 8).Value = order.ReceiverPhone ?? "";
            worksheet.Cell(row, 9).Value = order.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 10).Value = order.PayTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 11).Value = order.ShipTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 12).Value = string.Join("; ", order.OrderItems.Select(item => $"{item.Book?.Title ?? "未知"}×{item.Quantity}"));
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 导出借阅历史到 Excel
    /// </summary>
    public async Task<byte[]> ExportBorrowHistoryAsync()
    {
        var borrows = await _context.BorrowHistories
            .Include(b => b.Book)
            .Include(b => b.User)
            .OrderByDescending(b => b.BorrowDate)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("借阅历史");

        var headers = new[] { "用户名", "书名", "作者", "借阅日期", "归还日期" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightCoral;

        for (int i = 0; i < borrows.Count; i++)
        {
            var borrow = borrows[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = borrow.User?.UserName ?? "";
            worksheet.Cell(row, 2).Value = borrow.Book?.Title ?? "";
            worksheet.Cell(row, 3).Value = borrow.Book?.Author ?? "";
            worksheet.Cell(row, 4).Value = borrow.BorrowDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 5).Value = borrow.ReturnDate.ToString("yyyy-MM-dd");
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region 导入功能

    /// <summary>
    /// 生成图书导入模板
    /// </summary>
    public byte[] GenerateBookImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("图书导入模板");

        // 表头
        var headers = new[] { "书名", "作者", "出版社", "出版日期", "分类号", "借阅库存", "销售库存", "售价", "原价" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // 示例数据
        worksheet.Cell(2, 1).Value = "数据结构与算法";
        worksheet.Cell(2, 2).Value = "张三";
        worksheet.Cell(2, 3).Value = "清华大学出版社";
        worksheet.Cell(2, 4).Value = "2023-06";
        worksheet.Cell(2, 5).Value = "TP312/1";
        worksheet.Cell(2, 6).Value = 10;
        worksheet.Cell(2, 7).Value = 100;
        worksheet.Cell(2, 8).Value = 59.90;
        worksheet.Cell(2, 9).Value = 79.00;

        // 添加说明
        worksheet.Cell(4, 1).Value = "说明：";
        worksheet.Cell(4, 1).Style.Font.Bold = true;
        worksheet.Cell(5, 1).Value = "1. 书名、作者、出版社、出版日期、分类号、借阅库存为必填项";
        worksheet.Cell(6, 1).Value = "2. 销售库存默认100，售价默认39.90";
        worksheet.Cell(7, 1).Value = "3. 请删除本示例数据行后再填写实际数据";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 从 Excel 导入图书
    /// </summary>
    public async Task<BookImportResultDto> ImportBooksAsync(Stream fileStream)
    {
        var result = new BookImportResultDto();
        var errors = new List<ImportErrorDto>();

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // 跳过表头

            foreach (var row in rows)
            {
                result.TotalRows++;
                var rowNumber = row.RowNumber();

                try
                {
                    var title = row.Cell(1).GetString().Trim();
                    var author = row.Cell(2).GetString().Trim();
                    var publisher = row.Cell(3).GetString().Trim();
                    var publishedDate = row.Cell(4).GetString().Trim();
                    var identifier = row.Cell(5).GetString().Trim();

                    // 验证必填字段
                    if (string.IsNullOrEmpty(title))
                    {
                        errors.Add(new ImportErrorDto { Row = rowNumber, Field = "书名", Message = "书名不能为空" });
                        result.FailCount++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(author))
                    {
                        errors.Add(new ImportErrorDto { Row = rowNumber, Field = "作者", Message = "作者不能为空" });
                        result.FailCount++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(publisher))
                    {
                        errors.Add(new ImportErrorDto { Row = rowNumber, Field = "出版社", Message = "出版社不能为空" });
                        result.FailCount++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(identifier))
                    {
                        errors.Add(new ImportErrorDto { Row = rowNumber, Field = "分类号", Message = "分类号不能为空" });
                        result.FailCount++;
                        continue;
                    }

                    // 解析数值字段
                    var inventoryCell = row.Cell(6);
                    var inventory = inventoryCell.TryGetValue<int>(out var inv) ? inv : 10;

                    var saleInventoryCell = row.Cell(7);
                    var saleInventory = saleInventoryCell.TryGetValue<int>(out var saleInv) ? saleInv : 100;

                    var priceCell = row.Cell(8);
                    var price = priceCell.TryGetValue<decimal>(out var p) ? p : 39.90m;

                    var originalPriceCell = row.Cell(9);
                    decimal? originalPrice = originalPriceCell.TryGetValue<decimal>(out var op) ? op : null;

                    // 创建图书
                    var book = new Book
                    {
                        Title = title,
                        Author = author,
                        Publisher = publisher,
                        PublishedDate = publishedDate,
                        Identifier = identifier,
                        Inventory = (uint)inventory,
                        SaleInventory = (uint)saleInventory,
                        Price = price,
                        OriginalPrice = originalPrice,
                        InboundDate = DateTime.Now,
                        Borrowed = 0
                    };

                    _context.Books.Add(book);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "导入第 {Row} 行数据时出错", rowNumber);
                    errors.Add(new ImportErrorDto { Row = rowNumber, Field = "行数据", Message = ex.Message });
                    result.FailCount++;
                }
            }

            await _context.SaveChangesAsync();
            result.Errors = errors;

            _logger.LogInformation("图书导入完成: 总计 {Total} 行，成功 {Success}，失败 {Fail}",
                result.TotalRows, result.SuccessCount, result.FailCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel 文件解析失败");
            throw new Exception("Excel 文件解析失败，请检查文件格式是否正确");
        }

        return result;
    }

    #endregion
}
