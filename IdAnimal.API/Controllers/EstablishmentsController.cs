using IdAnimal.API.Data;
using IdAnimal.Shared.DTOs;
using IdAnimal.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdAnimal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstablishmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private const int DefaultUserId = 1; // Since we removed auth, use a default user

    public EstablishmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<EstablishmentDto>>> GetAll()
    {
        Console.WriteLine("Got a GET request");
        var userId = DefaultUserId;
        var establishments = await _context.Establishments
            .Where(e => e.UserId == userId)
            .Select(e => new EstablishmentDto
            {
                Id = e.Id,
                Name = e.Name,
                RegisterDate = e.RegisterDate,
                EstablishmentRegisterDate = e.EstablishmentRegisterDate,
                Province = e.Province,
                PostalCode = e.PostalCode,
                Renspa = e.Renspa,
                CattleCount = e.Cattle.Count
            })
            .ToListAsync();

        return Ok(establishments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EstablishmentDto>> GetById(int id)
    {
        var userId = DefaultUserId;
        var establishment = await _context.Establishments
            .Where(e => e.Id == id && e.UserId == userId)
            .Select(e => new EstablishmentDto
            {
                Id = e.Id,
                Name = e.Name,
                RegisterDate = e.RegisterDate,
                EstablishmentRegisterDate = e.EstablishmentRegisterDate,
                Province = e.Province,
                PostalCode = e.PostalCode,
                Renspa = e.Renspa,
                CattleCount = e.Cattle.Count
            })
            .FirstOrDefaultAsync();

        if (establishment == null)
        {
            return NotFound();
        }

        return Ok(establishment);
    }

    [HttpPost]
    public async Task<ActionResult<EstablishmentDto>> Create([FromBody] EstablishmentDto dto)
    {
        var userId = DefaultUserId;
        var establishment = new Establishment
        {
            Name = dto.Name,
            RegisterDate = dto.RegisterDate,
            EstablishmentRegisterDate = dto.EstablishmentRegisterDate,
            Province = dto.Province,
            PostalCode = dto.PostalCode,
            Renspa = dto.Renspa,
            UserId = userId
        };

        _context.Establishments.Add(establishment);
        await _context.SaveChangesAsync();

        dto.Id = establishment.Id;
        dto.CattleCount = 0;

        return CreatedAtAction(nameof(GetById), new { id = establishment.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EstablishmentDto dto)
    {
        var userId = DefaultUserId;
        var establishment = await _context.Establishments
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (establishment == null)
        {
            return NotFound();
        }

        establishment.Name = dto.Name;
        establishment.RegisterDate = dto.RegisterDate;
        establishment.EstablishmentRegisterDate = dto.EstablishmentRegisterDate;
        establishment.Province = dto.Province;
        establishment.PostalCode = dto.PostalCode;
        establishment.Renspa = dto.Renspa;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = DefaultUserId;
        var establishment = await _context.Establishments
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (establishment == null)
        {
            return NotFound();
        }

        _context.Establishments.Remove(establishment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
