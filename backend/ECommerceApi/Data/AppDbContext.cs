using Microsoft.EntityFrameworkCore;
using ECommerceApi.Models;
using ECommerceApi.Models.Enums;

namespace ECommerceApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Product =====
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.ImageUrl).HasMaxLength(500);
            entity.Property(p => p.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(p => p.UpdatedAt).HasColumnType("datetime").IsRequired();
            entity.HasIndex(p => p.Name);
        });

        // ===== Order =====
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedOnAdd();
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(o => o.Status)
                  .HasConversion<int>()
                  .IsRequired();
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(o => o.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(o => o.UpdatedAt).HasColumnType("datetime").IsRequired();
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CreatedAt);
        });

        // ===== OrderItem =====
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.Id).ValueGeneratedOnAdd();
            entity.Property(oi => oi.Quantity).IsRequired();
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(oi => oi.SubTotal).HasColumnType("decimal(18,2)").IsRequired();

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Seed demo products (MySQL compatible) =====
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "无线蓝牙耳机",
                Price = 299.99m,
                Stock = 10,
                Description = "高品质无线耳机，支持主动降噪，续航30小时。",
                ImageUrl = "https://picsum.photos/seed/headphones/400/400",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 2,
                Name = "机械键盘 RGB",
                Price = 159.99m,
                Stock = 5,
                Description = "全尺寸机械键盘，Cherry MX轴体，可自定义RGB灯效。",
                ImageUrl = "https://picsum.photos/seed/keyboard/400/400",
                CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 3,
                Name = "USB-C 扩展坞 7合1",
                Price = 89.99m,
                Stock = 8,
                Description = "紧凑型USB-C扩展坞，含HDMI、USB 3.0、SD读卡器和100W PD充电。",
                ImageUrl = "https://picsum.photos/seed/usbhub/400/400",
                CreatedAt = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 4,
                Name = "4K 网络摄像头 Pro",
                Price = 199.99m,
                Stock = 3,
                Description = "专业4K网络摄像头，支持自动对焦、内置麦克风和隐私快门。",
                ImageUrl = "https://picsum.photos/seed/webcam/400/400",
                CreatedAt = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 5,
                Name = "人体工学无线鼠标",
                Price = 129.99m,
                Stock = 7,
                Description = "垂直人体工学设计无线鼠标，可调DPI，静音按键。",
                ImageUrl = "https://picsum.photos/seed/mouse/400/400",
                CreatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
