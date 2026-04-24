using Main.API.JsonConverters;
using Main.Application.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Main.API.Models;

public class AddContactRequest
{
    [Required]
    public Guid UserContactId { get; set; }
    [Required]
    public string ContactName { get; set; } = string.Empty;
}

[JsonConverter(typeof(CreateChatRequestConverter))]
public abstract class BaseCreateChatRequest
{
    [Required]
    public ChatTypeApp ChatType { get; set; }
}

public class CreatePrivateChatRequest : BaseCreateChatRequest
{
    [Required]
    public Guid WithUserId { get; set; }
}

public class CreateGroupChatRequest : BaseCreateChatRequest
{
    [Required]
    public List<Guid> MemberIds { get; set; } = new();
    [Required]
    public string ChatName { get; set; } = string.Empty;
}

[JsonConverter(typeof(CreateMessageRequestConverter))]
public abstract class BaseCreateMessageRequest
{
    [Required]
    public MessageTypeApp MessageType { get; set; }
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Client version must be non-negative")]
    public long ClientVersion { get; set; }
}

public class SendMessageRequest : BaseCreateMessageRequest
{
    [Required]
    [MinLength(1)]
    public string Text { get; set; } = string.Empty;
}

public class ReplyMessageRequest : BaseCreateMessageRequest
{
    [Required]
    [MinLength(1)]
    public string Text { get; set; } = string.Empty;
    [Required]
    public long ReplyToMessageNum { get; set; }
}

public class ForwardMessageRequest : BaseCreateMessageRequest
{
    [Required]
    public Guid ForwardedFromChatId { get; set; }
    [Required]
    public long ForwardedFromMessageNum { get; set; }
}

public class EditMessageRequest
{
    [Required]
    public string NewText { get; set; } = string.Empty;
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Client version must be non-negative")]
    public long ClientVersion { get; set; }
}

public class ReadMessagesRequest
{
    [Required]
    public long LastMessageNum { get; set; }
}

public class SyncChatsRequest
{
    [Required]
    public List<ChatSyncItem> Chats { get; set; } = new();
}

public class ChatSyncItem
{
    [Required]
    public Guid ChatId { get; set; }
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Client version must be non-negative")]
    public long ClientVersion { get; set; }
}

public class UpdateDisplayedNameRequest
{
    [Required]
    [MinLength(1)]
    public string NewDisplayedName { get; set; } = string.Empty;
}
public class UpdateChatNameRequest
{
    [Required]
    [MinLength(1)]
    public string NewChatName { get; set; } = string.Empty;
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Client version must be non-negative")]
    public long ClientVersion { get; set; }
}

public class UpdateContactNameRequest
{
    [Required]
    [MinLength(1)]
    public string NewContactName { get; set; } = string.Empty;
}

public class AddMemberRequest
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Client version must be non-negative")]
    public long ClientVersion { get; set; }
}
