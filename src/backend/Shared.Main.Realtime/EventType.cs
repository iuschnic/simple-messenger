namespace Shared.Main.Realtime;

public enum EventType
{
    MessageReceived,
    MessageUpdated,
    MessageRead,
    UserChanged,
    ChatCreated,
    ChatUpdated,
    ChatDeleted,
    ChatUserJoined,
    ChatUserLeft
}