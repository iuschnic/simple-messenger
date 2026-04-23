using Main.Application.Dtos;
using Main.Application.Exceptions;
using Main.Application.InPorts;
using Main.Application.Mappers;
using Main.Application.OutPorts;
using Main.BL.Enums;
using Main.BL.Models;
using System.Xml.Linq;

namespace Main.Application.Services;

public class ChatService: BaseService, IChatService
{
    public ChatService(
        IChatRepository chatRepo,
        IUserRepository userRepo,
        IChatUserRepository chatUserRepo,
        IMessageRepository messageRepo,
        IMessageProducer messageProducer) : base(userRepo, chatRepo, chatUserRepo, messageRepo, messageProducer) { }
    public async Task<IEnumerable<ChatWithUsersDto>> GetChatsAsync(Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        var chats = await _chatRepo.GetUserChatsAsync(currentUserId);
        var userIds = chats
            .SelectMany(c => c.Participants)
            .Select(p => p.UserId)
            .Distinct()
            .ToList();
        var users = await _userRepo.GetByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id);
        return chats.Select(c => c.ToChatWithUsersDto(
            [.. c.Participants.Select(p => userMap[p.UserId])]));
    }

    public async Task<ChatWithUsersDto> GetChatByIdAsync(Guid chatId, Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        var chat = await GetChatOrThrow(chatId);
        var userIds = chat.Participants.Select(p => p.UserId).ToList();
        var users = await _userRepo.GetByIdsAsync(userIds);
        return chat.ToChatWithUsersDto([.. users]);
    }
    public async Task<Guid> CreatePrivateChatAsync(Guid otherUserId, Guid currentUserId)
    {
        if (currentUserId == otherUserId)
            throw new RuleViolationException("Cannot create private chat with yourself");
        await EnsureCurrentUserAuthorized(currentUserId);
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
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("chat name should not be null/whitespace");
        await EnsureCurrentUserAuthorized(currentUserId);
        var allUserIds = memberIds
            .Append(currentUserId)
            .Distinct()
            .ToList();
        var members = await _userRepo.GetByIdsAsync(allUserIds);
        var foundIds = members.Select(x => x.Id).ToHashSet();
        var notFound = allUserIds.Where(id => !foundIds.Contains(id)).ToList();

        if (notFound.Count != 0)
            throw new NotFoundException(nameof(User), notFound[0]);

        if (string.IsNullOrWhiteSpace(name))
            throw new RuleViolationException("Group chat must have a name");

        var userMap = members.ToDictionary(m => m.Id);
        var participants = allUserIds
            .Select(userId => new ChatUser(userId, 0))
            .ToList();
        var chat = Chat.CreateGroup(name, currentUserId, participants);
        await _chatRepo.CreateAsync(chat);

        var creatorName = userMap[currentUserId].DisplayedName;
        await _messageRepo.TryCreateAsync(
            Message.CreateSystem(
                chat.Id,
                $"{creatorName} создал чат"
            )
        );
        foreach (var userId in memberIds.Where(id => id != currentUserId).Distinct())
        {
            await _messageRepo.TryCreateAsync(
                Message.CreateSystem(
                    chat.Id,
                    $"{userMap[userId].DisplayedName} вступил в чат"
                )
            );
        }
        return chat.Id;
    }
    public async Task UpdateChatNameAsync(Guid chatId, string newName, Guid currentUserId)
    {
        if (string.IsNullOrEmpty(newName))
            throw new ArgumentException("chat name should not be null/whitespace");
        var user = await GetCurrentUserOrUnauthorized(currentUserId);
        var chat = await GetChatOrThrow(chatId);
        EnsureGroupChat(chat);
        EnsureOwner(chat, currentUserId);
        if (!await _chatRepo.TryUpdateNameAsync(chatId, newName))
            throw new TechnicalException("Failed to update chat name");
        await _messageRepo.TryCreateAsync(
                Message.CreateSystem(
                    chat.Id,
                    $"{user.DisplayedName} изменил(а) название группы на \"{newName}\""
                )
            );
    }
    public async Task AddMemberAsync(Guid chatId, Guid userIdToAdd, Guid currentUserId)
    {
        var currentUser = await GetCurrentUserOrUnauthorized(currentUserId);
        var toAddUser = await GetUserOrNotFound(userIdToAdd);
        var chat = await GetChatOrThrow(chatId);
        EnsureGroupChat(chat);
        await EnsureParticipant(chatId, currentUserId);
        await EnsureNotParticipant(chatId, userIdToAdd);

        var participant = new ChatUser(userIdToAdd, 0);
        if (!await _chatUserRepo.TryAddAsync(chatId, participant))
            throw new TechnicalException("Failed to add new member");

        await _messageRepo.TryCreateAsync(
                Message.CreateSystem(
                    chatId,
                    $"{currentUser.DisplayedName} добавил(а) {toAddUser.DisplayedName}"
                )
            );
    }
    public async Task RemoveMemberAsync(Guid chatId, Guid userIdToRemove, Guid currentUserId)
    {
        var currentUser = await GetCurrentUserOrUnauthorized(currentUserId);
        var toRemoveUser = await GetUserOrNotFound(userIdToRemove);
        var chat = await GetChatOrThrow(chatId);
        EnsureGroupChat(chat);
        await EnsureParticipant(chatId, currentUserId);
        await EnsureParticipant(chatId, userIdToRemove);
        EnsureOwner(chat, currentUserId);

        if (userIdToRemove == chat.OwnerUserId)
            throw new RuleViolationException("Cannot remove chat owner");
        if (userIdToRemove == currentUserId)
            throw new RuleViolationException("Use LeaveChat to leave the chat");

        if (!await _chatUserRepo.TryRemoveAsync(chatId, userIdToRemove))
            throw new TechnicalException("Failed to remove member");

        await _messageRepo.TryCreateAsync(
                Message.CreateSystem(
                    chatId,
                    $"{currentUser.DisplayedName} удалил(а) {toRemoveUser.DisplayedName}"
                )
            );
    }
    public async Task LeaveChatAsync(Guid chatId, Guid currentUserId)
    {
        var currentUser = await GetCurrentUserOrUnauthorized(currentUserId);
        var chat = await GetChatOrThrow(chatId);
        EnsureGroupChat(chat);
        await EnsureParticipant(chatId, currentUserId);
        EnsureNotOwner(chat, currentUserId);

        if (!await _chatUserRepo.TryRemoveAsync(chatId, currentUserId))
            throw new TechnicalException("Failed to remove member");

        await _messageRepo.TryCreateAsync(
                Message.CreateSystem(
                    chatId,
                    $"{currentUser.DisplayedName} покинул(а) группу"
                )
            );
    }
    private void EnsureGroupChat(Chat chat)
    {
        if (chat.Type != ChatType.Group)
            throw new RuleViolationException("Chat is not private");
    }
}
