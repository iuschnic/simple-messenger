using Confluent.Kafka;
using Main.API.Controllers;
using Main.Application.Handlers;
using Main.Application.InPorts;
using Main.Application.OutPorts;
using Main.Application.Services;
using Main.Auth.BR;
using Main.DB.Context;
using Main.DB.Repositories;
using Main.Realtime.BR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

// ================== Controllers ==================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ================== Swagger (Swashbuckle) ==================
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
    /*options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });*/
    /*options.EnableAnnotations();
    options.UseOneOfForPolymorphism();
    //options.UseAllOfForInheritance();
    options.SelectDiscriminatorNameUsing(type =>
    {
        if (type == typeof(BaseCreateChatRequest)) return "chatType";
        if (type == typeof(BaseCreateMessageRequest)) return "messageType";
        return null;
    });
    options.SelectDiscriminatorValueUsing(subType =>
    {
        return subType.Name switch
        {
            nameof(CreateGroupChatRequest) => "0",
            nameof(CreatePrivateChatRequest) => "1",
            nameof(SendMessageRequest) => "0",
            nameof(ReplyMessageRequest) => "1",
            nameof(ForwardMessageRequest) => "2",
            _ => null
        };
    });*/
});

// ================== Auth ==================
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

// ================== Serilog ==================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);

// ================== DB ==================
builder.Services.AddDbContext<MainDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresConnection")
    ));

// ================== Repositories ==================
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatUserRepository, ChatUserRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ================== Services ==================
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();

// ================== internal auth ==================
builder.Services.AddScoped<ApiKeyAuthFilter>();

// ================== Kafka ==================
builder.Services.Configure<KafkaProducerConfig>(
    builder.Configuration.GetSection("Kafka:Producer"));

builder.Services.Configure<KafkaConsumerConfig>(
    builder.Configuration.GetSection("Kafka:Consumer"));

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

builder.Services.AddSingleton<IMessageProducer, KafkaMessageProducer>();
builder.Services.AddHostedService<KafkaMessageConsumer>();

// ================== Build ==================
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// ================== Pipeline ==================
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

// ================== Swagger ==================
app.UseSwagger(options =>
{
    options.RouteTemplate = "api/v1/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "API V1");
    options.RoutePrefix = "api/v1/swagger";
});

app.MapControllers();

app.Run();