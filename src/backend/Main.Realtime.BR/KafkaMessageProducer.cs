using Confluent.Kafka;
using Main.Application.OutPorts;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using Serilog;
using Shared.Main.Realtime;
using Shared.Main.Realtime.Models;

namespace Main.Realtime.BR;

public class KafkaMessageProducer : IMessageProducer, IDisposable
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;
    private readonly ILogger _logger;

    public KafkaMessageProducer(
        IProducer<Null, byte[]> producer,
        IOptions<KafkaProducerConfig> config,
        ILogger logger)
    {
        _producer = producer;
        _topic = config.Value.Topic;
        _logger = logger;
    }

    public async Task SendMessageReceivedAsync(
        ulong messageNumber,
        Guid chatId,
        Guid? senderId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool isDeleted,
        ulong version,
        BL.Enums.MessageType type,
        ulong? replyToMessageNumber,
        Guid? forwardedFromUserId)
    {
        var dto = new MessageDto(
            messageNumber,
            chatId,
            senderId,
            text,
            createdAt,
            editedAt,
            isDeleted,
            version,
            MapMessageType(type),
            replyToMessageNumber,
            forwardedFromUserId);
        await ProduceAsync(EventType.MessageReceived, dto);
    }

    public async Task SendMessageUpdatedAsync(
        ulong messageNumber,
        Guid chatId,
        Guid? senderId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool isDeleted,
        ulong version,
        BL.Enums.MessageType type,
        ulong? replyToMessageNumber,
        Guid? forwardedFromUserId)
    {
        var dto = new MessageDto(
            messageNumber,
            chatId,
            senderId,
            text,
            createdAt,
            editedAt,
            isDeleted,
            version,
            MapMessageType(type),
            replyToMessageNumber,
            forwardedFromUserId);
        await ProduceAsync(EventType.MessageUpdated, dto);
    }

    public async Task SendMessageReadAsync(
        Guid chatId,
        Guid userId,
        string uniqueName,
        string displayedName,
        ulong lastMessageRead)
    {
        var userDto = new UserDto(
            userId,
            uniqueName,
            displayedName);
        var chatUserDto = new ChatUserDto(
            chatId,
            userDto,
            lastMessageRead);
        await ProduceAsync(EventType.MessageRead, chatUserDto);
    }

    public async Task SendUserChangedAsync(
        Guid userId,
        string uniqueName,
        string displayedName)
    {
        var dto = new UserDto(
            userId,
            uniqueName,
            displayedName);
        await ProduceAsync(EventType.UserChanged, dto);
    }

    public async Task SendChatCreatedAsync(
        Guid chatId,
        string? chatName,
        BL.Enums.ChatType chatType,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        IEnumerable<(Guid userId, string uniqueName, string displayedName)> participants)
    {
        var chatDto = new ChatDto(
            chatId,
            chatName,
            MapChatType(chatType),
            ownerId,
            createdAt,
            version,
            lastMessageNum);
        var participantsDto = participants.Select(p => new ChatUserDto(
            chatId,
            new UserDto(p.userId, p.uniqueName, p.displayedName), 0)).ToList();
        var fullChatDto = new FullChatDto(
            chatDto,
            participantsDto);
        await ProduceAsync(EventType.ChatCreated, fullChatDto);
    }

    public async Task SendChatUpdatedAsync(
        Guid chatId,
        string? chatName,
        BL.Enums.ChatType chatType,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum)
    {
        var dto = new ChatDto(
            chatId,
            chatName,
            MapChatType(chatType),
            ownerId,
            createdAt,
            version,
            lastMessageNum);
        await ProduceAsync(EventType.ChatUpdated, dto);
    }

    public async Task SendChatDeletedAsync(
        Guid chatId,
        string? chatName,
        BL.Enums.ChatType chatType,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        IEnumerable<(Guid userId, string uniqueName, string displayedName)> participants)
    {
        var chatDto = new ChatDto(
            chatId,
            chatName,
            MapChatType(chatType),
            ownerId,
            createdAt,
            version,
            lastMessageNum);
        var participantsDto = participants.Select(p => new ChatUserDto(
            chatId,
            new UserDto(p.userId, p.uniqueName, p.displayedName), 0)).ToList();
        var fullChatDto = new FullChatDto(
            chatDto,
            participantsDto);
        await ProduceAsync(EventType.ChatDeleted, fullChatDto);
    }

    public async Task SendChatUserJoinedAsync(
        Guid chatId,
        Guid userId,
        string uniqueName,
        string displayedName)
    {
        var userDto = new UserDto(
            userId,
            uniqueName,
            displayedName);
        var chatUserDto = new ChatUserDto(
            chatId,
            userDto,
            0);
        await ProduceAsync(EventType.ChatUserJoined, chatUserDto);
    }

    public async Task SendChatUserLeftAsync(
        Guid chatId,
        Guid userId,
        string uniqueName,
        string displayedName)
    {
        var userDto = new UserDto(
            userId,
            uniqueName,
            displayedName);
        var chatUserDto = new ChatUserDto(
            chatId,
            userDto,
            0);
        await ProduceAsync(EventType.ChatUserLeft, chatUserDto);
    }

    private async Task ProduceAsync(EventType eventType, object payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        var message = new Message<Null, byte[]>
        {
            Value = bytes,
            Headers = new Headers
            {
                { "EventType", BitConverter.GetBytes((int)eventType) }
            }
        };

        try
        {
            var deliveryResult = await _producer.ProduceAsync(_topic, message);
            _logger.Debug("Message delivered to {Topic}[{Partition}] at offset {Offset}",
                deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (ProduceException<Null, byte[]> ex)
        {
            _logger.Error(ex, "Failed to deliver message to {Topic}", _topic);
            throw;
        }
    }

    private static Shared.Main.Realtime.Models.MessageType MapMessageType(
        BL.Enums.MessageType type)
    {
        return type switch
        {
            BL.Enums.MessageType.Regular => Shared.Main.Realtime.Models.MessageType.Regular,
            BL.Enums.MessageType.Reply => Shared.Main.Realtime.Models.MessageType.Reply,
            BL.Enums.MessageType.Forward => Shared.Main.Realtime.Models.MessageType.Forward,
            BL.Enums.MessageType.System => Shared.Main.Realtime.Models.MessageType.System,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static Shared.Main.Realtime.Models.ChatType MapChatType(
        BL.Enums.ChatType type)
    {
        return type switch
        {
            BL.Enums.ChatType.Group => Shared.Main.Realtime.Models.ChatType.Group,
            BL.Enums.ChatType.Private => Shared.Main.Realtime.Models.ChatType.Private,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}

public class KafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int MessageSendMaxRetries { get; set; } = 3;
    public int RetryBackoffMs { get; set; } = 100;
}