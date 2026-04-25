using System.Text;
using BL.Contracts;
using BL.Interfaces;
using BL.Services;
using Dapper;
using DB.Database;
using DB.Repositories;
using Http;
using Messenger.Tests.Sandbox.Scenario;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RT;

namespace Messenger.Tests.Sandbox;

internal abstract class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Console.WriteLine("PROGRAM STARTED");

        // ================= CONFIG =================

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // ================= DI =================

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(config);

        // ================= DB =================

        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        var dbPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "messenger_debug.db"
        );

        Console.WriteLine($"DB PATH: {dbPath}");

        var factory = new DbConnectionFactory(dbPath);

        var init = new DbInitializer(factory);
        init.Reset().GetAwaiter().GetResult();
        init.Init().GetAwaiter().GetResult();

        var repoHub = new RepositoryHub(
            new AuthRepository(factory),
            new UserRepository(factory),
            new ChatRepository(factory),
            new MessageRepository(factory),
            new CurrentUserRepository(factory)
        );

        services.AddSingleton(repoHub);

        // ================= HTTP + RT =================

        // если хочешь оставить моки
        services.AddSingleton<IHttpClient, HttpClientImpl>();
        services.AddSingleton<IRealtimeClient, RealtimeClient>();

        // если реальные — раскомментируй:
        // services.AddHttpClient<IHttpClient, HttpClientImpl>();
        // services.AddSingleton<IRealtimeClient, RealtimeClient>();

        // ================= BL =================

        services.AddScoped<IMessengerService, MessengerService>();

        var provider = services.BuildServiceProvider();

        var bl = provider.GetRequiredService<IMessengerService>();

        // ================= EVENTS =================

        bl.Events.MessageReceived += m =>
        {
            Console.WriteLine($"[EVENT] New message: [{m.MessageNumber}] {m.Text}");
            return Task.CompletedTask;
        };

        bl.Events.UserLeftChat += (chatId, userId) =>
        {
            Console.WriteLine($"[EVENT] User {userId} left chat {chatId}");
            return Task.CompletedTask;
        };

        bl.Events.ChatCreated += chat =>
        {
            Console.WriteLine($"[EVENT] Chat created: {chat.Id}");
            return Task.CompletedTask;
        };
        
        bl.Events.ReconnectedToHub += () =>
        {
            Console.WriteLine($"[EVENT] Connection up;)");
            return Task.CompletedTask;
        };

        // ================= SCENARIO =================

        var http = provider.GetRequiredService<IHttpClient>();
        var scenario = new UiScenario(bl, http, repoHub);

        await scenario.Run();

        Console.WriteLine("Press ENTER to exit...");
        Console.ReadLine();
        Console.WriteLine("DONE");
    }
}