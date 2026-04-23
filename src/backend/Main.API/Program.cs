using Confluent.Kafka;
using Main.API.Controllers;
using Main.Application.Handlers;
using Main.Application.InPorts;
using Main.Application.OutPorts;
using Main.Application.Services;
using Main.DB.Context;
using Main.DB.Repositories;
using Main.Auth.BR;
using Main.Realtime.BR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using ILogger = Serilog.ILogger;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog =====
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton<ILogger>(_ => logger);

// ===== SignalR =====
builder.Services.AddSignalR();

// ===== JWT Authentication =====
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

        // Для SignalR - токен из query параметра
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/v1/hub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ===== Controllers =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Messenger API",
        Version = "v1",
        Description = "REST API for messaging service"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.IncludeXmlComments(string.Empty, includeControllerXmlComments: false);

    /*c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });*/
});

// ===== Kafka Configs =====
builder.Services.Configure<KafkaProducerConfig>(
    builder.Configuration.GetSection("Kafka:Producer"));
builder.Services.Configure<KafkaConsumerConfig>(
    builder.Configuration.GetSection("Kafka:Consumer"));

// ===== Kafka Infrastructure (Singleton) =====
builder.Services.AddSingleton<IProducer<Null, byte[]>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<KafkaProducerConfig>>().Value;
    return new ProducerBuilder<Null, byte[]>(new ProducerConfig
    {
        BootstrapServers = config.BootstrapServers,
        Acks = Acks.All,
        MessageSendMaxRetries = config.MessageSendMaxRetries,
        RetryBackoffMs = config.RetryBackoffMs,
        EnableIdempotence = true,
        CompressionType = CompressionType.Snappy
    }).Build();
});

builder.Services.AddSingleton<IConsumer<Null, byte[]>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<KafkaConsumerConfig>>().Value;
    return new ConsumerBuilder<Null, byte[]>(new ConsumerConfig
    {
        BootstrapServers = config.BootstrapServers,
        GroupId = config.GroupId,
        AutoOffsetReset = config.AutoOffsetReset,
        EnableAutoCommit = config.EnableAutoCommit
    }).Build();
});

// ===== Application Services =====
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddDbContext<MainDbContext>(options =>
                            options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                  ?? builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddSingleton<IMessageProducer, KafkaMessageProducer>();

builder.Services.AddHostedService<KafkaMessageConsumer>();

var app = builder.Build();

// ===== Middleware Pipeline =====
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Messenger API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);