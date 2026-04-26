using BL.Contracts;
using BL.Interfaces;
using BL.Services;
using Dapper;
using DB.Database;
using DB.Repositories;
using Http;
using Microsoft.Extensions.Configuration;
using RT;

namespace UI;

internal static class AppBootstrapper
{
    private static bool _guidHandlerRegistered;

    public static Form1 BuildMainForm()
    {
        if (!_guidHandlerRegistered)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            _guidHandlerRegistered = true;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var configuredDbPath = config["Database:Path"]
                               ?? throw new InvalidOperationException("Database:Path not configured");
        var dbPath = Path.IsPathRooted(configuredDbPath)
            ? configuredDbPath
            : Path.Combine(AppContext.BaseDirectory, configuredDbPath);
        var factory = new DbConnectionFactory(dbPath);

        var initializer = new DbInitializer(factory);
        initializer.Init().GetAwaiter().GetResult();

        var repositoryHub = new RepositoryHub(
            new AuthRepository(factory),
            new UserRepository(factory),
            new ChatRepository(factory),
            new MessageRepository(factory),
            new CurrentUserRepository(factory)
        );

        IHttpClient httpClient = new HttpClientImpl(config);
        IRealtimeClient realtimeClient = new RealtimeClient(config);
        IMessengerService messengerService = new MessengerService(httpClient, realtimeClient, repositoryHub);
        var session = new UiSession(messengerService, realtimeClient, httpClient);

        return new Form1(session);
    }
}
