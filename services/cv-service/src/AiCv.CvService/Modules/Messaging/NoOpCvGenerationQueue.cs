namespace AiCv.CvService.Modules.Messaging;

public sealed class NoOpCvGenerationQueue : ICvGenerationQueue
{
    public bool IsEnabled => false;

    public Task EnqueueAsync(CvGenerationRequestedMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
