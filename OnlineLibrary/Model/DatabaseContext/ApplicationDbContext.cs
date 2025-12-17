using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineLibrary.Model.DatabaseContext;

public class ApplicationDbContext : IdentityDbContext<ApiUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<Book> Books => Set<Book>();

    // User 属性不再需要，因为类定义时继承了 IdentityDbContext<ApiUser>，这个继承操作已经将 IdentityDbContext 中的 User 属性
    // 替换为ApiUser，ApiUser 对 IdentityUser 进行的扩展将反映在数据库的 AspNetUsers 表中。
    // public new DbSet<ApiUser> Users => Set<ApiUser>();

    public DbSet<CurrentBorrow> CurrentBorrows => Set<CurrentBorrow>();

    public DbSet<BorrowHistory> BorrowHistories => Set<BorrowHistory>();

    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<Recommend> Recommends => Set<Recommend>();

    public DbSet<BookComment> BookComments => Set<BookComment>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CurrentBorrow>()
            .HasKey(i => new { i.BookId, i.UserId });

        modelBuilder.Entity<CurrentBorrow>()
            .HasOne(x => x.User)
            .WithMany(y => y.CurrentBorrows)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BorrowHistory>()
            .HasKey(i => new { i.BookId, i.UserId });

        modelBuilder.Entity<BorrowHistory>()
            .HasOne(x => x.User)
            .WithMany(y => y.BorrowHistories)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BookComment>()
            .HasOne(x => x.User)
            .WithMany() // 如果 ApiUser 类中没有对应的集合导航属性，可以使用无参数的 WithMany 方法
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // 购物车项配置
        modelBuilder.Entity<CartItem>()
            .HasKey(i => new { i.BookId, i.UserId });

        modelBuilder.Entity<CartItem>()
            .HasOne(x => x.User)
            .WithMany(y => y.CartItems)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CartItem>()
            .HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // 订单配置
        modelBuilder.Entity<Order>()
            .HasOne(x => x.User)
            .WithMany(y => y.Orders)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Order>()
            .HasIndex(x => x.OrderNo)
            .IsUnique();

        // 订单项配置
        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Order)
            .WithMany(y => y.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}