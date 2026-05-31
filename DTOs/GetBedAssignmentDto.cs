namespace cwiczenia8.DTOs;

public class GetBedAssignmentDto
{
    public int Id { get; set; }
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
    public GetBedDto Bed { get; set; } = null!;
}
