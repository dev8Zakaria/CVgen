using AiCv.Api.Modules.Cvs.DTOs;
using AiCv.Api.Modules.Cvs.Entities;
using AiCv.Api.Modules.Cvs.Repositories;
using AiCv.Api.Modules.Profiles.Repositories;
using AiCv.Api.Shared.Storage;

namespace AiCv.Api.Modules.Cvs.Services;

public class CvService
{
    private readonly CvRepository _cvRepository;
    private readonly ProfileRepository _profileRepository;
    private readonly MinioStorageService _storageService;

    public CvService(CvRepository cvRepository, ProfileRepository profileRepository, MinioStorageService storageService)
    {
        _cvRepository = cvRepository;
        _profileRepository = profileRepository;
        _storageService = storageService;
    }

    public async Task<GeneratedCvResponseDto?> GenerateInitialCvAsync(string keycloakId, GenerateCvRequestDto request)
    {
        // 1. Récupérer l'utilisateur et son profil
        var user = await _profileRepository.GetUserWithProfileByKeycloakIdAsync(keycloakId);
        if (user == null) return null;

        string? assetUrl = null;

        // 2. Si un fichier est fourni, l'uploader sur MinIO
        if (request.AssetFile != null && request.AssetFile.Length > 0)
        {
            using var stream = request.AssetFile.OpenReadStream();
            assetUrl = await _storageService.UploadFileAsync(stream, request.AssetFile.FileName, request.AssetFile.ContentType);
        }

        // 3. Créer l'entité en base de données
        var cv = new Cv
        {
            UserId = user.Id,
            OpportunityId = request.OpportunityId,
            AssetUrl = assetUrl
        };

        await _cvRepository.AddCvAsync(cv);
        await _cvRepository.SaveChangesAsync();

        // 4. Mapper et renvoyer la réponse "Without AI"
        return new GeneratedCvResponseDto
        {
            CvId = cv.Id,
            AssetUrl = cv.AssetUrl,
            GeneratedAt = cv.CreatedAt,
            ProfessionalSummary = "Génération initiale (En attente de l'IA)",
            
            Profile = new GeneratedCvProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Profile?.Phone ?? string.Empty,
                Location = user.Profile?.Location ?? string.Empty,
                Headline = user.Profile?.Title ?? string.Empty
            },
            
            Target = new GeneratedCvTargetDto
            {
                OpportunityId = request.OpportunityId,
                JobTitle = "Titre à extraire de l'offre", // Zakaria fera l'appel IA pour ça plus tard
                CompanyName = "Entreprise cible"
            }
        };
    }
}