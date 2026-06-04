namespace AiCv.OpportunityService.Modules.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TPayload>(string routingKey, TPayload payload, CancellationToken cancellationToken = default);
}
