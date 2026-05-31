namespace cwiczenia8.DTOs;

public class GetAdmissionDto
{
    public int Id { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public GetWardDto Ward { get; set; } = null!;
}