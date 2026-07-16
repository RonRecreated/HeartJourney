using System.ComponentModel.DataAnnotations;

namespace HeartJourneyWeb.Features.Onboarding;

public class OnboardingFormModel
{
    [Required(ErrorMessage = "Display name is required.")]
    [MaxLength(80, ErrorMessage = "Display name must be 80 characters or fewer.")]
    public string? DisplayName { get; set; }

    [Required(ErrorMessage = "Age bracket is required.")]
    public string? AgeBracket { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "Religious background is required.")]
    public string? ReligiousBackground { get; set; }

    [Required(ErrorMessage = "Choose the season that best describes where you are.")]
    public string? CurrentRelationshipSeason { get; set; }
}