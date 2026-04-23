using System.Text.Json;
using Auth.BL.OutputPorts;
using Shared.Main.Auth.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Auth.Main.BR;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    
    public KafkaProducer(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
        _topic = configuration["Kafka:Topics:UserRegistered"] ?? "user-registered";
    }

    public async Task ProduceUserRegisteredAsync(UserCreateDto message, CancellationToken cancellationToken = default)
    {
        var key = message.UniqueName; // ключ для партицирования
        var value = JsonSerializer.Serialize(message);
        
        await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = key,
            Value = value
        }, cancellationToken);
    }

    public void Dispose() => _producer?.Dispose();
}