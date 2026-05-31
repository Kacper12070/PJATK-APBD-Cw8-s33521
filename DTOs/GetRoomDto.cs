namespace cwiczenia8.DTOs;

public class GetRoomDto
{
    public string Id { get; set; } = null!;
    public bool HasTv { get; set; }
    public GetWardDto Ward { get; set; } = null!;
}