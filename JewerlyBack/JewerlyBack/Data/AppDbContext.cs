using JewerlyBack.Entities;
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
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UpgradeAnalysis> UpgradeAnalyses { get; set; }
    public DbSet<UpgradePreviewJob> UpgradePreviewJobs { get; set; }

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

            entity.Property(e => e.AiCategoryDescription)
                .HasColumnType("text");

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

            entity.Property(e => e.AiDescription)
                .HasColumnType("text");

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
                .IsRequired();

            entity.Property(e => e.EngravingText)
                .HasMaxLength(100);

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

            // Index for efficient stone lookup by configuration
            entity.HasIndex(e => e.ConfigurationId)
                .HasDatabaseName("IX_ConfigurationStones_ConfigurationId");
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
                .HasMaxLength(8000); // Increased for longer AI prompts with detailed context

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
        // AuditLog
        // ========================================
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Changes)
                .HasColumnType("text");

            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            // Index for querying by entity
            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

            // Index for querying by user
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            // Index for querying by timestamp
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");
        });

        // ========================================
        // UpgradeAnalysis
        // ========================================
        modelBuilder.Entity<UpgradeAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired(false);

            entity.Property(e => e.GuestClientId)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(e => e.OriginalImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.JewelryType)
                .HasMaxLength(100);

            entity.Property(e => e.DetectedMetal)
                .HasMaxLength(100);

            entity.Property(e => e.DetectedMetalDescription)
                .HasMaxLength(500);

            entity.Property(e => e.StyleClassification)
                .HasMaxLength(100);

            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(5, 4);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.Property(e => e.UpdatedAtUtc)
                .IsRequired();

            // Relationship with user
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship with category
            entity.HasOne(e => e.DetectedCategory)
                .WithMany()
                .HasForeignKey(e => e.DetectedCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Index for user queries
            entity.HasIndex(e => e.UserId)
                .HasFilter("\"UserId\" IS NOT NULL");

            // Index for guest queries
            entity.HasIndex(e => e.GuestClientId)
                .HasFilter("\"GuestClientId\" IS NOT NULL");

            // Index for status queries
            entity.HasIndex(e => e.Status);
        });

        // ========================================
        // UpgradePreviewJob
        // ========================================
        modelBuilder.Entity<UpgradePreviewJob>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AnalysisId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired(false);

            entity.Property(e => e.GuestClientId)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.Prompt)
                .HasMaxLength(8000);

            entity.Property(e => e.EnhancedImageUrl)
                .HasMaxLength(1000);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.Property(e => e.UpdatedAtUtc)
                .IsRequired();

            // Relationship with analysis
            entity.HasOne(e => e.Analysis)
                .WithMany(e => e.PreviewJobs)
                .HasForeignKey(e => e.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with user
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Index for analysis queries
            entity.HasIndex(e => e.AnalysisId);

            // Index for status queries
            entity.HasIndex(e => e.Status);
        });

        // ========================================
        // SEED DATA
        // ========================================

        // Категории ювелирных изделий
        // Фиксированные Id и Code для стабильного API и фронтенда
        // Канонический список соответствует файлам в lib/assets/categories
        modelBuilder.Entity<JewelryCategory>().HasData(
            new JewelryCategory
            {
                Id = 1,
                Code = "rings",
                Name = "Rings",
                Description = "Engagement and decorative rings",
                AiCategoryDescription = "A ring is a circular band worn on the finger, ranging from simple bands to elaborate designs with stones. Rings can be engagement rings with prominent center stones, wedding bands, fashion rings, or signet rings. They are designed to encircle the finger smoothly and can feature various widths, profiles, and decorative elements.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 2,
                Code = "earrings",
                Name = "Earrings",
                Description = "Various types of earrings",
                AiCategoryDescription = "Earrings are jewelry pieces worn on or hanging from the earlobe or ear cartilage. They include studs that sit close to the ear, hoops that form circular shapes, drop earrings that dangle below the lobe, and more elaborate chandelier styles. Earrings are typically sold and worn as matching pairs.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 3,
                Code = "pendants",
                Name = "Pendants",
                Description = "Pendants and charms",
                AiCategoryDescription = "A pendant is a decorative element designed to hang from a chain or cord around the neck. Pendants can be geometric shapes, symbols, stones in settings, or representational forms. They typically feature a bail or loop at the top for attachment and serve as the focal point of a necklace.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 4,
                Code = "necklaces",
                Name = "Necklaces",
                Description = "Statement and delicate necklaces",
                AiCategoryDescription = "A necklace is a complete piece of jewelry that encircles the neck, including both the chain or structure and any decorative elements. Necklaces can be simple chains, tennis necklaces with continuous stones, chokers that sit high on the neck, or statement pieces with integrated pendants or decorative sections.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 5,
                Code = "bracelets",
                Name = "Bracelets",
                Description = "Bangles and chain bracelets",
                AiCategoryDescription = "A bracelet is jewelry worn around the wrist, either as a flexible chain with a clasp or as a rigid bangle that slips over the hand. Bracelets can be delicate chains, tennis bracelets with continuous stones, charm bracelets with dangling elements, or solid cuffs and bangles.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 6,
                Code = "chains",
                Name = "Chains",
                Description = "Necklace and bracelet chains",
                AiCategoryDescription = "A chain is a flexible series of connected metal links forming a continuous strand. Chains are worn as standalone jewelry or used as the foundation for pendants. Common styles include cable chains with round links, curb chains with flat links, rope chains with twisted construction, and box chains with square links.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 7,
                Code = "brooches",
                Name = "Brooches",
                Description = "Decorative brooches and pins",
                AiCategoryDescription = "A brooch is a decorative pin with a clasp mechanism on the back, designed to attach to clothing or fabric. Brooches can be floral designs, geometric shapes, animal figures, or abstract forms. They sit flat against fabric and serve as visible decorative accents on lapels, collars, or other garment areas.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 8,
                Code = "cufflinks",
                Name = "Cufflinks",
                Description = "Cufflinks and tie accessories",
                AiCategoryDescription = "Cufflinks are pairs of decorative fasteners used to secure the cuffs of dress shirts. They consist of a decorative front face connected to a backing mechanism that passes through buttonholes. Cufflinks are typically worn in formal settings and feature geometric, engraved, or stone-set designs.",
                IsActive = false // Деактивировано - нет в каноническом списке
            },
            new JewelryCategory
            {
                Id = 9,
                Code = "piercing",
                Name = "Piercing",
                Description = "Body piercing jewelry",
                AiCategoryDescription = "Piercing jewelry is designed for body piercings beyond standard earlobe piercings, including cartilage, nose, lip, eyebrow, navel, and other locations. Common styles include labret studs with flat backs, small hoops, straight and curved barbells. These pieces are typically small, secure, and designed for continuous wear.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 10,
                Code = "hair_jewelry",
                Name = "Hair jewelry",
                Description = "Hair accessories and ornaments",
                AiCategoryDescription = "Hair jewelry includes decorative accessories designed to adorn or secure hair. This includes hair pins with decorative tops, ornate hair combs with stones or metalwork, hair clips, and decorative barrettes. These pieces combine functionality with aesthetic appeal and are visible when worn in hairstyles.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 11,
                Code = "sets",
                Name = "Sets",
                Description = "Matching jewelry sets",
                AiCategoryDescription = "A jewelry set is a coordinated collection of matching pieces designed to be worn together, such as a necklace and earrings, or a ring and bracelet combination. Sets share design elements, materials, and style to create a cohesive look.",
                IsActive = false // Деактивировано - нет в каноническом списке
            },
            new JewelryCategory
            {
                Id = 12,
                Code = "mens_jewelry",
                Name = "Men's jewelry",
                Description = "Jewelry designed for men",
                AiCategoryDescription = "Men's jewelry includes pieces designed with masculine proportions and aesthetic, such as heavier chain necklaces, substantial link bracelets, bold signet rings, and cufflinks. These pieces typically feature larger dimensions, stronger lines, and more substantial construction compared to traditional jewelry.",
                IsActive = true
            },
            new JewelryCategory
            {
                Id = 13,
                Code = "custom",
                Name = "Custom Designs",
                Description = "Unique custom-made jewelry",
                AiCategoryDescription = "Custom jewelry encompasses unique, made-to-order pieces designed specifically for an individual customer. These pieces can be any category but are characterized by personalized design elements, custom proportions, unique stone arrangements, or special engravings that make them one-of-a-kind creations.",
                IsActive = false // Деактивировано - нет в каноническом списке
            },
            new JewelryCategory
            {
                Id = 14,
                Code = "cross_pendants",
                Name = "Cross pendants",
                Description = "Religious cross pendants and charms",
                AiCategoryDescription = "Cross pendants are religious or symbolic jewelry pieces in the shape of a cross, designed to hang from a chain. They range from simple Latin crosses with clean lines to elaborate Orthodox crosses with multiple bars, and can be plain metal or embellished with stones. Cross pendants serve both as expressions of faith and as decorative jewelry.",
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

        // ========================================
        // Базовые модели ювелирных изделий
        // Каноническ��й список для конструктора и AI конфигурации
        // ========================================
        modelBuilder.Entity<JewelryBaseModel>().HasData(
            // ========================================
            // RINGS (CategoryId = 1)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                CategoryId = 1,
                Code = "classic_solid_band",
                Name = "Classic Solid Band",
                Description = "Simple solid metal band with smooth polished surface, uniform width throughout",
                AiDescription = "A classic solid band ring with a smooth, even surface and medium width. The profile is gently rounded for everyday wear.",
                BasePrice = 250.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                CategoryId = 1,
                Code = "thin_band_elevated_setting",
                Name = "Thin Band with Elevated Setting",
                Description = "Delicate thin band featuring a minimal raised setting structure at the center, designed for a lightweight elegant appearance",
                AiDescription = "A delicate thin band ring with a small elevated setting structure at the top, emphasizing minimalism and lightness with a refined central focal point.",
                BasePrice = 400.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                CategoryId = 1,
                Code = "classic_elevated_setting_ring",
                Name = "Classic Elevated Setting Ring",
                Description = "Classic ring with a prominent raised central setting structure on a slender band, elegant silhouette with elevated focal point",
                AiDescription = "A ring with a raised central setting structure featuring an elegant elevated profile, with a clean band that draws attention to the central focal point.",
                BasePrice = 800.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                CategoryId = 1,
                Code = "continuous_setting_band",
                Name = "Continuous Setting Band",
                Description = "Narrow band with a continuous row of evenly spaced small setting structures encircling the entire ring",
                AiDescription = "A narrow ring with a continuous line of evenly spaced small setting structures going all the way around the band, creating a uniform decorative pattern from every angle.",
                BasePrice = 950.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                CategoryId = 1,
                Code = "signet_ring",
                Name = "Signet Ring",
                Description = "Broad band with a flat rectangular or oval top surface suitable for engraving or decorative elements",
                AiDescription = "A solid, slightly heavier ring with a flat or gently curved top surface intended for a symbol or engraving, with a strong, masculine silhouette.",
                BasePrice = 450.0m,
                IsActive = true
            },

            // ========================================
            // EARRINGS (CategoryId = 2)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                CategoryId = 2,
                Code = "classic_stud",
                Name = "Classic Stud",
                Description = "Simple stud earring with a decorative element set directly on a post, minimalist and close to the earlobe",
                AiDescription = "A small stud earring with a decorative element or smooth disc sitting close to the earlobe, mounted on a straight post with a simple backing.",
                BasePrice = 300.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                CategoryId = 2,
                Code = "cluster_design_stud",
                Name = "Cluster Design Stud",
                Description = "Stud earring featuring multiple small setting structures arranged in a tight decorative cluster pattern",
                AiDescription = "A stud earring formed by a tight cluster of several small decorative elements arranged into a compact ornamental shape that sits on the earlobe.",
                BasePrice = 450.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                CategoryId = 2,
                Code = "small_hoop",
                Name = "Small Hoop",
                Description = "Smooth metal hoop in a small diameter, simple circular shape that hugs the earlobe",
                AiDescription = "A small, smooth hoop earring that hugs the earlobe closely, with a continuous circular or slightly oval shape and polished surface.",
                BasePrice = 220.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                CategoryId = 2,
                Code = "embellished_hoop",
                Name = "Embellished Hoop",
                Description = "Hoop earring with small decorative settings along the outer edge, adding visual interest to the classic hoop design",
                AiDescription = "A medium-sized hoop earring with decorative setting structures along the visible outer front section of the hoop, combining a clean circular form with an embellished accent line.",
                BasePrice = 550.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                CategoryId = 2,
                Code = "simple_drop_earring",
                Name = "Simple Drop Earring",
                Description = "Earring with a decorative element suspended below the earlobe on a short chain or wire",
                AiDescription = "A minimalist drop earring where a small pendant element hangs from a short connector, creating a light movement just below the earlobe.",
                BasePrice = 380.0m,
                IsActive = true
            },

            // ========================================
            // PENDANTS (CategoryId = 3)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                CategoryId = 3,
                Code = "round_disc_pendant",
                Name = "Round Disc Pendant",
                Description = "Flat circular disc pendant with smooth polished surface, versatile minimalist design",
                AiDescription = "A flat round disc pendant with a polished surface and a small bail at the top, featuring clean minimalist styling.",
                BasePrice = 180.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                CategoryId = 3,
                Code = "bar_pendant_vertical",
                Name = "Bar Pendant (Vertical)",
                Description = "Slender vertical bar pendant with clean lines and modern aesthetic, suspended by the top edge",
                AiDescription = "A narrow vertical bar pendant with clean straight edges and a slim rectangular shape, hanging from a small bail for a modern look.",
                BasePrice = 200.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                CategoryId = 3,
                Code = "heart_outline_pendant",
                Name = "Heart Outline Pendant",
                Description = "Open heart shape formed by a thin metal outline, romantic and delicate design",
                AiDescription = "A pendant in the shape of a heart outline with open center, made from a smooth metal contour that feels light and romantic.",
                BasePrice = 220.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                CategoryId = 3,
                Code = "elevated_setting_pendant",
                Name = "Elevated Setting Pendant",
                Description = "Pendant with a central raised setting structure as the focal point, suspended from a chain",
                AiDescription = "A pendant built around a central elevated setting structure, suspended from a small bail so the setting becomes the main focus.",
                BasePrice = 350.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
                CategoryId = 3,
                Code = "open_circle_pendant",
                Name = "Open Circle Pendant",
                Description = "Circular ring pendant with open center, representing continuity and eternity",
                AiDescription = "A simple open circle pendant with a smooth round contour and empty center, symbolizing continuity and minimalism.",
                BasePrice = 190.0m,
                IsActive = true
            },

            // ========================================
            // NECKLACES (CategoryId = 4)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                CategoryId = 4,
                Code = "chain_small_central_pendant",
                Name = "Chain with Small Central Pendant",
                Description = "Delicate chain with a small decorative pendant or charm positioned at the center front",
                AiDescription = "A fine chain necklace with a single small pendant fixed in the center so it always rests at the front of the neck.",
                BasePrice = 280.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                CategoryId = 4,
                Code = "continuous_setting_necklace",
                Name = "Continuous Setting Necklace",
                Description = "Continuous line of identical setting structures arranged side by side, creating an elegant collar effect",
                AiDescription = "A continuous line necklace made of closely arranged uniform setting structures, forming a flexible decorative band around the neck.",
                BasePrice = 1200.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                CategoryId = 4,
                Code = "minimal_choker_band",
                Name = "Minimal Choker Band",
                Description = "Simple thin metal band that sits snugly around the neck, modern and streamlined",
                AiDescription = "A short, close-fitting necklace that sits high on the neck, designed as a smooth, minimal band without large dangling elements.",
                BasePrice = 220.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                CategoryId = 4,
                Code = "name_plate_necklace",
                Name = "Name Plate Necklace",
                Description = "Horizontal rectangular plate attached to a chain, designed for personalized text or decorative elements",
                AiDescription = "A necklace with a horizontal plate element at the center of a fine chain, suitable for personalization or decorative styling.",
                BasePrice = 250.0m,
                IsActive = true
            },

            // ========================================
            // BRACELETS (CategoryId = 5)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                CategoryId = 5,
                Code = "chain_bracelet",
                Name = "Chain Bracelet",
                Description = "Flexible linked chain bracelet with clasp closure, elegant and adjustable",
                AiDescription = "A classic chain bracelet composed of repeating metal links, flexible and lightweight, closing with a simple clasp.",
                BasePrice = 280.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                CategoryId = 5,
                Code = "solid_bangle",
                Name = "Solid Bangle",
                Description = "Rigid circular bracelet in solid metal, slips over the hand without a clasp",
                AiDescription = "A rigid bangle bracelet formed as a closed or nearly closed ring, with a smooth exterior surface and consistent thickness.",
                BasePrice = 320.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                CategoryId = 5,
                Code = "continuous_setting_bracelet",
                Name = "Continuous Setting Bracelet",
                Description = "Line bracelet with a continuous row of individually linked setting structures",
                AiDescription = "A bracelet made of a single row of evenly spaced setting structures linked closely together, creating a continuous decorative line around the wrist.",
                BasePrice = 950.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                CategoryId = 5,
                Code = "charm_bracelet",
                Name = "Charm Bracelet",
                Description = "Chain bracelet with attachment points for hanging decorative charms or pendants",
                AiDescription = "A bracelet with a series of small pendants or charms attached to a base chain, allowing multiple decorative elements to dangle.",
                BasePrice = 300.0m,
                IsActive = true
            },

            // ========================================
            // CHAINS (CategoryId = 6)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                CategoryId = 6,
                Code = "cable_chain",
                Name = "Cable Chain",
                Description = "Classic chain with uniform oval or round links connected in a simple alternating pattern",
                AiDescription = "A simple cable chain made from uniform round or oval links connected in a straightforward pattern, suitable for pendants or standalone wear.",
                BasePrice = 150.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000002"),
                CategoryId = 6,
                Code = "curb_chain",
                Name = "Curb Chain",
                Description = "Chain with interlocking uniform links that lie flat when worn, creating a smooth surface",
                AiDescription = "A curb chain with flattened, twisted links that lie smoothly against the skin, giving a slightly heavier and more masculine look.",
                BasePrice = 170.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000003"),
                CategoryId = 6,
                Code = "figaro_chain",
                Name = "Figaro Chain",
                Description = "Chain with alternating pattern of short and long oval links, typically three short links followed by one elongated link",
                AiDescription = "A Figaro chain with a repeating pattern of one or two shorter links followed by a longer link, creating a rhythmic, stylish structure.",
                BasePrice = 160.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000004"),
                CategoryId = 6,
                Code = "rope_chain",
                Name = "Rope Chain",
                Description = "Chain with small links twisted together to resemble rope texture, creating a thick and durable design",
                AiDescription = "A rope chain built from twisted links that visually mimic a rope, with a textured, three-dimensional appearance and continuous spiral look.",
                BasePrice = 200.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000005"),
                CategoryId = 6,
                Code = "box_chain",
                Name = "Box Chain",
                Description = "Chain with square links forming a smooth, continuous tube-like appearance",
                AiDescription = "A chain composed of small square or box-shaped links, forming a strong, geometric and slightly more structured profile.",
                BasePrice = 180.0m,
                IsActive = true
            },

            // ========================================
            // BROOCHES (CategoryId = 7)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                CategoryId = 7,
                Code = "floral_brooch",
                Name = "Floral Brooch",
                Description = "Decorative pin with flower-inspired design featuring petals radiating from a central focal point",
                AiDescription = "A brooch shaped like a stylized flower with petals radiating from the center, designed to sit flat on fabric and add a decorative accent.",
                BasePrice = 280.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000002"),
                CategoryId = 7,
                Code = "geometric_bar_brooch",
                Name = "Geometric Bar Brooch",
                Description = "Horizontal bar pin with clean geometric lines and modern aesthetic",
                AiDescription = "An elongated bar-shaped brooch with clean geometric lines, often worn horizontally or diagonally as a subtle modern statement.",
                BasePrice = 220.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000003"),
                CategoryId = 7,
                Code = "animal_shape_brooch",
                Name = "Animal Shape Brooch",
                Description = "Decorative pin shaped like an animal or creature, with detailed metalwork and enamel accents",
                AiDescription = "A brooch representing the silhouette or detailed figure of an animal, with contours and details emphasizing its character.",
                BasePrice = 300.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("70000000-0000-0000-0000-000000000004"),
                CategoryId = 7,
                Code = "monogram_brooch",
                Name = "Monogram Brooch",
                Description = "Pin featuring stylized initials or letters in an elegant font design",
                AiDescription = "A brooch based on one or more letters intertwined into a decorative monogram, designed to stand out on clothing with refined lines.",
                BasePrice = 250.0m,
                IsActive = true
            },

            // ========================================
            // PIERCING (CategoryId = 9)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
                CategoryId = 9,
                Code = "labret_stud",
                Name = "Labret Stud",
                Description = "Flat-back piercing stud with decorative front, suitable for lip, ear cartilage, or other piercings",
                AiDescription = "A piercing jewelry piece with a flat disc on one end and a decorative top on the other, designed to sit flush against the skin.",
                BasePrice = 120.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000002"),
                CategoryId = 9,
                Code = "hoop_piercing",
                Name = "Hoop Piercing",
                Description = "Circular or semi-circular hoop for various piercing locations, with secure closure mechanism",
                AiDescription = "A small, smooth hoop for piercing that forms a mostly closed circle, suitable for ears, nose, or other placements.",
                BasePrice = 100.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000003"),
                CategoryId = 9,
                Code = "straight_barbell",
                Name = "Straight Barbell",
                Description = "Straight bar with threaded balls or decorative ends on both sides, used for tongue, nipple, or industrial piercings",
                AiDescription = "A straight bar piercing with a central shaft and a removable ball or decorative element on each end, used for tongue or ear piercings.",
                BasePrice = 90.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("90000000-0000-0000-0000-000000000004"),
                CategoryId = 9,
                Code = "curved_barbell",
                Name = "Curved Barbell",
                Description = "Gently curved bar with threaded ends, commonly used for eyebrow, navel, or rook piercings",
                AiDescription = "A gently curved barbell with beads or decorative ends, intended for areas like the eyebrow or navel where the curve follows the body.",
                BasePrice = 95.0m,
                IsActive = true
            },

            // ========================================
            // HAIR JEWELRY (CategoryId = 10)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                CategoryId = 10,
                Code = "decorative_top_hair_pin",
                Name = "Decorative Top Hair Pin",
                Description = "Simple hair pin with a decorative element at the top",
                AiDescription = "A slim hair pin with a decorative element mounted at one end, meant to be partially visible in the hairstyle.",
                BasePrice = 80.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                CategoryId = 10,
                Code = "cluster_decorative_hair_pin",
                Name = "Cluster Decorative Hair Pin",
                Description = "Hair pin featuring a cluster of decorative elements in an ornate design",
                AiDescription = "A hair pin with a small cluster of decorative elements arranged near the tip, creating a more pronounced ornamental accent in the hair.",
                BasePrice = 120.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                CategoryId = 10,
                Code = "decorative_hair_comb",
                Name = "Decorative Hair Comb",
                Description = "Hair comb with decorative top edge embellished with metalwork patterns",
                AiDescription = "A short comb with multiple teeth that slide into the hair and a decorative top bar featuring ornamental metal motifs.",
                BasePrice = 150.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"),
                CategoryId = 10,
                Code = "minimal_bar_hair_clip",
                Name = "Minimal Bar Hair Clip",
                Description = "Simple metal bar hair clip with clean lines and minimal decoration",
                AiDescription = "A sleek bar-style hair clip with a clean rectangular front piece, designed to hold a section of hair with minimal visual clutter.",
                BasePrice = 60.0m,
                IsActive = true
            },

            // ========================================
            // MEN'S JEWELRY (CategoryId = 12)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("c0000000-0000-0000-0000-000000000001"),
                CategoryId = 12,
                Code = "mens_signet_ring",
                Name = "Men's Signet Ring",
                Description = "Bold signet ring with wide band and large flat top surface, suitable for engraving or emblem",
                AiDescription = "A substantial men's signet ring with a flat or lightly domed top face and thicker band, designed for a bold, classic masculine statement.",
                BasePrice = 500.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("c0000000-0000-0000-0000-000000000002"),
                CategoryId = 12,
                Code = "mens_chain_necklace",
                Name = "Men's Chain Necklace",
                Description = "Substantial chain necklace with heavier gauge links, masculine and durable design",
                AiDescription = "A medium-thickness chain necklace with sturdy links and a slightly heavier feel, intended for men's everyday wear.",
                BasePrice = 400.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("c0000000-0000-0000-0000-000000000003"),
                CategoryId = 12,
                Code = "mens_link_bracelet",
                Name = "Men's Link Bracelet",
                Description = "Heavy link bracelet with robust construction and masculine proportions",
                AiDescription = "A bracelet made of larger, stronger links that form a solid masculine chain around the wrist, closing with a robust clasp.",
                BasePrice = 450.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("c0000000-0000-0000-0000-000000000004"),
                CategoryId = 12,
                Code = "classic_cufflinks",
                Name = "Classic Cufflinks",
                Description = "Pair of dress shirt cufflinks with simple geometric shape, suitable for formal wear",
                AiDescription = "A pair of classic cufflinks with a flat or slightly domed decorative front face and a hinged back part that secures the cuff.",
                BasePrice = 180.0m,
                IsActive = true
            },

            // ========================================
            // CROSS PENDANTS (CategoryId = 14)
            // ========================================
            new JewelryBaseModel
            {
                Id = Guid.Parse("e0000000-0000-0000-0000-000000000001"),
                CategoryId = 14,
                Code = "plain_latin_cross",
                Name = "Plain Latin Cross",
                Description = "Simple Latin cross with clean lines and smooth surface, traditional proportions with longer vertical beam",
                AiDescription = "A simple Latin cross pendant with clean straight arms and a slightly elongated vertical bar.",
                BasePrice = 150.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("e0000000-0000-0000-0000-000000000002"),
                CategoryId = 14,
                Code = "orthodox_style_cross",
                Name = "Orthodox-Style Cross",
                Description = "Three-bar cross design in Orthodox tradition, featuring slanted lower bar and detailed proportions",
                AiDescription = "A pendant in an Orthodox-style cross shape with characteristic additional crossbars and more intricate contours.",
                BasePrice = 180.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("e0000000-0000-0000-0000-000000000003"),
                CategoryId = 14,
                Code = "embellished_cross",
                Name = "Embellished Cross",
                Description = "Latin cross with decorative setting structures along the beams or at intersection points",
                AiDescription = "A cross pendant with small decorative settings along the arms, adding visual interest while preserving the clear cross silhouette.",
                BasePrice = 280.0m,
                IsActive = true
            },
            new JewelryBaseModel
            {
                Id = Guid.Parse("e0000000-0000-0000-0000-000000000004"),
                CategoryId = 14,
                Code = "minimal_thin_cross",
                Name = "Minimal Thin Cross",
                Description = "Delicate cross with very thin wire-like construction, modern and understated design",
                AiDescription = "A very slim, minimal cross pendant with narrow arms and a lightweight appearance, emphasizing simplicity and elegance.",
                BasePrice = 120.0m,
                IsActive = true
            }
        );
    }
}
