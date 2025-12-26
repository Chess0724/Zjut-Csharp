using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Constant;
using OnlineLibrary.Dto;
using OnlineLibrary.Service;

namespace OnlineLibrary.Controller;

/// <summary>
/// Excel 导入导出控制器
/// </summary>
[Route("/Excel")]
[ApiController]
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Moderator}")]
public class ExcelController : ControllerBase
{
    private readonly ExcelService _excelService;
    private readonly ILogger<ExcelController> _logger;

    public ExcelController(ExcelService excelService, ILogger<ExcelController> logger)
    {
        _excelService = excelService;
        _logger = logger;
    }

    /// <summary>
    /// 导出图书数据为 Excel
    /// </summary>
    [HttpGet("books")]
    public async Task<IActionResult> ExportBooks()
    {
        try
        {
            var bytes = await _excelService.ExportBooksAsync();
            var fileName = $"图书列表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出图书数据失败");
            return BadRequest("导出失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 导出用户数据为 Excel（仅管理员）
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ExportUsers()
    {
        try
        {
            var bytes = await _excelService.ExportUsersAsync();
            var fileName = $"用户列表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出用户数据失败");
            return BadRequest("导出失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 导出订单数据为 Excel
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> ExportOrders()
    {
        try
        {
            var bytes = await _excelService.ExportOrdersAsync();
            var fileName = $"订单列表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出订单数据失败");
            return BadRequest("导出失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 导出借阅历史为 Excel
    /// </summary>
    [HttpGet("borrows")]
    public async Task<IActionResult> ExportBorrows()
    {
        try
        {
            var bytes = await _excelService.ExportBorrowHistoryAsync();
            var fileName = $"借阅历史_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出借阅历史失败");
            return BadRequest("导出失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 下载图书导入模板
    /// </summary>
    [HttpGet("template/books")]
    public IActionResult DownloadBookTemplate()
    {
        try
        {
            var bytes = _excelService.GenerateBookImportTemplate();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "图书导入模板.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成导入模板失败");
            return BadRequest("生成模板失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 批量导入图书
    /// </summary>
    [HttpPost("books")]
    public async Task<ActionResult<BookImportResultDto>> ImportBooks(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("请选择要上传的文件");
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("仅支持 .xlsx 格式的 Excel 文件");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _excelService.ImportBooksAsync(stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入图书数据失败");
            return BadRequest("导入失败：" + ex.Message);
        }
    }
}
