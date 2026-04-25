using BL.Interfaces;

namespace UI;

internal sealed class UiSession
{
    public UiSession(IMessengerService messenger, FakeRealtimeClient realtimeClient, FakeHttpClient httpClient)
    {
        Messenger = messenger;
        RealtimeClient = realtimeClient;
        HttpClient = httpClient;
    }

    public IMessengerService Messenger { get; }
    public FakeRealtimeClient RealtimeClient { get; }
    public FakeHttpClient HttpClient { get; }
}
