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
    public DbSet<AiPreviewJob> AiPreviewJobs { get; set; }

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
        // AiPreviewJob
        // ========================================
        modelBuilder.Entity<AiPreviewJob>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ConfigurationId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired(false); // Nullable для гостей

            entity.Property(e => e.GuestClientId)
                .HasMaxLength(100)
                .IsRequired(false); // Nullable, заполняется только для гостей

            entity.Property(e => e.Type)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.Prompt)
                .HasMaxLength(2000);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(e => e.SingleImageUrl)
                .HasMaxLength(1000);

            entity.Property(e => e.FramesJson)
                .HasMaxLength(10000); // JSON массив с URL фреймов

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.Property(e => e.UpdatedAtUtc)
                .IsRequired();

            // Связь с конфигурацией
            entity.HasOne(e => e.Configuration)
                .WithMany()
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении конфигурации удаляем и jobs

            // Индекс для быстрого поиска по конфигурации
            entity.HasIndex(e => e.ConfigurationId);

            // Индекс для быстрого поиска по статусу (для фоновой обработки)
            entity.HasIndex(e => e.Status);

            // Композитный индекс для поиска гостевых заданий (GuestClientId + Status)
            entity.HasIndex(e => new { e.GuestClientId, e.Status })
                .HasFilter("\"GuestClientId\" IS NOT NULL");
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
            // Yellow Gold
            new Material
            {
                Id = 1,
                Code = "gold_14k_yellow",
                Name = "14K Yellow Gold",
                MetalType = "gold",
                Karat = 14,
                ColorHex = "#FFD700",
                PriceFactor = 1.0m,
                IsActive = true
            },
            new Material
            {
                Id = 2,
                Code = "gold_18k_yellow",
                Name = "18K Yellow Gold",
                MetalType = "gold",
                Karat = 18,
                ColorHex = "#FFDF00",
                PriceFactor = 1.2m,
                IsActive = true
            },
            // White Gold
            new Material
            {
                Id = 3,
                Code = "gold_14k_white",
                Name = "14K White Gold",
                MetalType = "gold",
                Karat = 14,
                ColorHex = "#E5E4E2",
                PriceFactor = 1.05m,
                IsActive = true
            },
            new Material
            {
                Id = 4,
                Code = "gold_18k_white",
                Name = "18K White Gold",
                MetalType = "gold",
                Karat = 18,
                ColorHex = "#E8E8E8",
                PriceFactor = 1.25m,
                IsActive = true
            },
            // Rose Gold
            new Material
            {
                Id = 5,
                Code = "gold_14k_rose",
                Name = "14K Rose Gold",
                MetalType = "gold",
                Karat = 14,
                ColorHex = "#B76E79",
                PriceFactor = 1.05m,
                IsActive = true
            },
            new Material
            {
                Id = 6,
                Code = "gold_18k_rose",
                Name = "18K Rose Gold",
                MetalType = "gold",
                Karat = 18,
                ColorHex = "#C9A0A0",
                PriceFactor = 1.25m,
                IsActive = true
            },
            // Platinum
            new Material
            {
                Id = 7,
                Code = "platinum",
                Name = "Platinum",
                MetalType = "platinum",
                Karat = null,
                ColorHex = "#E5E4E2",
                PriceFactor = 1.4m,
                IsActive = true
            },
            // Silver
            new Material
            {
                Id = 8,
                Code = "silver_925",
                Name = "Sterling Silver 925",
                MetalType = "silver",
                Karat = null,
                ColorHex = "#C0C0C0",
                PriceFactor = 0.6m,
                IsActive = true
            },
            // Titanium
            new Material
            {
                Id = 9,
                Code = "titanium",
                Name = "Titanium",
                MetalType = "titanium",
                Karat = null,
                ColorHex = "#878681",
                PriceFactor = 0.8m,
                IsActive = true
            }
        );

        // Типы камней
        modelBuilder.Entity<StoneType>().HasData(
            new StoneType
            {
                Id = 1,
                Code = "diamond",
                Name = "Diamond",
                Color = "Clear",
                DefaultPricePerCarat = 5000.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 2,
                Code = "sapphire",
                Name = "Sapphire",
                Color = "Blue",
                DefaultPricePerCarat = 1500.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 3,
                Code = "ruby",
                Name = "Ruby",
                Color = "Red",
                DefaultPricePerCarat = 1800.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 4,
                Code = "emerald",
                Name = "Emerald",
                Color = "Green",
                DefaultPricePerCarat = 2000.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 5,
                Code = "moissanite",
                Name = "Moissanite",
                Color = "Clear",
                DefaultPricePerCarat = 400.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 6,
                Code = "topaz",
                Name = "Topaz",
                Color = "Blue",
                DefaultPricePerCarat = 250.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 7,
                Code = "amethyst",
                Name = "Amethyst",
                Color = "Purple",
                DefaultPricePerCarat = 150.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 8,
                Code = "citrine",
                Name = "Citrine",
                Color = "Yellow",
                DefaultPricePerCarat = 180.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 9,
                Code = "aquamarine",
                Name = "Aquamarine",
                Color = "Light Blue",
                DefaultPricePerCarat = 300.0m,
                IsActive = true
            },
            new StoneType
            {
                Id = 10,
                Code = "garnet",
                Name = "Garnet",
                Color = "Deep Red",
                DefaultPricePerCarat = 200.0m,
                IsActive = true
            }
        );

        // Базовые модели ювелирных изделий
        modelBuilder.Entity<JewelryBaseModel>().HasData(
            // Rings (CategoryId = 1)
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                CategoryId = 1,
                Code = "ring_solitaire_classic",
                Name = "Classic Solitaire Ring",
                Description = "Elegant thin band with a single center stone in prong setting",
                BasePrice = 500.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"defaultRingSize\":16.5,\"bandWidth\":2.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                CategoryId = 1,
                Code = "ring_engagement_halo",
                Name = "Halo Engagement Ring",
                Description = "Center stone surrounded by a halo of smaller accent stones",
                BasePrice = 800.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"defaultRingSize\":16.5,\"bandWidth\":2.5}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                CategoryId = 1,
                Code = "ring_wide_band",
                Name = "Wide Band Ring",
                Description = "Modern wide band with smooth polished surface",
                BasePrice = 400.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"defaultRingSize\":17.0,\"bandWidth\":5.0}"
            },

            // Earrings (CategoryId = 2)
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                CategoryId = 2,
                Code = "earring_stud_classic",
                Name = "Classic Stud Earrings",
                Description = "Minimalist studs with a single gemstone",
                BasePrice = 300.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"stoneSize\":5.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                CategoryId = 2,
                Code = "earring_hoop_medium",
                Name = "Medium Hoop Earrings",
                Description = "Classic round hoops, medium size",
                BasePrice = 250.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"diameter\":25.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                CategoryId = 2,
                Code = "earring_drop_elegant",
                Name = "Elegant Drop Earrings",
                Description = "Graceful drop earrings with dangling gemstone",
                BasePrice = 450.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"length\":35.0}"
            },

            // Pendants (CategoryId = 3)
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                CategoryId = 3,
                Code = "pendant_round_simple",
                Name = "Round Pendant",
                Description = "Simple round pendant with center stone",
                BasePrice = 200.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"diameter\":15.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                CategoryId = 3,
                Code = "pendant_heart_classic",
                Name = "Heart Pendant",
                Description = "Classic heart-shaped pendant",
                BasePrice = 220.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"width\":12.0,\"height\":12.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                CategoryId = 3,
                Code = "pendant_solitaire",
                Name = "Solitaire Pendant",
                Description = "Single stone pendant in prong setting",
                BasePrice = 280.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"stoneSize\":6.0}"
            },

            // Necklaces (CategoryId = 4)
            new JewelryBaseModel
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                CategoryId = 4,
                Code = "necklace_cable_chain",
                Name = "Cable Chain Necklace",
                Description = "Classic cable chain necklace",
                BasePrice = 350.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"length\":45.0,\"linkSize\":3.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                CategoryId = 4,
                Code = "necklace_pendant_base",
                Name = "Pendant Necklace Base",
                Description = "Delicate chain designed for pendants",
                BasePrice = 180.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"length\":42.0,\"linkSize\":2.0}"
            },

            // Bracelets (CategoryId = 5)
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                CategoryId = 5,
                Code = "bracelet_chain_classic",
                Name = "Classic Chain Bracelet",
                Description = "Simple elegant chain bracelet",
                BasePrice = 280.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"length\":18.0,\"linkSize\":3.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                CategoryId = 5,
                Code = "bracelet_bangle_simple",
                Name = "Simple Bangle Bracelet",
                Description = "Smooth round bangle bracelet",
                BasePrice = 320.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"diameter\":65.0,\"width\":4.0}"
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                CategoryId = 5,
                Code = "bracelet_tennis",
                Name = "Tennis Bracelet",
                Description = "Classic tennis bracelet with line of stones",
                BasePrice = 950.0m,
                IsActive = true,
                PreviewImageUrl = null,
                MetadataJson = "{\"length\":18.0,\"stoneCount\":20}"
            }
        );
    }
}
