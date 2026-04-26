using BL.Contracts;
using BL.Interfaces;

namespace UI;

internal sealed class UiSession
{
    public UiSession(IMessengerService messenger, IRealtimeClient realtimeClient, IHttpClient httpClient)
    {
        Messenger = messenger;
        HttpClient = httpClient;
    }

    public IMessengerService Messenger { get; }
    public IHttpClient HttpClient { get; }
}
