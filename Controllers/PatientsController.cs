using cwiczenia8.DTOs;
using cwiczenia8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cwiczenia8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var query = _context.Patients
            .Include(p => p.Admissions)
                .ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments)
                .ThenInclude(ba => ba.Bed)
                    .ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments)
                .ThenInclude(ba => ba.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => 
                EF.Functions.Like(p.FirstName, $"%{search}%") || 
                EF.Functions.Like(p.LastName, $"%{search}%"));
        }

        var patients = await query.Select(p => new GetPatientDto
        {
            Pesel = p.Pesel,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Age = p.Age,
            Sex = p.Sex ? "Male" : "Female",
            Admissions = p.Admissions.Select(a => new GetAdmissionDto
            {
                Id = a.Id,
                AdmissionDate = a.AdmissionDate,
                DischargeDate = a.DischargeDate,
                Ward = new GetWardDto
                {
                    Id = a.Ward.Id,
                    Name = a.Ward.Name,
                    Description = a.Ward.Description
                }
            }).ToList(),
            BedAssignments = p.BedAssignments.Select(ba => new GetBedAssignmentDto
            {
                Id = ba.Id,
                From = ba.From,
                To = ba.To,
                Bed = new GetBedDto
                {
                    Id = ba.Bed.Id,
                    BedType = new GetBedTypeDto
                    {
                        Id = ba.Bed.BedType.Id,
                        Name = ba.Bed.BedType.Name,
                        Description = ba.Bed.BedType.Description
                    },
                    Room = new GetRoomDto
                    {
                        Id = ba.Bed.RoomId,
                        HasTv = ba.Bed.Room.HasTv,
                        Ward = new GetWardDto
                        {
                            Id = ba.Bed.Room.Ward.Id,
                            Name = ba.Bed.Room.Ward.Name,
                            Description = ba.Bed.Room.Ward.Description
                        }
                    }
                }
            }).ToList()
        }).ToListAsync();

        return Ok(patients);
    }
    
    [HttpPost("{id}/bedassignments")]
    public async Task<IActionResult> AssignBed(string id, [FromBody] PostBedAssignmentDto dto)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == id);
        if (!patientExists)
        {
            return NotFound($"Pacjent o numerze PESEL {id} nie istnieje w bazie.");
        }
        var freeBed = await _context.Beds
            .Where(b => b.BedType.Name == dto.BedType && b.Room.Ward.Name == dto.Ward)
            .Where(b => !b.BedAssignments.Any(ba => 
                (dto.To == null || ba.From < dto.To) && 
                (ba.To == null || ba.To > dto.From)
            )) 
            .FirstOrDefaultAsync();
        
        if (freeBed == null)
        {
            return NotFound($"Nie znaleziono wolnego łóżka typu '{dto.BedType}' na oddziale '{dto.Ward}' w wybranym terminie.");
        }
        
        var newAssignment = new BedAssignment
        {
            PatientPesel = id,
            BedId = freeBed.Id,
            From = dto.From,
            To = dto.To
        };

        _context.BedAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();

        return StatusCode(201, "Łóżko zostało pomyślnie przypisane pacjentowi.");
    }
}