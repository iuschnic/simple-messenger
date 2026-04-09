using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.Extensions.Options;
using Realtime.BL.OutputPorts;
using Shared.Main.Realtime;

namespace Realtime.Main.BR;

public class KafkaMessageConsumer : BackgroundService
{
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IMessageHandler _messageHandler;
    private readonly string _topic;
    
    public KafkaMessageConsumer(
        IMessageHandler messageHandler,
        IOptions<KafkaConsumerConfig> config)
    {
        _messageHandler = messageHandler;
        
        var kafkaConfig = config.Value;
        _topic = kafkaConfig.Topic;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaConfig.BootstrapServers,
            GroupId = kafkaConfig.GroupId,
            AutoOffsetReset = kafkaConfig.AutoOffsetReset,
            EnableAutoCommit = kafkaConfig.EnableAutoCommit
        };
        
        _consumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                await ProcessMessageAsync(consumeResult);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        
        _consumer.Close();
    }
    
    private async Task ProcessMessageAsync(ConsumeResult<Null, byte[]> consumeResult)
    {
        var eventType = ExtractEventTypeFromHeaders(consumeResult.Message.Headers);
        if (!eventType.HasValue)
        {
            _consumer.Commit(consumeResult);
            return;
        }

        var dataJson = Encoding.UTF8.GetString(consumeResult.Message.Value);

        try
        {
            await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType.Value, dataJson);
        }
        finally
        {
            _consumer.Commit(consumeResult);
        }
    }

    private static EventType? ExtractEventTypeFromHeaders(Headers headers)
    {
        var header = headers.FirstOrDefault(h => h.Key == "EventType");

        if (header?.GetValueBytes() is { Length: 4 } bytes)
        {
            return (EventType)BitConverter.ToInt32(bytes);
        }

        return null;
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}

public record KafkaConsumerConfig(
    string BootstrapServers,
    string GroupId,
    string Topic,
    bool EnableAutoCommit = false,
    AutoOffsetReset AutoOffsetReset = AutoOffsetReset.Earliest);
