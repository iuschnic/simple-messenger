using Main.BL.Enums;
using Main.BL.Models;

namespace Main.Application.Dtos;

public class ChatWithUsers
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public ChatType Type { get; init; }
    public Guid? OwnerUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public ulong Version { get; init; }
    public ulong LastMessageNum { get; init; }
    public IReadOnlyList<User> Participants { get; init; }
}
