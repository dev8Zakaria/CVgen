namespace AiCv.CvService.Modules.Cvs.Clients;

public interface IProfileClient
{
    Task<ProfileDto?> GetCurrentProfileAsync(string authorizationHeader, CancellationToken cancellationToken = default);
}
