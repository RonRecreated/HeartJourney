public class Dimension
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }

    public string Summary { get; set; }

    public List<string> MilestoneIds { get; set; }

    public int SortOrder { get; set; }
}