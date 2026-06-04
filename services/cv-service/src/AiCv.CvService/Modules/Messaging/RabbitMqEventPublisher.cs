using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AiCv.CvService.Modules.Messaging;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly object _connectionLock = new();
    private IConnection? _connection;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishAsync<TPayload>(string routingKey, TPayload payload, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        try
        {
            using var channel = GetConnection().CreateModel();
            channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
            DeclareAuditQueue(channel);

            var envelope = new IntegrationEventEnvelope<TPayload>(
                Guid.NewGuid(),
                routingKey,
                "cv-service",
                DateTimeOffset.UtcNow,
                payload);

            var body = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
            var properties = channel.CreateBasicProperties();
            properties.ContentType = "application/json";
            properties.DeliveryMode = 2;
            properties.MessageId = envelope.Id.ToString();
            properties.Type = routingKey;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            channel.BasicPublish(_options.ExchangeName, routingKey, mandatory: false, basicProperties: properties, body: body);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "RabbitMQ publish failed for routing key {RoutingKey}.", routingKey);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private IConnection GetConnection()
    {
        if (_connection?.IsOpen == true)
        {
            return _connection;
        }

        lock (_connectionLock)
        {
            if (_connection?.IsOpen == true)
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _connection = factory.CreateConnection();
            return _connection;
        }
    }

    private void DeclareAuditQueue(IModel channel)
    {
        if (string.IsNullOrWhiteSpace(_options.AuditQueueName))
        {
            return;
        }

        channel.QueueDeclare(_options.AuditQueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.AuditQueueName, _options.ExchangeName, "#");
    }

    private sealed record IntegrationEventEnvelope<TPayload>(
        Guid Id,
        string Type,
        string Source,
        DateTimeOffset OccurredAt,
        TPayload Data);
}
