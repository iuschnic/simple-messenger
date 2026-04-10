using System.Text;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Realtime.API.Utils;
using Realtime.BL;
using Realtime.BL.InputPorts;
using Realtime.BL.OutputPorts;
using Realtime.DB;
using Realtime.Main.BR;
using Serilog;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

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

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSingleton<ILogger>(_ => logger);

builder.Services.Configure<KafkaConsumerConfig>(
    builder.Configuration.GetSection("KafkaConsumer"));

builder.Services.AddSingleton<IConsumer<Null, byte[]>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<KafkaConsumerConfig>>().Value;
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = config.BootstrapServers,
        GroupId = config.GroupId,
        AutoOffsetReset = config.AutoOffsetReset,
        EnableAutoCommit = config.EnableAutoCommit
    };
    return new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
});

builder.Services.AddSingleton<IMessageHandler, MessageHandler>();
builder.Services.AddSingleton<IConnectionsRepository, ConnectionRepository>();
builder.Services.AddSingleton<IGroupManager, GroupManager>();
builder.Services.AddSingleton<HttpChatReceiver>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

builder.Services.AddHostedService<KafkaMessageConsumer>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<RealtimeHub>("/api/v1/hub");
app.MapGet("api/v1/health", () => Results.Ok("Healthy"));

await app.RunAsync().ConfigureAwait(false);