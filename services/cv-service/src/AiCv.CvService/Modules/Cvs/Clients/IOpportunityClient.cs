namespace AiCv.CvService.Modules.Cvs.Clients;

public interface IOpportunityClient
{
    Task<OpportunityDto?> GetOpportunityAsync(Guid opportunityId, string authorizationHeader, CancellationToken cancellationToken = default);
}
