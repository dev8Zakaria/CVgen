using System.Text.Json;
using AiCv.CvService.Modules.Cvs.Clients;
using AiCv.CvService.Modules.Cvs.DTOs;
using AiCv.CvService.Modules.Cvs.Entities;
using AiCv.CvService.Modules.Cvs.Repositories;
using AiCv.CvService.Modules.Messaging;

namespace AiCv.CvService.Modules.Cvs.Services;

public sealed class CvGenerationJobService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CvRepository _repository;
    private readonly IProfileClient _profileClient;
    private readonly IOpportunityClient _opportunityClient;
    private readonly ICvGenerationQueue _queue;
    private readonly IEventPublisher _eventPublisher;

    public CvGenerationJobService(
        CvRepository repository,
        IProfileClient profileClient,
        IOpportunityClient opportunityClient,
        ICvGenerationQueue queue,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _profileClient = profileClient;
        _opportunityClient = opportunityClient;
        _queue = queue;
        _eventPublisher = eventPublisher;
    }

    public bool QueueEnabled => _queue.IsEnabled;

    public async Task<CvGenerationJobResponseDto?> CreateQueuedJobAsync(
        string keycloakId,
        string authorizationHeader,
        GenerateCvRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileClient.GetCurrentProfileAsync(authorizationHeader, cancellationToken);
        var opportunity = await _opportunityClient.GetOpportunityAsync(request.OpportunityId, authorizationHeader, cancellationToken);
        if (profile is null || opportunity is null)
        {
            return null;
        }

        var job = new CvGenerationJob
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            OpportunityId = request.OpportunityId,
            AssetFileName = request.AssetFile?.FileName,
            ProfileSnapshotJson = JsonSerializer.Serialize(profile, JsonOptions),
            OpportunitySnapshotJson = JsonSerializer.Serialize(opportunity, JsonOptions),
            Status = "queued",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddJobAsync(job);
        await _repository.SaveChangesAsync();

        await _queue.EnqueueAsync(new CvGenerationRequestedMessage
        {
            JobId = job.Id,
            KeycloakId = keycloakId,
            OpportunityId = request.OpportunityId,
            AssetFileName = request.AssetFile?.FileName
        }, cancellationToken);

        await _eventPublisher.PublishAsync("cv.generation_queued", new
        {
            job.Id,
            job.KeycloakId,
            job.OpportunityId,
            job.Status,
            job.CreatedAt
        }, cancellationToken);

        return ToDto(job);
    }

    public async Task<CvGenerationJobResponseDto?> GetByIdAsync(Guid jobId, string keycloakId)
    {
        var job = await _repository.GetJobByIdAndUserAsync(jobId, keycloakId);
        return job is null ? null : ToDto(job);
    }

    public Task<CvGenerationJob?> GetJobAsync(Guid jobId)
    {
        return _repository.GetJobByIdAsync(jobId);
    }

    public async Task MarkProcessingAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId);
        if (job is null)
        {
            return;
        }

        job.Status = "processing";
        job.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();
        await _eventPublisher.PublishAsync("cv.generation_processing", new
        {
            job.Id,
            job.KeycloakId,
            job.OpportunityId,
            job.Status,
            job.UpdatedAt
        }, cancellationToken);
    }

    public async Task MarkCompletedAsync(Guid jobId, Guid generatedCvId, CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId);
        if (job is null)
        {
            return;
        }

        job.Status = "completed";
        job.GeneratedCvId = generatedCvId;
        job.ErrorMessage = null;
        job.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();
        await _eventPublisher.PublishAsync("cv.generation_completed", new
        {
            job.Id,
            job.KeycloakId,
            job.OpportunityId,
            job.GeneratedCvId,
            job.Status,
            job.UpdatedAt
        }, cancellationToken);
    }

    public async Task MarkFailedAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId);
        if (job is null)
        {
            return;
        }

        job.Status = "failed";
        job.ErrorMessage = errorMessage.Length > 1000 ? errorMessage[..1000] : errorMessage;
        job.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();
        await _eventPublisher.PublishAsync("cv.generation_failed", new
        {
            job.Id,
            job.KeycloakId,
            job.OpportunityId,
            job.Status,
            job.ErrorMessage,
            job.UpdatedAt
        }, cancellationToken);
    }

    private static CvGenerationJobResponseDto ToDto(CvGenerationJob job) => new()
    {
        JobId = job.Id,
        Status = job.Status,
        OpportunityId = job.OpportunityId,
        GeneratedCvId = job.GeneratedCvId,
        ErrorMessage = job.ErrorMessage,
        CreatedAt = job.CreatedAt,
        UpdatedAt = job.UpdatedAt
    };
}
