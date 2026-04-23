using Shared.Main.Auth.Dtos;

namespace Auth.BL.OutputPorts;

public interface IKafkaProducer
{
    Task ProduceUserRegisteredAsync(UserCreateDto message);
}