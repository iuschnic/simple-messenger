using System;
using System.IO;
using Dapper;
using BL.Interfaces;
using BL.Services;
using DB.Database;
using DB.Repositories;
using Messenger.Tests.Sandbox.Mocks;
using Messenger.Tests.Sandbox.Scenario;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("PROGRAM STARTED");

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

        var http = new FakeHttpClient();
        var rt = new FakeRealtimeClient();

        var bl = new MessengerService(http, rt, repoHub);
        bl.Events.MessageReceived += m =>
        {
            Console.WriteLine($"[EVENT] New message: [{m.MessageNumber}] {m.Text}");
        };

        bl.Events.UserLeftChat += (chatId, userId) =>
        {
            Console.WriteLine($"[EVENT] User {userId} left chat {chatId}");
        };

        bl.Events.ChatCreated += chatId =>
        {
            Console.WriteLine($"[EVENT] Chat created: {chatId}");
        };

        var scenario = new UiScenario(bl, http, repoHub);
        scenario.Run();

        Console.WriteLine("DONE");
    }
}