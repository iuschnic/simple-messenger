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

        var scenario = new UiScenario(bl, http, repoHub);
        scenario.Run();

        Console.WriteLine("DONE");
    }
}