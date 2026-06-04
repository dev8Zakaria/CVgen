namespace AiCv.CvService.Modules.Messaging;

public sealed class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<TPayload>(string routingKey, TPayload payload, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
