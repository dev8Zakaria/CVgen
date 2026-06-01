using AiCv.CvService.Modules.Cvs.DTOs;

namespace AiCv.CvService.Modules.Cvs.Clients;

public interface IAiCvGenerationClient
{
    Task<AiCvGenerationResponseDto> GenerateCvAsync(AiCvGenerationRequestDto request, CancellationToken cancellationToken = default);
}
