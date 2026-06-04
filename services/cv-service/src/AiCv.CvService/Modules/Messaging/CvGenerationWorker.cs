using System.Text;
using System.Text.Json;
using AiCv.CvService.Modules.Cvs.Clients;
using AiCv.CvService.Modules.Cvs.DTOs;
using AiCv.CvService.Modules.Cvs.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AiCv.CvService.Modules.Messaging;

public sealed class CvGenerationWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CvGenerationWorker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public CvGenerationWorker(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<CvGenerationWorker> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("RabbitMQ CV generation worker is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                StartConsuming(stoppingToken);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "RabbitMQ CV generation worker could not connect. Retrying in 5 seconds.");
                DisposeRabbitMq();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void StartConsuming(CancellationToken stoppingToken)
    {
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
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(_options.CvGenerationQueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.CvGenerationQueueName, _options.ExchangeName, RabbitMqCvGenerationQueue.RoutingKey);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, eventArgs) =>
        {
            var deliveryTag = eventArgs.DeliveryTag;

            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var message = JsonSerializer.Deserialize<CvGenerationRequestedMessage>(json, JsonOptions);
                if (message is null)
                {
                    _channel.BasicAck(deliveryTag, multiple: false);
                    return;
                }

                ProcessMessageAsync(message, stoppingToken).GetAwaiter().GetResult();
                _channel.BasicAck(deliveryTag, multiple: false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "RabbitMQ CV generation message processing failed.");
                _channel.BasicNack(deliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(_options.CvGenerationQueueName, autoAck: false, consumer);
        _logger.LogInformation("RabbitMQ CV generation worker is listening on queue {QueueName}.", _options.CvGenerationQueueName);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        DisposeRabbitMq();
        base.Dispose();
    }

    private void DisposeRabbitMq()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }

    private async Task ProcessMessageAsync(CvGenerationRequestedMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<CvGenerationJobService>();
        var cvService = scope.ServiceProvider.GetRequiredService<CvGenerationService>();

        await jobService.MarkProcessingAsync(message.JobId, cancellationToken);

        try
        {
            var job = await jobService.GetJobAsync(message.JobId);
            if (job is null)
            {
                _logger.LogWarning("CV generation job {JobId} was not found.", message.JobId);
                return;
            }

            var profile = JsonSerializer.Deserialize<ProfileDto>(job.ProfileSnapshotJson, JsonOptions);
            var opportunity = JsonSerializer.Deserialize<OpportunityDto>(job.OpportunitySnapshotJson, JsonOptions);
            if (profile is null || opportunity is null)
            {
                await jobService.MarkFailedAsync(message.JobId, "Profile or opportunity snapshot could not be loaded.", cancellationToken);
                return;
            }

            var generated = await cvService.GenerateFromSnapshotsAsync(
                message.KeycloakId,
                message.AssetFileName,
                null,
                null,
                profile,
                opportunity,
                cancellationToken);

            await jobService.MarkCompletedAsync(message.JobId, generated.Id, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "CV generation job {JobId} failed.", message.JobId);
            await jobService.MarkFailedAsync(message.JobId, exception.Message, cancellationToken);
        }
    }
}
