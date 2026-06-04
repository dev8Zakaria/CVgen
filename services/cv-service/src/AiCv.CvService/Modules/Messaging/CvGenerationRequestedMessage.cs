namespace AiCv.CvService.Modules.Messaging;

public sealed class CvGenerationRequestedMessage
{
    public Guid JobId { get; set; }
    public Guid OpportunityId { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string? AssetFileName { get; set; }
}
