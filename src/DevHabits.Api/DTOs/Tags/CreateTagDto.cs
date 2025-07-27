namespace DevHabits.Api.DTOs.Tags;

public sealed record CreateTagDto
{
    public required string Name { get; set; }

    public string? Description { get; set; }
}
