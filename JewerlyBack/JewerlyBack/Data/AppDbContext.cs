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

            // PasswordHash nullable для пользователей через внешних провайдеров
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500);

            entity.Property(e => e.Name)
                .HasMaxLength(200);

            entity.Property(e => e.Provider)
                .HasMaxLength(50);

            entity.Property(e => e.ExternalId)
                .HasMaxLength(256);

            // Индекс для быстрого поиска по email (case-insensitive в PostgreSQL)
            entity.HasIndex(e => e.Email)
                .IsUnique();

            // Индекс для быстрого поиска по внешнему провайдеру
            entity.HasIndex(e => new { e.Provider, e.ExternalId })
                .HasFilter("\"Provider\" IS NOT NULL");

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
        // Фиксированные Id и Code для стабильного API и фронтенда
        modelBuilder.Entity<JewelryCategory>().HasData(
            new JewelryCategory
            {
                Id = 1,
                Code = "rings",
                Name = "Rings",
                Description = "Engagement and decorative rings",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 2,
                Code = "earrings",
                Name = "Earrings",
                Description = "Various types of earrings",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 3,
                Code = "pendants",
                Name = "Pendants",
                Description = "Pendants and charms",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 4,
                Code = "necklaces",
                Name = "Necklaces",
                Description = "Statement and delicate necklaces",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 5,
                Code = "bracelets",
                Name = "Bracelets",
                Description = "Bangles and chain bracelets",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 6,
                Code = "chains",
                Name = "Chains",
                Description = "Necklace and bracelet chains",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 7,
                Code = "brooches",
                Name = "Brooches",
                Description = "Decorative brooches and pins",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 8,
                Code = "cufflinks",
                Name = "Cufflinks",
                Description = "Cufflinks and tie accessories",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 9,
                Code = "piercing",
                Name = "Piercing",
                Description = "Body piercing jewelry",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 10,
                Code = "hair_jewelry",
                Name = "Hair Jewelry",
                Description = "Hair accessories and ornaments",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 11,
                Code = "sets",
                Name = "Sets",
                Description = "Matching jewelry sets",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 12,
                Code = "mens_jewelry",
                Name = "Men's Jewelry",
                Description = "Jewelry designed for men",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 13,
                Code = "custom",
                Name = "Custom Designs",
                Description = "Unique custom-made jewelry",
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
