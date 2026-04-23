using System.Text;
using Auth.BL.InputPorts;
using Auth.BL.OutputPorts;
using Auth.BL.Services;
using Auth.BL.Utils;
using Auth.DB.Postgres;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ILogger = Serilog.ILogger;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------- Основные службы ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger с поддержкой JWT
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });

    // options.AddSecurityRequirement(new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecuritySchemeReference("Bearer"),
    //         new List<string>()
    //     }
    // });
});

// ---------- Аутентификация и JWT ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// ---------- База данных (PostgreSQL) ----------
// Предполагаем, что у нас только Postgres, без switch по DbType
builder.Services.AddDbContext<ServerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")!));

// ---------- Регистрация репозиториев и сервисов ----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IKafkaProducer, Auth.Main.BR.KafkaProducer>(); // если KafkaProducer в папке Kafka в проекте Auth.Api

// ---------- Логирование (Serilog) ----------
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddScoped<ILogger>(_ => logger);

// ---------- Сборка приложения ----------
var app = builder.Build();

// Автоматическое создание БД (только для разработки)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
    await db.Database.EnsureCreatedAsync();
    var log = scope.ServiceProvider.GetRequiredService<ILogger>();
    log.Information("Starting up");
}

// Middleware для режима read-only (можно оставить)
var isReadOnly = builder.Configuration.GetValue<bool>("ReadOnly");
if (isReadOnly)
{
    var writeMethods = new[] { "POST", "PUT", "PATCH", "DELETE" };
    app.Use(async (context, next) =>
    {
        if (writeMethods.Contains(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("This server is read-only");
            return;
        }
        await next();
    });
}

// Swagger
app.UseSwagger(options =>
{
    options.RouteTemplate = "api/v1/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "API V1");
    options.RoutePrefix = "api/v1/swagger";
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("api/v1/health", () => Results.Ok("Healthy"));

await app.RunAsync();

public abstract partial class Program;