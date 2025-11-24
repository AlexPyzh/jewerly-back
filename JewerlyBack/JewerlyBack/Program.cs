using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.S3;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Infrastructure.Auth;
using JewerlyBack.Infrastructure.Middleware;
using JewerlyBack.Infrastructure.Storage;
using JewerlyBack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ========================================
// CORS Configuration
// ========================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // TODO: Настроить разрешённые origins для production
            policy.WithOrigins("https://your-app-domain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
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
        Description = "API для мобильного приложения Jewerly",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@jewerly.app"
        }
    });

    // JWT Bearer authentication в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен"
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

builder.Services.AddAuthorization();

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
    return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
});

builder.Services.AddSingleton<IS3StorageService, S3StorageService>();

// ========================================
// Application Services
// ========================================
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// ========================================
// Build Application
// ========================================
var app = builder.Build();

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
app.UseCors();

// 5. Swagger (только в Development или по флагу)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Jewerly API v1");
        options.RoutePrefix = "swagger";
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
