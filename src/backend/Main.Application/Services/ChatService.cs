using Main.Application.Dtos;
using Main.Application.InPorts;
using Main.Application.OutPorts;
using Main.BL.Exceptions;
using Main.BL.Models;

namespace Main.Application.Services;

public class ChatService: BaseService, IChatService
{
    public ChatService(
        IChatRepository chatRepo,
        IUserRepository userRepo,
        IChatUserRepository chatUserRepo,
        IMessageRepository messageRepo) : base(userRepo, chatRepo, chatUserRepo, messageRepo) { }
    public async Task<IEnumerable<ChatWithUsers>> GetChatsAsync(Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var chats = await _chatRepo.GetUserChatsAsync(currentUserId);
        var userIds = chats
            .SelectMany(c => c.Participants)
            .Select(p => p.UserId)
            .Distinct()
            .ToList();
        var users = await _userRepo.GetByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id);
        return chats.Select(chat => new ChatWithUsers
        {
            Id = chat.Id,
            Name = chat.Name,
            Type = chat.Type,
            OwnerUserId = chat.OwnerUserId,
            CreatedAt = chat.CreatedAt,
            Version = chat.Version,
            LastMessageNum = chat.LastMessageNum,
            Participants = chat.Participants
                .Select(p => userMap[p.UserId])
                .ToList()
        });
    }

    public async Task<ChatWithUsers> GetChatByIdAsync(Guid chatId, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var chat = await _chatRepo.GetByIdAsync(chatId);
        var userIds = chat.Participants.Select(p => p.UserId).ToList();
        var users = await _userRepo.GetByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id);
        return new ChatWithUsers
        {
            Id = chat.Id,
            Name = chat.Name,
            Type = chat.Type,
            OwnerUserId = chat.OwnerUserId,
            CreatedAt = chat.CreatedAt,
            Version = chat.Version,
            LastMessageNum = chat.LastMessageNum,
            Participants = users.ToList()
        };
    }
    public async Task<Guid> CreatePrivateChatAsync(Guid otherUserId, Guid currentUserId)
    {
        if (currentUserId == otherUserId)
            throw new RuleViolationException("Cannot create private chat with yourself");
        await EnsureUserExists(currentUserId);
        await EnsureUserExists(otherUserId);

        if (await _chatRepo.ExistsPrivateBetweenUsersAsync(currentUserId, otherUserId))
            throw new ConflictException("Private chat already exists between these users");

        var participant1 = new ChatUser(currentUserId, 0);
        var participant2 = new ChatUser(otherUserId, 0);

        var chat = Chat.CreatePrivate(participant1, participant2);
        await _chatRepo.CreateAsync(chat);
        return chat.Id;
    }
    public async Task<Guid> CreateGroupChatAsync(string name, List<Guid> memberIds, Guid currentUserId)
    {
        var allUserIds = memberIds.Append(currentUserId).Distinct();
        foreach (var userId in allUserIds)
            await EnsureUserExists(userId);

        if (string.IsNullOrWhiteSpace(name))
            throw new RuleViolationException("Group chat must have a name");

        var participants = allUserIds.Select(userId => new ChatUser(userId, 0)).ToList();
        var chat = Chat.CreateGroup(name, currentUserId, participants);
        await _chatRepo.CreateAsync(chat);
        return chat.Id;
    }
    public async Task UpdateChatNameAsync(Guid chatId, string newName, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);

        var chat = await GetChatOrThrow(chatId);
        EnsureOwner(chat, currentUserId);

        if (!await _chatRepo.TryUpdateNameAsync(chatId, newName))
            throw new TechnicalException("Failed to update chat name");
    }
    public async Task AddMemberAsync(Guid chatId, Guid userIdToAdd, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureUserExists(userIdToAdd);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        await EnsureNotParticipant(chatId, userIdToAdd);

        var participant = new ChatUser(userIdToAdd, 0);

        if (!await _chatUserRepo.TryAddAsync(chatId, participant))
            throw new TechnicalException("Failed to add new member");
    }
    public async Task RemoveMemberAsync(Guid chatId, Guid userIdToRemove, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureUserExists(userIdToRemove);
        var chat = await GetChatOrThrow(chatId);
        await EnsureParticipant(chatId, currentUserId);
        await EnsureParticipant(chatId, userIdToRemove);

        EnsureOwner(chat, currentUserId);

        if (userIdToRemove == chat.OwnerUserId)
            throw new RuleViolationException("Cannot remove chat owner");

        if (userIdToRemove == currentUserId)
            throw new RuleViolationException("Use LeaveChat to leave the chat");

        if (!await _chatUserRepo.TryRemoveAsync(chatId, userIdToRemove))
            throw new TechnicalException("Failed to remove member");
    }
    public async Task LeaveChatAsync(Guid chatId, Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var chat = await GetChatOrThrow(chatId);
        await EnsureParticipant(chatId, currentUserId);

        EnsureNotOwner(chat, currentUserId);

        if (!await _chatUserRepo.TryRemoveAsync(chatId, currentUserId))
            throw new TechnicalException("Failed to remove member");
    }
}
