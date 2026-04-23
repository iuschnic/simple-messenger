using Auth.BL.OutputPorts;
using Shared.Main.Auth.Dtos;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Main.Auth;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace Auth.Main.BR;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;
    private readonly ILogger _logger;
    
    public KafkaProducer(IConfiguration configuration, ILogger logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };
        _producer = new ProducerBuilder<Null, byte[]>(config).Build();
        _topic = configuration["Kafka:Topics:UserRegistered"] ?? "user-registered";
        _logger = logger;
    }

    public async Task ProduceUserRegisteredAsync(UserCreateDto message)
    {        
        await ProduceAsync(EventType.CreateUser, message);
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
    public void Dispose() => _producer?.Dispose();
}

public class KafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int MessageSendMaxRetries { get; set; } = 3;
    public int RetryBackoffMs { get; set; } = 100;
}