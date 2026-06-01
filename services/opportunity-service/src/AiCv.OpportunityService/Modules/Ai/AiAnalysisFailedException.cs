namespace AiCv.OpportunityService.Modules.Ai;

public sealed class AiAnalysisFailedException : Exception
{
    public AiAnalysisFailedException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
