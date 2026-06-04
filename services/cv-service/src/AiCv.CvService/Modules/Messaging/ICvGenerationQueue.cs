namespace AiCv.CvService.Modules.Messaging;

public interface ICvGenerationQueue
{
    bool IsEnabled { get; }
    Task EnqueueAsync(CvGenerationRequestedMessage message, CancellationToken cancellationToken = default);
}
