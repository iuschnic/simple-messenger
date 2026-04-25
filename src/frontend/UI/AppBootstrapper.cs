using BL.Contracts;
using BL.Interfaces;
using BL.Services;
using Dapper;
using DB.Database;
using DB.Repositories;

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

        var dbPath = Path.Combine(AppContext.BaseDirectory, "messenger_ui.db");
        var factory = new DbConnectionFactory(dbPath);

        var initializer = new DbInitializer(factory);
        initializer.Reset().GetAwaiter().GetResult();
        initializer.Init().GetAwaiter().GetResult();

        var repositoryHub = new RepositoryHub(
            new AuthRepository(factory),
            new UserRepository(factory),
            new ChatRepository(factory),
            new MessageRepository(factory),
            new CurrentUserRepository(factory)
        );

        var httpClient = new FakeHttpClient();
        var realtimeClient = new FakeRealtimeClient();
        IMessengerService messengerService = new MessengerService(httpClient, realtimeClient, repositoryHub);
        var session = new UiSession(messengerService, realtimeClient, httpClient);

        return new Form1(session);
    }
}
