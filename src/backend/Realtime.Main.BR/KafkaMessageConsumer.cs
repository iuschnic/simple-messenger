using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.Extensions.Options;
using Realtime.BL.OutputPorts;
using Serilog;
using Shared.Main.Realtime;

namespace Realtime.Main.BR;

public class KafkaMessageConsumer : BackgroundService
{
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IMessageHandler _messageHandler;
    private readonly string _topic;
    private readonly ILogger _logger;
    
    public KafkaMessageConsumer(
        IMessageHandler messageHandler,
        IOptions<KafkaConsumerConfig> config,
        ILogger logger)
    {
        _messageHandler = messageHandler;
        _logger = logger;
        
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
        _logger.Information("Subscribed to {Topic}", _topic);

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
            catch (Exception ex)
            {
                _logger.Error(ex, "Broken message from broker. Skipping...");
                await Task.Delay(1000, stoppingToken);
            }
        }
        
        _consumer.Close();
        _logger.Information("Consumer stopped");
    }
    
    private async Task ProcessMessageAsync(ConsumeResult<Null, byte[]> consumeResult)
    {
        var eventType = ExtractEventTypeFromHeaders(consumeResult.Message.Headers);
        if (!eventType.HasValue)
        {
            _logger.Error("Received message from broker doesn't have type of event. Skipping...");
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

public class KafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public bool EnableAutoCommit { get; set; } = false;
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
}
