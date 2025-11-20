using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<AppUser> Users { get; set; }
    public DbSet<JewelryCategory> JewelryCategories { get; set; }
    public DbSet<JewelryBaseModel> JewelryBaseModels { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<StoneType> StoneTypes { get; set; }
    public DbSet<JewelryConfiguration> JewelryConfigurations { get; set; }
    public DbSet<JewelryConfigurationStone> JewelryConfigurationStones { get; set; }
    public DbSet<JewelryConfigurationEngraving> JewelryConfigurationEngravings { get; set; }
    public DbSet<UploadedAsset> UploadedAssets { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // AppUser
        // ========================================
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Name)
                .HasMaxLength(200);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            // При удалении пользователя НЕ удаляем его заказы и конфигурации
            // Это важные бизнес-данные, которые должны сохраняться для истории
            entity.HasMany(e => e.Orders)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Configurations)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Assets)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // JewelryCategory
        // ========================================
        modelBuilder.Entity<JewelryCategory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            // При удалении категории НЕ удаляем базовые модели
            // Защита от случайного удаления данных
            entity.HasMany(e => e.BaseModels)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // JewelryBaseModel
        // ========================================
        modelBuilder.Entity<JewelryBaseModel>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.PreviewImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.BasePrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            // При удалении базовой модели НЕ удаляем конфигурации пользователей
            // Это защищает пользовательские данные
            entity.HasMany(e => e.Configurations)
                .WithOne(e => e.BaseModel)
                .HasForeignKey(e => e.BaseModelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // Material
        // ========================================
        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.MetalType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ColorHex)
                .HasMaxLength(7); // #RRGGBB

            entity.Property(e => e.PriceFactor)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            // При удалении материала НЕ удаляем конфигурации
            entity.HasMany(e => e.Configurations)
                .WithOne(e => e.Material)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // StoneType
        // ========================================
        modelBuilder.Entity<StoneType>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Color)
                .HasMaxLength(100);

            entity.Property(e => e.DefaultPricePerCarat)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.Code)
                .IsUnique();

            // При удалении типа камня НЕ удаляем камни в конфигурациях
            entity.HasMany(e => e.ConfigurationStones)
                .WithOne(e => e.StoneType)
                .HasForeignKey(e => e.StoneTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // JewelryConfiguration
        // ========================================
        modelBuilder.Entity<JewelryConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(200);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EstimatedPrice)
                .HasPrecision(18, 2);

            // При удалении конфигурации каскадно удаляем зависимые данные
            // (камни, гравировки) - это часть конфигурации
            entity.HasMany(e => e.Stones)
                .WithOne(e => e.Configuration)
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Engravings)
                .WithOne(e => e.Configuration)
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Для загруженных файлов - SetNull (файлы могут существовать независимо)
            entity.HasMany(e => e.Assets)
                .WithOne(e => e.Configuration)
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.SetNull);

            // При удалении конфигурации НЕ удаляем позиции заказа
            // Заказ должен сохранять историю того, что было заказано
            entity.HasMany(e => e.OrderItems)
                .WithOne(e => e.Configuration)
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // JewelryConfigurationStone
        // ========================================
        modelBuilder.Entity<JewelryConfigurationStone>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CaratWeight)
                .HasPrecision(18, 2);

            entity.Property(e => e.SizeMm)
                .HasPrecision(18, 2);
        });

        // ========================================
        // JewelryConfigurationEngraving
        // ========================================
        modelBuilder.Entity<JewelryConfigurationEngraving>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FontName)
                .HasMaxLength(100);

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.SizeMm)
                .HasPrecision(18, 2);
        });

        // ========================================
        // UploadedAsset
        // ========================================
        modelBuilder.Entity<UploadedAsset>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.OriginalFileName)
                .HasMaxLength(500);
        });

        // ========================================
        // Order
        // ========================================
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3); // ISO 4217 (EUR, USD, etc.)

            entity.HasIndex(e => e.OrderNumber)
                .IsUnique();

            // При удалении заказа каскадно удаляем позиции заказа
            entity.HasMany(e => e.Items)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========================================
        // OrderItem
        // ========================================
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.ItemPrice)
                .IsRequired()
                .HasPrecision(18, 2);
        });

        // ========================================
        // SEED DATA
        // ========================================

        // Категории ювелирных изделий
        modelBuilder.Entity<JewelryCategory>().HasData(
            new JewelryCategory
            {
                Id = 1,
                Code = "ring",
                Name = "Кольца",
                Description = "Обручальные и декоративные кольца",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 2,
                Code = "pendant",
                Name = "Подвески",
                Description = "Подвески и кулоны",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 3,
                Code = "earring",
                Name = "Серьги",
                Description = "Серьги различных типов",
                IsActive = true
            }
        );

        // Материалы
        modelBuilder.Entity<Material>().HasData(
            new Material
            {
                Id = 1,
                Code = "gold_585_yellow",
                Name = "Золото 585 жёлтое",
                MetalType = "gold",
                Karat = 14,
                ColorHex = "#FFD700",
                PriceFactor = 1.0m,
                IsActive = true
            },
            new Material
            {
                Id = 2,
                Code = "gold_585_white",
                Name = "Золото 585 белое",
                MetalType = "gold",
                Karat = 14,
                ColorHex = "#E5E4E2",
                PriceFactor = 1.1m,
                IsActive = true
            },
            new Material
            {
                Id = 3,
                Code = "silver_925",
                Name = "Серебро 925",
                MetalType = "silver",
                Karat = null,
                ColorHex = "#C0C0C0",
                PriceFactor = 0.3m,
                IsActive = true
            },
            new Material
            {
                Id = 4,
                Code = "platinum",
                Name = "Платина",
                MetalType = "platinum",
                Karat = null,
                ColorHex = "#E5E4E2",
                PriceFactor = 2.5m,
                IsActive = true
            }
        );

        // Типы камней
        modelBuilder.Entity<StoneType>().HasData(
            new StoneType
            {
                Id = 1,
                Code = "diamond",
                Name = "Бриллиант",
                Color = "Бесцветный",
                DefaultPricePerCarat = 50000.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 2,
                Code = "sapphire",
                Name = "Сапфир",
                Color = "Синий",
                DefaultPricePerCarat = 15000.0m,
                IsActive = true
            }
        );
    }
}
