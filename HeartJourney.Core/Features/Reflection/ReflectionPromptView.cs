namespace HeartJourney.Core.Features.Reflection;

public class ReflectionPromptView
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string AnswerType { get; set; } = string.Empty;

    public List<AnswerOptionView> AnswerOptions { get; set; } = new();

    public bool AllowNotes { get; set; }

    public string NotesPrompt { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}