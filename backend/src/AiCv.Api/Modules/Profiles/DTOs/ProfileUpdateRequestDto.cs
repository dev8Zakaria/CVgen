namespace AiCv.Api.Modules.Profiles.DTOs;

public class ProfileUpdateRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}