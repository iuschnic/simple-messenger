using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Main.Application.InPorts;
using Serilog;
using Shared.Main.Auth;
using System.Text;

namespace Main.Auth.BR;

public class KafkaMessageConsumer : BackgroundService
{
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _topic;
    private readonly ILogger _logger;

    public KafkaMessageConsumer(
        IConsumer<Null, byte[]> consumer,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaConsumerConfig> config,
        ILogger logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _topic = config.Value.Topic;
        _logger = logger;
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
                    await ProcessMessageAsync(consumeResult, handler);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing message. Skipping...");
                await Task.Delay(1000, stoppingToken);
            }
        }
        _consumer.Close();
        _logger.Information("Consumer stopped");
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<Null, byte[]> consumeResult,
        IMessageHandler handler)
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
            await handler.OnMessageReceivedFromBrokerAsync(eventType.Value, dataJson);
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
            return (EventType)BitConverter.ToInt32(bytes);
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