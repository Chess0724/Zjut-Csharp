using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Constant;
using OnlineLibrary.Model;
using OnlineLibrary.Model.Csv;
using OnlineLibrary.Model.DatabaseContext;
using System.Globalization;

namespace OnlineLibrary.Controller;

// [Authorize(Roles = RoleNames.Admin)] // 临时注释，初始化完成后请恢复
[Route("[controller]/[action]")]
[ApiController]
public class SeedController(
    ApplicationDbContext context,
    IWebHostEnvironment env,
    ILogger<SeedController> logger,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApiUser> userManager)
    : ControllerBase
{
    [HttpPut]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> BookData()
    {
        // SETUP
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            MissingFieldFound = null,
            BadDataFound = null,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(env.ContentRootPath + "/Data/Books.csv");
        using var csv = new CsvReader(reader, config);
        var existingBooks = await context.Books.ToDictionaryAsync(book => book.Identifier);

        // EXECUTE
        var records = csv.GetRecords<BookRecord>();
        foreach (var record in records)
        {
            if (existingBooks.ContainsKey(record.Identifier))
            {
                logger.LogInformation("Book {Title} already exists.", record.Title);
                continue;
            }
            var book = new Book
            {
                Title = record.Title,
                Author = record.Author,
                Publisher = record.Publisher,
                PublishedDate = record.PublishedDate,
                Identifier = record.Identifier,
                Inventory = record.Inventory,
                Borrowed = record.Count,
                InboundDate = DateTime.Now
            };
            await context.Books.AddAsync(book);
            logger.LogInformation("Book {Title} added.", record.Title);
        }
        await context.SaveChangesAsync();

        return new JsonResult(new
        {
            books = context.Books.Count()
        });
    }

    [HttpPut]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> SettingsData()
    {
        var settings = await context.Settings.FirstOrDefaultAsync();
        if (settings != null)
            return new JsonResult(new
            {
                addedSettings = 0
            });
        var setting = new Setting
        {
            BorrowLimit = 20,
            BorrowDurationDays = 30
        };
        await context.Settings.AddAsync(setting);
        await context.SaveChangesAsync();
        return new JsonResult(new
        {
            addedSettings = 2
        });
    }

    [HttpPut]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> AuthData()
    {
        var usersCreated = 0;
        var rolesCreated = 0;
        var usersAddedToRoles = 0;

        // 添加用户
        var user = await userManager.FindByNameAsync("TestUser");
        if (user == null)
        {
            user = new ApiUser
            {
                UserName = "TestUser",
                Email = "testuser@online-library.org"
            };
            await userManager.CreateAsync(user, "123456");
            usersCreated++;
        }

        var moderator = await userManager.FindByNameAsync("TestModerator");
        if (moderator == null)
        {
            moderator = new ApiUser
            {
                UserName = "TestModerator",
                Email = "testmoderator@online-library.org"
            };
            await userManager.CreateAsync(moderator, "123456");
            usersCreated++;
        }

        var admin = await userManager.FindByNameAsync("TestAdmin");
        if (admin == null)
        {
            admin = new ApiUser
            {
                UserName = "TestAdmin",
                Email = "testadmin@online-library.org"
            };
            await userManager.CreateAsync(admin, "123456");
            usersCreated++;
        }

        var deleted = await userManager.FindByNameAsync("DeletedUser");
        if (deleted == null)
        {
            deleted = new ApiUser
            {
                UserName = "DeletedUser",
                Email = "deleted@online-library.org"
            };
            await userManager.CreateAsync(deleted, "123456");
            usersCreated++;
        }

        // 添加角色，一共有3种角色

        if (!await roleManager.RoleExistsAsync(RoleNames.User))
        {
            await roleManager.CreateAsync(
                new IdentityRole(RoleNames.User));
            rolesCreated++;
        }

        if (!await roleManager.RoleExistsAsync(RoleNames.Moderator))
        {
            await roleManager.CreateAsync(
                new IdentityRole(RoleNames.Moderator));
            rolesCreated++;
        }

        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            await roleManager.CreateAsync(
                new IdentityRole(RoleNames.Admin));
            rolesCreated++;
        }

        // 为3种角色对应的测试用户添加用户组，如果用户不存在就什么也不做
        var testUser = await userManager.FindByNameAsync("TestUser");
        if (testUser != null && !await userManager.IsInRoleAsync(testUser, RoleNames.User))
        {
            await userManager.AddToRoleAsync(testUser, RoleNames.User);
            usersAddedToRoles++;
        }

        var testModerator = await userManager.FindByNameAsync("TestModerator");
        if (testModerator != null)
        {
            if (!await userManager.IsInRoleAsync(testModerator, RoleNames.Moderator))
                await userManager.AddToRoleAsync(testModerator, RoleNames.Moderator);
            if (!await userManager.IsInRoleAsync(testModerator, RoleNames.User))
                await userManager.AddToRoleAsync(testModerator, RoleNames.User);
            usersAddedToRoles++;
        }

        var testAdmin = await userManager.FindByNameAsync("TestAdmin");
        if (testAdmin != null)
        {
            if (!await userManager.IsInRoleAsync(testAdmin, RoleNames.Admin))
                await userManager.AddToRoleAsync(testAdmin, RoleNames.Admin);
            if (!await userManager.IsInRoleAsync(testAdmin, RoleNames.Moderator))
                await userManager.AddToRoleAsync(testAdmin, RoleNames.Moderator);
            if (!await userManager.IsInRoleAsync(testAdmin, RoleNames.User))
                await userManager.AddToRoleAsync(testAdmin, RoleNames.User);
            usersAddedToRoles++;
        }

        var deletedUser = await userManager.FindByNameAsync("DeletedUser");
        if (deletedUser != null)
        {
            if (!await userManager.IsInRoleAsync(deletedUser, RoleNames.User))
                await userManager.AddToRoleAsync(deletedUser, RoleNames.User);
            usersAddedToRoles++;
        }

        return new JsonResult(new
        {
            UsersCreated = usersCreated,
            RolesCreated = rolesCreated,
            UsersAddedToRoles = usersAddedToRoles
        });
    }

    /// <summary>
    /// 批量更新书籍价格（根据分类设置合理价格）
    /// </summary>
    [HttpPut]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> BookPrices()
    {
        var books = await context.Books.ToListAsync();
        var random = new Random(42); // 固定种子保证每次结果一致
        var updatedCount = 0;

        foreach (var book in books)
        {
            // 根据索书号分类确定基础价格区间
            var category = book.Identifier.Length > 0 ? book.Identifier[0] : 'Z';
            decimal basePrice = category switch
            {
                'A' => 35m,  // 马克思主义
                'B' => 42m,  // 哲学、宗教
                'C' => 38m,  // 社会科学总论
                'D' => 45m,  // 政治、法律
                'E' => 48m,  // 军事
                'F' => 52m,  // 经济
                'G' => 39m,  // 文化、科学、教育、体育
                'H' => 36m,  // 语言、文字
                'I' => 32m,  // 文学
                'J' => 58m,  // 艺术
                'K' => 46m,  // 历史、地理
                'N' => 55m,  // 自然科学总论
                'O' => 62m,  // 数理科学和化学
                'P' => 68m,  // 天文学、地球科学
                'Q' => 56m,  // 生物科学
                'R' => 72m,  // 医药、卫生
                'S' => 48m,  // 农业科学
                'T' => 65m,  // 工业技术
                'U' => 58m,  // 交通运输
                'V' => 75m,  // 航空、航天
                'X' => 52m,  // 环境科学、安全科学
                'Z' => 38m,  // 综合性图书
                _ => 45m     // 其他
            };

            // 添加随机浮动 (-10% ~ +30%)
            var fluctuation = (decimal)(random.NextDouble() * 0.4 - 0.1);
            var price = basePrice * (1 + fluctuation);

            // 价格取整到小数点后1位，并确保以.9结尾（常见定价策略）
            price = Math.Round(price, 0) - 0.1m;
            if (price < 15m) price = 15.9m;
            if (price > 150m) price = 149.9m;

            book.Price = price;

            // 设置原价（30%的书有折扣）
            if (random.NextDouble() < 0.3)
            {
                book.OriginalPrice = Math.Round(price * (decimal)(1.2 + random.NextDouble() * 0.3), 0) - 0.1m;
            }

            updatedCount++;
        }

        await context.SaveChangesAsync();
        logger.LogInformation("更新了 {Count} 本书籍的价格", updatedCount);

        return Ok(new
        {
            Code = 0,
            Message = "更新成功",
            Data = new
            {
                UpdatedBooks = updatedCount,
                TotalBooks = books.Count
            }
        });
    }

    /// <summary>
    /// 初始化所有书籍的销售库存
    /// 为所有 SaleInventory 为 0 的书籍设置默认销售库存
    /// </summary>
    [HttpPut]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> InitSaleInventory()
    {
        var books = await context.Books
            .Where(b => b.SaleInventory == 0)
            .ToListAsync();

        var random = new Random();
        var updatedCount = 0;

        foreach (var book in books)
        {
            // 销售库存设置为借阅库存的5-10倍（随机）
            var multiplier = random.Next(5, 11);
            book.SaleInventory = Math.Max(50, book.Inventory * (uint)multiplier);
            updatedCount++;
        }

        await context.SaveChangesAsync();
        logger.LogInformation("初始化了 {Count} 本书籍的销售库存", updatedCount);

        return Ok(new
        {
            Code = 0,
            Message = "初始化销售库存成功",
            Data = new
            {
                UpdatedBooks = updatedCount,
                TotalBooks = await context.Books.CountAsync()
            }
        });
    }
}