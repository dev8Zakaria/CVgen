using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AiCv.CvService.Modules.Messaging;

public sealed class RabbitMqCvGenerationQueue : ICvGenerationQueue, IDisposable
{
    public const string RoutingKey = "cv.generate.requested";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly object _connectionLock = new();
    private IConnection? _connection;

    public RabbitMqCvGenerationQueue(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public bool IsEnabled => _options.Enabled;

    public Task EnqueueAsync(CvGenerationRequestedMessage message, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        using var channel = GetConnection().CreateModel();
        DeclareTopology(channel);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        var properties = channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2;
        properties.MessageId = message.JobId.ToString();
        properties.Type = RoutingKey;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(_options.ExchangeName, RoutingKey, mandatory: false, basicProperties: properties, body: body);
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
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = false
            };

            _connection = factory.CreateConnection();
            return _connection;
        }
    }

    private void DeclareTopology(IModel channel)
    {
        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        channel.QueueDeclare(_options.CvGenerationQueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.CvGenerationQueueName, _options.ExchangeName, RoutingKey);

        if (!string.IsNullOrWhiteSpace(_options.AuditQueueName))
        {
            channel.QueueDeclare(_options.AuditQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_options.AuditQueueName, _options.ExchangeName, "#");
        }
    }
}
