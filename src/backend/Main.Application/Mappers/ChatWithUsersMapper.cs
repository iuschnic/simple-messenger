using Main.Application.Dtos;
using Main.BL.Models;

namespace Main.Application.Mappers;

public static class ChatWithUsersMapper
{
    public static ChatWithUsersDto ToDto(this Chat domain, List<User> users)
    {
        var userMap = users.ToDictionary(u => u.Id);
        var missingUserIds = domain.Participants
            .Select(p => p.UserId)
            .Where(id => !userMap.ContainsKey(id))
            .ToList();
        if (missingUserIds.Any())
        {
            throw new ArgumentException(
                $"Users with ids [{string.Join(", ", missingUserIds)}] not found in users list",
                nameof(users));
        }
        return new ChatWithUsersDto
        {
            Id = domain.Id,
            Name = domain.Name,
            Type = domain.Type,
            OwnerUserId = domain.OwnerUserId,
            CreatedAt = domain.CreatedAt,
            Version = domain.Version,
            LastMessageNum = domain.LastMessageNum,
            Participants = domain.Participants
                .Select(p => userMap[p.UserId])
                .ToList()
                .AsReadOnly()
        };
    }
}
