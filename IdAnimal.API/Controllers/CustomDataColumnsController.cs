using IdAnimal.API.Data;
using IdAnimal.Shared.DTOs;
using IdAnimal.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdAnimal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomDataColumnsController : ControllerBase
{
    private readonly AppDbContext _context;
    private const int DefaultUserId = 1; // Since we removed auth, use a default user

    public CustomDataColumnsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomDataColumnDto>>> GetAll()
    {
        var columns = await _context.CustomDataColumns
            .Where(cdc => cdc.UserId == DefaultUserId)
            .OrderBy(cdc => cdc.ColumnName)
            .Select(c => new CustomDataColumnDto
            {
                Id = c.Id,
                ColumnName = c.ColumnName,
                DataType = c.DataType
            })
            .ToListAsync();

        return Ok(columns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomDataColumnDto>> GetById(int id)
    {
        var column = await _context.CustomDataColumns
            .Where(cdc => cdc.Id == id && cdc.UserId == DefaultUserId)
            .Select(c => new CustomDataColumnDto
            {
                Id = c.Id,
                ColumnName = c.ColumnName,
                DataType = c.DataType
            })
            .FirstOrDefaultAsync();

        if (column == null)
        {
            return NotFound();
        }

        return Ok(column);
    }

    [HttpPost]
    public async Task<ActionResult<CustomDataColumnDto>> Create([FromBody] CustomDataColumnDto dto)
    {
        var exists = await _context.CustomDataColumns
            .AnyAsync(cdc => cdc.ColumnName == dto.ColumnName && cdc.UserId == DefaultUserId);

        if (exists)
        {
            return BadRequest(new { message = "Column already exists" });
        }

        var column = new CustomDataColumn
        {
            ColumnName = dto.ColumnName,
            DataType = dto.DataType,
            UserId = DefaultUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomDataColumns.Add(column);
        await _context.SaveChangesAsync();

        dto.Id = column.Id;
        return CreatedAtAction(nameof(GetById), new { id = column.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomDataColumnDto dto)
    {
        var column = await _context.CustomDataColumns
            .FirstOrDefaultAsync(cdc => cdc.Id == id && cdc.UserId == DefaultUserId);

        if (column == null)
        {
            return NotFound();
        }

        column.ColumnName = dto.ColumnName;
        column.DataType = dto.DataType;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var column = await _context.CustomDataColumns
            .FirstOrDefaultAsync(cdc => cdc.Id == id && cdc.UserId == DefaultUserId);

        if (column == null)
        {
            return NotFound();
        }

        _context.CustomDataColumns.Remove(column);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
