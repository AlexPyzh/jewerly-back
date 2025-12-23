using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.S3;
using FluentValidation;
using FluentValidation.AspNetCore;
using JewerlyBack.Application.Ai;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Infrastructure.Ai;
using JewerlyBack.Infrastructure.Ai.Configuration;
using JewerlyBack.Infrastructure.Auth;
using JewerlyBack.Infrastructure.Configuration;
using JewerlyBack.Infrastructure.Middleware;
using JewerlyBack.Infrastructure.Storage;
using JewerlyBack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Ensure environment variables are loaded
builder.Configuration.AddEnvironmentVariables();

// ========================================
// Logging Configuration
// ========================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
    // TODO: В production добавить Serilog с sink в ELK/Seq/CloudWatch
    // builder.Host.UseSerilog(...);
}

// ========================================
// Controllers & JSON Configuration
// ========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // Removed JsonStringEnumConverter to serialize enums as integers (0, 1, etc.)
        // This is required for Flutter admin which expects integer enum values
    });

// ========================================
// FluentValidation Configuration
// ========================================
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();

// ========================================
// CORS Configuration
// ========================================
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection(CorsOptions.SectionName));

var corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>()
    ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: разрешаем указанные localhost origins + любые заголовки/методы
            if (corsOptions.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
            else
            {
                // Fallback: разрешаем всё для разработки
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
        }
        else
        {
            // Production: только явно указанные origins из конфигурации
            // TODO: В Production замените AllowedOrigins в appsettings.json на реальные домены
            // Например: ["https://app.jewerly.com", "https://jewerly.com"]
            if (corsOptions.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                      .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin")
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains(); // Разрешаем поддомены
            }
            else
            {
                throw new InvalidOperationException(
                    "CORS AllowedOrigins must be configured in Production. " +
                    "Add 'Cors:AllowedOrigins' to appsettings.json");
            }
        }
    });
});

// ========================================
// Swagger Configuration
// ========================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Jewerly API",
        Version = "v1",
        Description = @"
# Jewerly API

REST API для мобильного приложения Jewerly (кастомные ювелирные изделия).

## Аутентификация

API использует JWT Bearer токены.

### Как получить токен:
1. **Регистрация**: `POST /api/account/register`
2. **Вход**: `POST /api/account/login`
3. **Google Sign-In**: `POST /api/account/google`
4. **Apple Sign-In**: `POST /api/account/apple`

### Как использовать токен:
После успешной аутентификации вы получите `accessToken` в ответе.

В Swagger UI:
- Нажмите кнопку **Authorize** 🔒
- Введите токен в формате: `Bearer YOUR_ACCESS_TOKEN` или просто `YOUR_ACCESS_TOKEN`
- Нажмите **Authorize**

Во Flutter/HTTP клиентах добавьте заголовок:
```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

## Основные разделы API

- **Account** — регистрация, вход, профиль
- **Catalog** — категории, материалы, камни, базовые модели (публичные endpoints)
- **Configurations** — создание и управление конфигурациями изделий
- **Assets** — загрузка файлов (паттерны, текстуры, изображения)
- **Orders** — создание и управление заказами
- **Health** — health check endpoints для мониторинга
",
        Contact = new OpenApiContact
        {
            Name = "Jewerly Support",
            Email = "support@jewerly.app"
        }
    });

    // TODO: Включить XML-комментарии для более детальной документации
    // Раскомментируйте после добавления <GenerateDocumentationFile>true</GenerateDocumentationFile> в .csproj:
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // if (File.Exists(xmlPath))
    // {
    //     options.IncludeXmlComments(xmlPath);
    // }

    // JWT Bearer authentication в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header с использованием Bearer scheme.

**Как использовать:**
1. Получите токен через `/api/account/login`, `/api/account/register`, или OAuth endpoints
2. Введите **только токен** (без слова 'Bearer') в поле ниже
3. Нажмите **Authorize**

Пример токена:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Swagger автоматически добавит префикс 'Bearer' к вашему токену."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Группировка по тегам для лучшей навигации
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Unknown" };
    });

    options.OrderActionsBy(api => api.RelativePath);
});

// ========================================
// Database Configuration
// ========================================
var connectionStringKey = builder.Environment.IsDevelopment()
    ? "DebugConnectionString"
    : "ReleaseConnectionString";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(connectionStringKey),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);

            npgsqlOptions.CommandTimeout(30);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// ========================================
// JWT Authentication Configuration
// ========================================
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection(GoogleAuthOptions.SectionName));
builder.Services.Configure<AppleAuthOptions>(builder.Configuration.GetSection(AppleAuthOptions.SectionName));

// ========================================
// AI Preview Configuration
// ========================================
builder.Services.Configure<AiPreviewOptions>(builder.Configuration.GetSection(AiPreviewOptions.SectionName));

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
    ?? throw new InvalidOperationException("Auth configuration is missing");

if (authOptions.JwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT key must be at least 32 characters");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authOptions.JwtIssuer,
        ValidateAudience = true,
        ValidAudience = authOptions.JwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT auth failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "admin");
    });
});

// ========================================
// S3 Storage Configuration
// ========================================
builder.Services.Configure<S3Options>(builder.Configuration.GetSection(S3Options.SectionName));

var s3Options = builder.Configuration.GetSection(S3Options.SectionName).Get<S3Options>()
    ?? throw new InvalidOperationException("S3 configuration is missing");

builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = s3Options.ServiceUrl,
        ForcePathStyle = s3Options.ForcePathStyle,
        UseHttp = false
    };

    // Используем прямую передачу учетных данных
    return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
});

builder.Services.AddSingleton<IS3StorageService, S3StorageService>();

// ========================================
// Ideogram AI Configuration
// ========================================
// IMPORTANT: ApiKey is NOT stored in appsettings.json!
// ApiKey is loaded from the IDEOGRAM_API_KEY environment variable.
//
// Setting the key:
// - Development: export IDEOGRAM_API_KEY=...
// - Docker: environment in docker-compose.yml
// - Heroku/Render: Config Vars / Environment Variables
// - GitHub Actions: secrets.IDEOGRAM_API_KEY
//
// ApiKey validation:
// - In Production: required, application will not start without key
// - In Development: optional, can work without key (for UI/DB testing)
var ideogramOptionsBuilder = builder.Services
    .AddOptions<IdeogramAiOptions>()
    .Bind(builder.Configuration.GetSection(IdeogramAiOptions.SectionName))
    .Configure(options =>
    {
        var envKey = Environment.GetEnvironmentVariable("IDEOGRAM_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            options.ApiKey = envKey;
        }
    });

// Validate API key only in Production
if (!builder.Environment.IsDevelopment())
{
    ideogramOptionsBuilder
        .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey),
            "IDEOGRAM_API_KEY must be provided in Production. Set it as an environment variable.")
        .ValidateOnStart();
}

// ========================================
// OpenAI Vision Configuration (for Upgrade Flow)
// ========================================
// IMPORTANT: ApiKey is NOT stored in appsettings.json!
// ApiKey is loaded from the OPENAI_API_KEY environment variable.
//
// Setting the key:
// - Development: export OPENAI_API_KEY=...
// - Docker: environment in docker-compose.yml
// - Heroku/Render: Config Vars / Environment Variables
// - GitHub Actions: secrets.OPENAI_API_KEY
//
// ApiKey validation:
// - In Production: required, application will not start without key
// - In Development: optional, can work without key (for UI/DB testing)
var openAiVisionOptionsBuilder = builder.Services
    .AddOptions<OpenAiVisionOptions>()
    .Bind(builder.Configuration.GetSection(OpenAiVisionOptions.SectionName))
    .Configure(options =>
    {
        var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            options.ApiKey = envKey;
        }
    });

// Validate API key only in Production
if (!builder.Environment.IsDevelopment())
{
    openAiVisionOptionsBuilder
        .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey),
            "OPENAI_API_KEY must be provided in Production. Set it as an environment variable.")
        .ValidateOnStart();
}

// Register OpenAI Vision HttpClient and services
builder.Services.AddHttpClient<IJewelryVisionAnalyzer, OpenAiVisionClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OpenAiVisionOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddSingleton<IJewelryAnalysisPromptBuilder, JewelryAnalysisPromptBuilder>();

// ========================================
// Caching
// ========================================
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICatalogCacheService, CatalogCacheService>();

// ========================================
// Application Services
// ========================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAiPreviewService, AiPreviewService>();
builder.Services.AddScoped<IUpgradeService, UpgradeService>();
// AuditService uses IServiceScopeFactory to create its own DbContext scopes,
// so it can safely be a Singleton (one instance reused for all requests)
builder.Services.AddSingleton<IAuditService, AuditService>();

// AI Services
// Named HttpClient for downloading images from Ideogram CDN (no auth headers)
builder.Services.AddHttpClient("IdeogramImageDownload", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    // Enable HTTP/2 for potentially faster downloads
    client.DefaultRequestVersion = System.Net.HttpVersion.Version20;
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
});

// Register HttpClient for IdeogramAiImageProvider with IAiImageProvider interface
// Note: Ideogram 3.0 uses full URL in requests, base address is just for connection pooling
builder.Services.AddHttpClient<IAiImageProvider, IdeogramAiImageProvider>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<IdeogramAiOptions>>().Value;
    // Base address for connection pooling
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    // Note: Api-Key header is added per-request in the provider
});
builder.Services.AddScoped<IAiPromptBuilder, AiPromptBuilder>();
builder.Services.AddScoped<IAiConfigBuilder, AiConfigBuilder>();
builder.Services.AddScoped<ISemanticContextBuilder, SemanticContextBuilder>();

// Background Services
builder.Services.AddHostedService<AiPreviewBackgroundService>();
builder.Services.AddHostedService<UpgradePreviewBackgroundService>();

// ========================================
// Build Application
// ========================================
var app = builder.Build();

// ========================================
// Log Ideogram AI Configuration (once at startup)
// ========================================
{
    var ideogramOptions = app.Services.GetRequiredService<IOptions<IdeogramAiOptions>>().Value;
    Console.WriteLine();
    Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
    Console.WriteLine("│ 🎨 Ideogram AI 3.0 Image Provider Configuration             │");
    Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
    Console.WriteLine($"│ Base URL:         {ideogramOptions.BaseUrl,-42}│");
    Console.WriteLine($"│ Generate Endpoint:{ideogramOptions.GenerateEndpoint,-42}│");
    Console.WriteLine($"│ HTTP Timeout:     {ideogramOptions.TimeoutSeconds} seconds{new string(' ', 32)}│");
    Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
    Console.WriteLine("│ Generation Parameters:                                      │");
    Console.WriteLine($"│ Aspect Ratio:     {ideogramOptions.AspectRatio,-42}│");
    Console.WriteLine($"│ Rendering Speed:  {ideogramOptions.RenderingSpeed,-42}│");
    Console.WriteLine($"│ Style Type:       {ideogramOptions.StyleType,-42}│");
    Console.WriteLine($"│ Magic Prompt:     {ideogramOptions.MagicPrompt,-42}│");
    Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
    var apiKeyStatus = string.IsNullOrWhiteSpace(ideogramOptions.ApiKey) ? "❌ NOT CONFIGURED" : "✅ Configured (hidden)";
    Console.WriteLine($"│ API Key:          {apiKeyStatus,-42}│");
    Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
    Console.WriteLine();
}

// ========================================
// Log OpenAI Vision Configuration (once at startup)
// ========================================
{
    var openAiOptions = app.Services.GetRequiredService<IOptions<OpenAiVisionOptions>>().Value;
    Console.WriteLine();
    Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
    Console.WriteLine("│ 👁️  OpenAI Vision Configuration (Upgrade Flow)              │");
    Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
    Console.WriteLine($"│ Base URL:         {openAiOptions.BaseUrl,-42}│");
    Console.WriteLine($"│ Model:            {openAiOptions.Model,-42}│");
    Console.WriteLine($"│ HTTP Timeout:     {openAiOptions.TimeoutSeconds} seconds{new string(' ', 32)}│");
    Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
    Console.WriteLine("│ Analysis Parameters:                                        │");
    Console.WriteLine($"│ Max Tokens:       {openAiOptions.MaxTokens,-42}│");
    Console.WriteLine($"│ Temperature:      {openAiOptions.Temperature,-42}│");
    Console.WriteLine($"│ Image Detail:     {openAiOptions.ImageDetail,-42}│");
    Console.WriteLine($"│ Max Retries:      {openAiOptions.MaxRetries,-42}│");
    Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
    var openAiKeyStatus = string.IsNullOrWhiteSpace(openAiOptions.ApiKey) ? "❌ NOT CONFIGURED" : "✅ Configured (hidden)";
    Console.WriteLine($"│ API Key:          {openAiKeyStatus,-42}│");
    Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
    Console.WriteLine();
}

// ========================================
// Middleware Pipeline (ORDER MATTERS!)
// ========================================

// 1. Global exception handler — первый, чтобы ловить всё
app.UseGlobalExceptionHandler();

// 2. Request logging
app.UseRequestLogging();

// 3. HTTPS Redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// 4. CORS
app.UseCors("DefaultCorsPolicy");

// 5. Swagger (в Development — свободно, в Production — с ограничениями)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Jewerly API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Jewerly API - Swagger UI";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();

        // Автоматическая авторизация для Development режима
        options.InjectJavascript("/swagger-auto-auth.js");
    });

    // Добавляем endpoint для custom JavaScript
    app.MapGet("/swagger-auto-auth.js", () =>
    {
        var js = @"
window.addEventListener('load', function() {
    // Функция для автоматической авторизации
    async function autoAuth() {
        try {
            console.log('[Swagger Auto-Auth] Fetching dev token...');

            // Получаем токен от dev endpoint
            const response = await fetch('/api/account/dev-token');

            if (!response.ok) {
                console.warn('[Swagger Auto-Auth] Dev token endpoint returned:', response.status);
                return;
            }

            const data = await response.json();
            const token = data.token || data.accessToken;

            if (!token) {
                console.warn('[Swagger Auto-Auth] No token in response');
                return;
            }

            console.log('[Swagger Auto-Auth] Token received, authorizing...');

            // Ждем, пока Swagger UI инициализируется
            let attempts = 0;
            const maxAttempts = 50;

            const authInterval = setInterval(() => {
                attempts++;

                if (window.ui && window.ui.authActions) {
                    clearInterval(authInterval);

                    // Авторизуемся
                    window.ui.authActions.authorize({
                        Bearer: {
                            name: 'Bearer',
                            schema: {
                                type: 'apiKey',
                                in: 'header',
                                name: 'Authorization',
                                description: ''
                            },
                            value: token
                        }
                    });

                    console.log('[Swagger Auto-Auth] ✅ Automatically authorized as user2@example.com');

                    // Показываем уведомление пользователю
                    setTimeout(() => {
                        const authButton = document.querySelector('.btn.authorize');
                        if (authButton) {
                            authButton.style.backgroundColor = '#4caf50';
                            authButton.style.borderColor = '#4caf50';
                            setTimeout(() => {
                                authButton.style.backgroundColor = '';
                                authButton.style.borderColor = '';
                            }, 2000);
                        }
                    }, 500);
                } else if (attempts >= maxAttempts) {
                    clearInterval(authInterval);
                    console.warn('[Swagger Auto-Auth] Failed to find Swagger UI instance');
                }
            }, 100);

        } catch (error) {
            console.error('[Swagger Auto-Auth] Error:', error);
        }
    }

    // Запускаем автоматическую авторизацию
    autoAuth();
});
";
        return Results.Content(js, "application/javascript");
    }).ExcludeFromDescription();
}
else
{
    // Production: Swagger доступен, но рекомендуется защитить
    // TODO: В production настроить защиту Swagger через:
    // - IP whitelist (только внутренние IP)
    // - Basic Authentication
    // - Или полностью отключить, оставив только для staging окружения
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Jewerly API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Jewerly API - Production";
    });
}

// 6. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Map controllers
app.MapControllers();

// ========================================
// Startup Logging
// ========================================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(
    "Application started. Environment: {Environment}, Version: {Version}",
    app.Environment.EnvironmentName,
    typeof(Program).Assembly.GetName().Version);

app.Run();
