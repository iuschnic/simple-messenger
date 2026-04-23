using Shared.Main.Auth.Models;

namespace Auth.BL.OutputPorts;

public interface IKafkaProducer
{
    Task ProduceUserRegisteredAsync(UserCreateDto message, CancellationToken cancellationToken = default);
}