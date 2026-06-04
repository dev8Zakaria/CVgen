using System.Text;
using System.Text.Json;
using AiCv.CvService.Modules.Cvs.Clients;
using AiCv.CvService.Modules.Cvs.DTOs;
using AiCv.CvService.Modules.Cvs.Entities;
using AiCv.CvService.Modules.Cvs.Repositories;
using AiCv.CvService.Modules.Cvs.Storage;
using AiCv.CvService.Modules.Messaging;

namespace AiCv.CvService.Modules.Cvs.Services;

public class CvGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CvRepository _repository;
    private readonly IProfileClient _profileClient;
    private readonly IOpportunityClient _opportunityClient;
    private readonly IAiCvGenerationClient _aiClient;
    private readonly ICvObjectStorage _objectStorage;
    private readonly IEventPublisher _eventPublisher;

    public CvGenerationService(
        CvRepository repository,
        IProfileClient profileClient,
        IOpportunityClient opportunityClient,
        IAiCvGenerationClient aiClient,
        ICvObjectStorage objectStorage,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _profileClient = profileClient;
        _opportunityClient = opportunityClient;
        _aiClient = aiClient;
        _objectStorage = objectStorage;
        _eventPublisher = eventPublisher;
    }

    public async Task<GeneratedCvResponseDto?> GenerateAsync(
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

        return await GenerateFromSnapshotsAsync(
            keycloakId,
            request.AssetFile?.FileName,
            request.AssetFile?.ContentType,
            request.AssetFile?.Length,
            profile,
            opportunity,
            cancellationToken);
    }

    public async Task<GeneratedCvResponseDto> GenerateFromSnapshotsAsync(
        string keycloakId,
        string? assetFileName,
        string? assetContentType,
        long? assetSizeBytes,
        ProfileDto profile,
        OpportunityDto opportunity,
        CancellationToken cancellationToken = default)
    {
        var aiResponse = await _aiClient.GenerateCvAsync(BuildAiRequest(profile, opportunity), cancellationToken);
        var response = BuildGeneratedResponse(Guid.NewGuid(), assetFileName, profile, opportunity, aiResponse);

        var pdfFileName = SanitizeFileName($"{response.Content.Header.Name}-{response.Target.JobTitle}-{response.Target.CompanyName}-cv.pdf");
        var pdfObjectKey = BuildPdfObjectKey(keycloakId, response.Id, pdfFileName);
        var pdfContent = AtsPdfRenderer.Render(response);
        await _objectStorage.UploadPdfAsync(pdfObjectKey, pdfContent, cancellationToken);

        var generatedCv = new GeneratedCv
        {
            Id = response.Id,
            KeycloakId = keycloakId,
            OpportunityId = opportunity.Id,
            JobTitle = opportunity.Title,
            CompanyName = opportunity.CompanyName,
            Template = response.Content.Target.Role.Length % 2 == 0 ? "Atelier Ivory" : "Monograph Slate",
            Status = "Saved",
            ProfessionalSummary = response.ProfessionalSummary,
            ContentJson = JsonSerializer.Serialize(response, JsonOptions),
            AssetFileName = assetFileName,
            AssetContentType = assetContentType,
            AssetSizeBytes = assetSizeBytes,
            PdfObjectKey = pdfObjectKey,
            PdfBucketName = _objectStorage.BucketName,
            PdfSizeBytes = pdfContent.Length,
            CreatedAt = response.GeneratedAt,
            UpdatedAt = response.GeneratedAt
        };

        await _repository.AddAsync(generatedCv);
        await _repository.SaveChangesAsync();
        await _eventPublisher.PublishAsync("cv.generated", new
        {
            generatedCv.Id,
            generatedCv.KeycloakId,
            generatedCv.OpportunityId,
            generatedCv.JobTitle,
            generatedCv.CompanyName,
            generatedCv.Template,
            generatedCv.Status,
            generatedCv.PdfBucketName,
            generatedCv.PdfObjectKey,
            generatedCv.PdfSizeBytes,
            generatedCv.CreatedAt
        }, cancellationToken);

        return response;
    }

    public async Task<List<GeneratedCvListItemDto>> GetAllAsync(string keycloakId)
    {
        var cvs = await _repository.GetAllByUserAsync(keycloakId);

        return cvs.Select(cv => new GeneratedCvListItemDto
        {
            Id = cv.Id,
            OpportunityId = cv.OpportunityId,
            JobTitle = cv.JobTitle,
            CompanyName = cv.CompanyName,
            Template = cv.Template,
            Status = cv.Status,
            CreatedAt = cv.CreatedAt
        }).ToList();
    }

    public async Task<GeneratedCvResponseDto?> GetByIdAsync(Guid id, string keycloakId)
    {
        var cv = await _repository.GetByIdAndUserAsync(id, keycloakId);
        return cv is null ? null : JsonSerializer.Deserialize<GeneratedCvResponseDto>(cv.ContentJson, JsonOptions);
    }

    public async Task<bool> DeleteAsync(Guid id, string keycloakId)
    {
        var cv = await _repository.GetByIdAndUserAsync(id, keycloakId);
        if (cv is null)
        {
            return false;
        }

        _repository.Remove(cv);
        await _repository.SaveChangesAsync();
        await _eventPublisher.PublishAsync("cv.deleted", new
        {
            cv.Id,
            cv.KeycloakId,
            cv.OpportunityId,
            cv.JobTitle,
            cv.CompanyName,
            DeletedAt = DateTimeOffset.UtcNow
        });

        return true;
    }

    public async Task<(string FileName, string Html)?> BuildDownloadHtmlAsync(Guid id, string keycloakId)
    {
        var cv = await GetByIdAsync(id, keycloakId);
        if (cv is null)
        {
            return null;
        }

        var safeTitle = SanitizeFileName($"{cv.Content.Header.Name}-{cv.Target.JobTitle}-{cv.Target.CompanyName}-cv.html");
        var contactLine = BuildContactLine(cv);
        var educationHtml = BuildEducationHtml(cv.Content.Education);
        var projectHtml = BuildProjectsHtml(cv.Content.Projects);
        var experienceHtml = BuildExperienceHtml(cv.Content.Experience);
        var skillsHtml = BuildSkillsHtml(cv.Content);
        var certificationHtml = BuildSimpleItemsHtml("Certifications", cv.Content.Certifications);
        var languagesHtml = BuildSimpleItemsHtml("Languages", cv.Content.Languages);
        var html = $$"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{Escape(cv.Content.Header.Name)}} - {{Escape(cv.Target.JobTitle)}} CV</title>
  <style>
    :root {
      --ink: #111111;
      --muted: #3f3f46;
      --rule: #343434;
      --soft-rule: #d4d4d8;
    }

    * {
      box-sizing: border-box;
    }

    html {
      background: #f4f4f5;
    }

    body {
      width: 210mm;
      min-height: 297mm;
      margin: 0 auto;
      padding: 18mm 18mm 16mm;
      background: #ffffff;
      color: var(--ink);
      font-family: "Times New Roman", Times, serif;
      font-size: 11.2pt;
      line-height: 1.32;
    }

    header {
      display: grid;
      grid-template-columns: 1.4fr 1fr;
      gap: 18px;
      padding-bottom: 10px;
      border-bottom: 1.4px solid var(--rule);
    }

    h1 {
      margin: 0;
      font-size: 22pt;
      line-height: 1;
      letter-spacing: 0.01em;
    }

    .headline {
      margin: 5px 0 0;
      font-size: 11.5pt;
      font-weight: 700;
    }

    .target {
      margin: 3px 0 0;
      color: var(--muted);
      font-size: 10.5pt;
    }

    .contact {
      margin: 0;
      text-align: right;
      font-size: 10.5pt;
      line-height: 1.45;
      white-space: pre-line;
    }

    section {
      margin-top: 13px;
      break-inside: avoid;
    }

    h2 {
      margin: 0 0 5px;
      border-bottom: 1px solid var(--rule);
      font-size: 13.5pt;
      font-variant: small-caps;
      letter-spacing: 0.06em;
      line-height: 1.1;
    }

    h3 {
      margin: 0;
      font-size: 11.2pt;
      line-height: 1.25;
    }

    .entry {
      margin-top: 7px;
      break-inside: avoid;
    }

    .entry-head {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 14px;
      align-items: baseline;
    }

    .entry-meta {
      margin: 1px 0 0;
      color: var(--muted);
      font-style: italic;
    }

    .date {
      color: var(--muted);
      font-size: 10.5pt;
      font-style: italic;
      white-space: nowrap;
    }

    p {
      margin: 0;
    }

    ul {
      margin: 4px 0 0 16px;
      padding: 0;
    }

    li {
      margin: 1px 0;
      padding-left: 1px;
    }

    .summary {
      text-align: justify;
    }

    .skills p {
      margin: 2px 0;
    }

    .label {
      font-weight: 700;
    }

    @media print {
      html {
        background: #ffffff;
      }

      body {
        width: auto;
        min-height: auto;
        margin: 0;
        padding: 12mm 14mm;
      }

      a {
        color: inherit;
        text-decoration: none;
      }
    }
  </style>
</head>
<body>
  <header>
    <div>
      <h1>{{Escape(cv.Content.Header.Name)}}</h1>
      <p class="headline">{{Escape(cv.Content.Header.Title)}}</p>
      <p class="target">Target role: {{Escape(cv.Target.JobTitle)}} at {{Escape(cv.Target.CompanyName)}}</p>
    </div>
    <p class="contact">{{contactLine}}</p>
  </header>

  <section>
    <h2>Professional Summary</h2>
    <p class="summary">{{Escape(cv.Content.Summary)}}</p>
  </section>

  {{educationHtml}}
  {{projectHtml}}

  <section>
    <h2>Experience</h2>
  {{experienceHtml}}
  </section>

  {{skillsHtml}}
  {{certificationHtml}}
  {{languagesHtml}}
</body>
</html>
""";

        return (safeTitle, html);
    }

    public async Task<(string FileName, byte[] Content)?> BuildDownloadPdfAsync(Guid id, string keycloakId)
    {
        var generatedCv = await _repository.GetByIdAndUserAsync(id, keycloakId);
        if (generatedCv is null)
        {
            return null;
        }

        var cv = JsonSerializer.Deserialize<GeneratedCvResponseDto>(generatedCv.ContentJson, JsonOptions);
        if (cv is null)
        {
            return null;
        }

        var safeTitle = SanitizeFileName($"{cv.Content.Header.Name}-{cv.Target.JobTitle}-{cv.Target.CompanyName}-cv.pdf");
        if (!string.IsNullOrWhiteSpace(generatedCv.PdfObjectKey))
        {
            var storedPdf = await _objectStorage.GetPdfAsync(generatedCv.PdfObjectKey);
            if (storedPdf is not null)
            {
                return (safeTitle, storedPdf);
            }
        }

        var pdfContent = AtsPdfRenderer.Render(cv);
        var objectKey = generatedCv.PdfObjectKey ?? BuildPdfObjectKey(keycloakId, generatedCv.Id, safeTitle);
        await _objectStorage.UploadPdfAsync(objectKey, pdfContent);

        generatedCv.PdfObjectKey = objectKey;
        generatedCv.PdfBucketName = _objectStorage.BucketName;
        generatedCv.PdfSizeBytes = pdfContent.Length;
        generatedCv.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        return (safeTitle, pdfContent);
    }

    private static AiCvGenerationRequestDto BuildAiRequest(ProfileDto profile, OpportunityDto opportunity)
    {
        return new AiCvGenerationRequestDto
        {
            Profile = new AiCvProfileInputDto
            {
                FullName = profile.FullName,
                Email = profile.Email,
                Phone = profile.Phone,
                Location = profile.Location,
                Headline = profile.Title,
                Summary = profile.Summary,
                Skills = profile.Skills.Select(skill => skill.Name).Where(value => !string.IsNullOrWhiteSpace(value)).ToList(),
                Experiences = profile.Experiences.Cast<object>().ToList(),
                Educations = profile.Educations.Cast<object>().ToList(),
                Projects = profile.Projects.Cast<object>().ToList(),
                Languages = profile.Languages.Cast<object>().ToList(),
                Certifications = profile.Certifications.Cast<object>().ToList()
            },
            Opportunity = new AiCvOpportunityInputDto
            {
                Id = opportunity.Id.ToString(),
                Title = opportunity.Title,
                CompanyName = opportunity.CompanyName,
                Description = opportunity.Description,
                AnalysisSummary = opportunity.Analysis?.AnalysisSummary ?? "",
                ExtractedSkills = opportunity.Analysis?.ExtractedSkills ?? [],
                ExtractedKeywords = opportunity.Analysis?.ExtractedKeywords ?? [],
                ExtractedResponsibilities = opportunity.Analysis?.ExtractedResponsibilities ?? [],
                DetectedTechnologies = opportunity.Analysis?.DetectedTechnologies ?? [],
                DetectedExperienceLevel = opportunity.Analysis?.DetectedExperienceLevel ?? "",
                DetectedLocation = opportunity.Analysis?.DetectedLocation ?? "",
                DetectedContractType = opportunity.Analysis?.DetectedContractType ?? "",
                MustHaveRequirements = opportunity.Analysis?.MustHaveRequirements ?? [],
                NiceToHaveRequirements = opportunity.Analysis?.NiceToHaveRequirements ?? [],
                CvFocusPoints = opportunity.Analysis?.CvFocusPoints ?? [],
                CandidateRisks = opportunity.Analysis?.CandidateRisks ?? []
            }
        };
    }

    private static GeneratedCvResponseDto BuildGeneratedResponse(
        Guid cvId,
        string? assetFileName,
        ProfileDto profile,
        OpportunityDto opportunity,
        AiCvGenerationResponseDto aiResponse)
    {
        var generatedAt = DateTime.UtcNow;
        var skills = aiResponse.HighlightedSkills.Count > 0
            ? aiResponse.HighlightedSkills
            : profile.Skills.Select(skill => skill.Name).Where(value => !string.IsNullOrWhiteSpace(value)).Take(10).ToList();

        var content = new GeneratedCvContentDto
        {
            Header = new GeneratedCvHeaderDto
            {
                Name = profile.FullName,
                Title = profile.Title,
                Email = profile.Email,
                Phone = profile.Phone,
                Address = profile.Location
            },
            Summary = BuildProfessionalSummary(profile, opportunity, aiResponse),
            Experience = profile.Experiences.Select(experience => ToGeneratedExperience(experience, aiResponse)).ToList(),
            Education = profile.Educations.Select(ToGeneratedEducation).ToList(),
            Projects = profile.Projects.Select(project => ToGeneratedProject(project, aiResponse)).ToList(),
            Skills = skills,
            SkillGroups = BuildSkillGroups(profile.Skills, skills),
            Languages = profile.Languages.Select(language => new GeneratedCvSimpleItemDto
            {
                Name = language.Name,
                Detail = language.Level
            }).ToList(),
            Certifications = profile.Certifications.Select(certification => new GeneratedCvSimpleItemDto
            {
                Name = certification.Title,
                Detail = string.Join(" - ", new[] { certification.Issuer, certification.Year }.Where(value => !string.IsNullOrWhiteSpace(value)))
            }).ToList(),
            Notes = aiResponse.TailoredExperienceHints,
            Target = new GeneratedCvContentTargetDto
            {
                Role = opportunity.Title,
                Company = opportunity.CompanyName
            }
        };

        return new GeneratedCvResponseDto
        {
            Id = cvId,
            CvId = cvId,
            AssetFileName = assetFileName,
            Profile = new GeneratedCvProfileDto
            {
                FullName = profile.FullName,
                Email = profile.Email,
                Phone = profile.Phone,
                Location = profile.Location,
                Headline = profile.Title
            },
            Target = new GeneratedCvTargetDto
            {
                OpportunityId = opportunity.Id,
                JobTitle = opportunity.Title,
                CompanyName = opportunity.CompanyName
            },
            ProfessionalSummary = BuildProfessionalSummary(profile, opportunity, aiResponse),
            HighlightedSkills = skills,
            MatchingKeywords = aiResponse.MatchingKeywords,
            TailoredExperienceHints = aiResponse.TailoredExperienceHints,
            Content = content,
            GeneratedAt = generatedAt
        };
    }

    private static string BuildProfessionalSummary(ProfileDto profile, OpportunityDto opportunity, AiCvGenerationResponseDto aiResponse)
    {
        if (!string.IsNullOrWhiteSpace(aiResponse.ProfessionalSummary))
        {
            return aiResponse.ProfessionalSummary.Trim();
        }

        var role = string.IsNullOrWhiteSpace(opportunity.Title) ? "the target role" : opportunity.Title;
        var skills = profile.Skills
            .Select(skill => skill.Name)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Take(4);

        return $"{profile.Title} with practical experience relevant to {role}, including {string.Join(", ", skills)}. Strong focus on maintainable delivery, API quality, and cross-functional collaboration.";
    }

    private static GeneratedCvExperienceDto ToGeneratedExperience(ProfileExperienceDto experience, AiCvGenerationResponseDto aiResponse)
    {
        var optimized = aiResponse.OptimizedExperiences.FirstOrDefault(item =>
            Guid.TryParse(item.Id, out var parsedId) && parsedId == experience.Id);
        var bullets = CleanFinalBullets(optimized?.Bullets).Count > 0
            ? CleanFinalBullets(optimized?.Bullets)
            : SplitBullets(experience.Description);

        return new GeneratedCvExperienceDto
        {
            Id = experience.Id.ToString(),
            Company = experience.Company,
            Role = experience.Position,
            Period = FormatPeriod(experience.StartDate, experience.EndDate),
            Bullets = bullets
        };
    }

    private static GeneratedCvEducationDto ToGeneratedEducation(ProfileEducationDto education)
    {
        return new GeneratedCvEducationDto
        {
            School = education.School,
            Degree = education.Degree,
            Field = education.Field
        };
    }

    private static GeneratedCvProjectDto ToGeneratedProject(ProfileProjectDto project, AiCvGenerationResponseDto aiResponse)
    {
        var optimized = aiResponse.OptimizedProjects.FirstOrDefault(item =>
            string.Equals(item.Name, project.Name, StringComparison.OrdinalIgnoreCase));
        var bullets = CleanFinalBullets(optimized?.Bullets).Count > 0
            ? CleanFinalBullets(optimized?.Bullets)
            : SplitBullets(project.Description);

        return new GeneratedCvProjectDto
        {
            Name = project.Name,
            Description = project.Description,
            Technologies = project.Technologies,
            Bullets = bullets
        };
    }

    private static List<GeneratedCvSkillGroupDto> BuildSkillGroups(List<ProfileSkillDto> profileSkills, List<string> fallbackSkills)
    {
        var groupedSkills = profileSkills
            .Where(skill => !string.IsNullOrWhiteSpace(skill.Name))
            .GroupBy(skill => string.IsNullOrWhiteSpace(skill.Category) ? "Core Skills" : skill.Category.Trim())
            .Select(group => new GeneratedCvSkillGroupDto
            {
                Label = group.Key,
                Items = group.Select(skill => skill.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            })
            .Where(group => group.Items.Count > 0)
            .ToList();

        if (groupedSkills.Count > 0)
        {
            var existingSkills = groupedSkills
                .SelectMany(group => group.Items)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var targetedSkills = fallbackSkills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Where(skill => !existingSkills.Contains(skill))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (targetedSkills.Count > 0)
            {
                groupedSkills.Add(new GeneratedCvSkillGroupDto
                {
                    Label = "Targeted Skills",
                    Items = targetedSkills
                });
            }

            return groupedSkills;
        }

        return
        [
            new GeneratedCvSkillGroupDto
            {
                Label = "Core Skills",
                Items = fallbackSkills.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            }
        ];
    }

    private static List<string> SplitBullets(string description)
    {
        return description
            .Split(["\r\n", "\n", "."], StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Take(3)
            .ToList();
    }

    private static List<string> CleanFinalBullets(List<string>? bullets)
    {
        if (bullets is null)
        {
            return [];
        }

        var adviceWords = new[] { "emphasize", "highlight", "mention", "focus on", "tailor", "showcase" };

        return bullets
            .Select(value => value.Trim().TrimStart('-', '•', '*').Trim())
            .Where(value => value.Length > 0)
            .Where(value => !adviceWords.Any(word => value.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .Take(4)
            .ToList();
    }

    private static string FormatPeriod(DateTime startDate, DateTime? endDate)
    {
        var start = startDate.ToString("yyyy-MM");
        var end = endDate?.ToString("yyyy-MM") ?? "Present";
        return $"{start} - {end}";
    }

    private static string BuildExperienceHtml(List<GeneratedCvExperienceDto> experiences)
    {
        if (experiences.Count == 0)
        {
            return "<p>No professional experience added yet.</p>";
        }

        var builder = new StringBuilder();
        foreach (var experience in experiences)
        {
            builder.Append("<div class=\"entry\">");
            builder.Append("<div class=\"entry-head\">");
            builder.Append($"<h3>{Escape(experience.Role)} - {Escape(experience.Company)}</h3>");
            builder.Append($"<span class=\"date\">{Escape(experience.Period)}</span>");
            builder.Append("</div>");
            if (!string.IsNullOrWhiteSpace(experience.Location))
            {
                builder.Append($"<p class=\"entry-meta\">{Escape(experience.Location)}</p>");
            }

            builder.Append("<ul>");
            foreach (var bullet in experience.Bullets)
            {
                builder.Append($"<li>{Escape(bullet)}</li>");
            }
            builder.Append("</ul>");
            builder.Append("</div>");
        }

        return builder.ToString();
    }

    private static string BuildEducationHtml(List<GeneratedCvEducationDto> education)
    {
        if (education.Count == 0)
        {
            return "";
        }

        var builder = new StringBuilder("<section><h2>Education</h2>");
        foreach (var item in education)
        {
            var title = string.Join(" in ", new[] { item.Degree, item.Field }.Where(value => !string.IsNullOrWhiteSpace(value)));
            builder.Append("<div class=\"entry\">");
            builder.Append($"<h3>{Escape(title)}</h3>");
            builder.Append($"<p class=\"entry-meta\">{Escape(item.School)}</p>");
            builder.Append("</div>");
        }
        builder.Append("</section>");

        return builder.ToString();
    }

    private static string BuildProjectsHtml(List<GeneratedCvProjectDto> projects)
    {
        if (projects.Count == 0)
        {
            return "";
        }

        var builder = new StringBuilder("<section><h2>Projects</h2>");
        foreach (var project in projects)
        {
            builder.Append("<div class=\"entry\">");
            builder.Append($"<h3>{Escape(project.Name)}</h3>");
            if (!string.IsNullOrWhiteSpace(project.Description))
            {
                builder.Append($"<p class=\"entry-meta\">{Escape(project.Description)}</p>");
            }
            if (project.Bullets.Count > 0)
            {
                builder.Append("<ul>");
                foreach (var bullet in project.Bullets)
                {
                    builder.Append($"<li>{Escape(bullet)}</li>");
                }
                builder.Append("</ul>");
            }
            if (!string.IsNullOrWhiteSpace(project.Technologies))
            {
                builder.Append($"<p><span class=\"label\">Technologies:</span> {Escape(project.Technologies)}</p>");
            }
            builder.Append("</div>");
        }
        builder.Append("</section>");

        return builder.ToString();
    }

    private static string BuildSkillsHtml(GeneratedCvContentDto content)
    {
        var groups = content.SkillGroups.Count > 0
            ? content.SkillGroups
            : [new GeneratedCvSkillGroupDto { Label = "Core Skills", Items = content.Skills }];

        if (groups.Count == 0 || groups.All(group => group.Items.Count == 0))
        {
            return "";
        }

        var builder = new StringBuilder("<section class=\"skills\"><h2>Technical Skills</h2>");
        foreach (var group in groups.Where(group => group.Items.Count > 0))
        {
            builder.Append("<p>");
            builder.Append($"<span class=\"label\">{Escape(group.Label)}:</span> ");
            builder.Append(Escape(string.Join(", ", group.Items)));
            builder.Append("</p>");
        }
        builder.Append("</section>");

        return builder.ToString();
    }

    private static string BuildSimpleItemsHtml(string title, List<GeneratedCvSimpleItemDto> items)
    {
        if (items.Count == 0)
        {
            return "";
        }

        var values = items
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .Select(item => string.IsNullOrWhiteSpace(item.Detail)
                ? Escape(item.Name)
                : $"{Escape(item.Name)} ({Escape(item.Detail)})")
            .ToList();

        return values.Count == 0
            ? ""
            : $"<section><h2>{Escape(title)}</h2><p>{string.Join(", ", values)}</p></section>";
    }

    private static string BuildContactLine(GeneratedCvResponseDto cv)
    {
        return string.Join("\n", new[]
        {
            cv.Content.Header.Phone,
            cv.Content.Header.Email,
            cv.Content.Header.Address
        }.Where(value => !string.IsNullOrWhiteSpace(value)).Select(Escape));
    }

    private static string Escape(string? value)
    {
        return System.Net.WebUtility.HtmlEncode(value ?? "");
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '-');
        }

        return value.ToLowerInvariant();
    }

    private static string BuildPdfObjectKey(string keycloakId, Guid cvId, string fileName)
    {
        var safeUser = SanitizeFileName(keycloakId);
        return $"{safeUser}/{cvId}/{fileName}";
    }
}
