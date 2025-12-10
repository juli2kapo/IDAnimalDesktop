using IdAnimal.API.Data;
using IdAnimal.API.Services;
using IdAnimal.Shared.DTOs;
using IdAnimal.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdAnimal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CattleController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICloudStorageService _cloudStorage;
    private const int DefaultUserId = 1; // Since we removed auth, use a default user

    public CattleController(AppDbContext context, ICloudStorageService cloudStorage)
    {
        _context = context;
        _cloudStorage = cloudStorage;
    }

    [HttpGet]
    public async Task<ActionResult<List<CattleDto>>> GetAll([FromQuery] int? establishmentId = null, [FromQuery] int? userId = null)
    {
        var query = _context.Cattle
            .Include(c => c.Establishment)
            .Include(c => c.Images)
            .Include(c => c.Videos)
            .Include(c => c.CustomDataValues)
            .ThenInclude(cdv => cdv.CustomDataColumn) // Ensure this is included
            .Where(c => c.Establishment!.UserId == userId);

        if (establishmentId.HasValue)
        {
            query = query.Where(c => c.EstablishmentId == establishmentId.Value);
        }

        // 1. Execute Query and bring entities to memory
        var cattleEntities = await query.ToListAsync();

        // 2. Map to DTO in memory (Client-side evaluation)
        var cattleDtos = cattleEntities.Select(c => new CattleDto
        {
            Id = c.Id,
            Caravan = c.Caravan,
            Name = c.Name,
            Weight = c.Weight,
            Origin = c.Origin,
            Age = c.Age,
            Gender = c.Gender,
            GDM = c.GDM,
            EstablishmentId = c.EstablishmentId,
            EstablishmentName = c.Establishment?.Name ?? "Sin Nombre",
            ImageCount = c.Images.Count,
            VideoCount = c.Videos.Count,
            // Now ToDictionary works because we are in memory
            CustomData = c.CustomDataValues
                .Where(cdv => cdv.CustomDataColumn != null) // Safety check
                .ToDictionary(
                    cdv => cdv.CustomDataColumn!.ColumnName,
                    cdv => cdv.Value
                )
        }).ToList();

        return Ok(cattleDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CattleDetailDto>> GetById(int id)
    {
        var userId = DefaultUserId;
        var cattle = await _context.Cattle
            .Include(c => c.Establishment)
            .Include(c => c.Images)
            .Include(c => c.FullImages)
            .Include(c => c.Videos)
            .Include(c => c.CustomDataValues)
            .ThenInclude(cdv => cdv.CustomDataColumn)
            .Where(c => c.Id == id && c.Establishment!.UserId == userId)
            .FirstOrDefaultAsync();

        if (cattle == null)
        {
            return NotFound();
        }

        var dto = new CattleDetailDto
        {
            Id = cattle.Id,
            Caravan = cattle.Caravan,
            Name = cattle.Name,
            Weight = cattle.Weight,
            Origin = cattle.Origin,
            Age = cattle.Age,
            Gender = cattle.Gender,
            GDM = cattle.GDM,
            EstablishmentId = cattle.EstablishmentId,
            EstablishmentName = cattle.Establishment?.Name,
            Images = cattle.Images.Select(i => new CattleImageDto
            {
                Id = i.Id,
                Path = i.Path,
                AddedDate = i.AddedDate
            }).ToList(),
            FullImages = cattle.FullImages.Select(i => new CattleFullImageDto
            {
                Id = i.Id,
                Path = i.Path,
                AddedDate = i.AddedDate
            }).ToList(),
            Videos = cattle.Videos.Select(v => new CattleVideoDto
            {
                Id = v.Id,
                Path = v.Path,
                AddedDate = v.AddedDate
            }).ToList(),
            CustomData = cattle.CustomDataValues.ToDictionary(
                cdv => cdv.CustomDataColumn!.ColumnName,
                cdv => cdv.Value
            )
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CattleDto>> Create([FromBody] CattleDto dto)
    {
        var userId = DefaultUserId;
        var establishment = await _context.Establishments
            .FirstOrDefaultAsync(e => e.Id == dto.EstablishmentId && e.UserId == userId);

        if (establishment == null)
        {
            return BadRequest(new { message = "Invalid establishment" });
        }

        var cattle = new Cattle
        {
            Caravan = dto.Caravan,
            Name = dto.Name,
            Weight = dto.Weight,
            Origin = dto.Origin,
            Age = dto.Age,
            Gender = dto.Gender,
            GDM = dto.GDM,
            EstablishmentId = dto.EstablishmentId
        };

        _context.Cattle.Add(cattle);
        await _context.SaveChangesAsync();

        // Add custom data if provided
        if (dto.CustomData != null && dto.CustomData.Any())
        {
            await UpdateCustomDataAsync(cattle.Id, userId, dto.CustomData);
        }

        dto.Id = cattle.Id;
        dto.EstablishmentName = establishment.Name;

        return CreatedAtAction(nameof(GetById), new { id = cattle.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CattleDto dto)
    {
        var userId = DefaultUserId;
        var cattle = await _context.Cattle
            .Include(c => c.Establishment)
            .FirstOrDefaultAsync(c => c.Id == id && c.Establishment!.UserId == userId);

        if (cattle == null)
        {
            return NotFound();
        }

        cattle.Caravan = dto.Caravan;
        cattle.Name = dto.Name;
        cattle.Weight = dto.Weight;
        cattle.Origin = dto.Origin;
        cattle.Age = dto.Age;
        cattle.Gender = dto.Gender;
        cattle.GDM = dto.GDM;

        // Update custom data if provided
        if (dto.CustomData != null)
        {
            await UpdateCustomDataAsync(cattle.Id, userId, dto.CustomData);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = DefaultUserId;
        var cattle = await _context.Cattle
            .Include(c => c.Establishment)
            .FirstOrDefaultAsync(c => c.Id == id && c.Establishment!.UserId == userId);

        if (cattle == null)
        {
            return NotFound();
        }

        _context.Cattle.Remove(cattle);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage([FromBody] UploadImageRequest request)
    {
        var userId = DefaultUserId;
        var cattle = await _context.Cattle
            .Include(c => c.Establishment)
            .FirstOrDefaultAsync(c => c.Id == request.CattleId && c.Establishment!.UserId == userId);

        if (cattle == null)
        {
            return NotFound(new { message = "Cattle not found" });
        }

        try
        {
            var fileName = $"{cattle.Caravan}_{request.ImageType}_{DateTime.UtcNow.Ticks}";
            var folder = $"cattle/{cattle.Caravan}/{request.ImageType.ToLower()}";
            var imageUrl = await _cloudStorage.UploadImageAsync(request.ImageBase64, folder, fileName);

            if (request.ImageType.Equals("Snout", StringComparison.OrdinalIgnoreCase))
            {
                var image = new CattleImage
                {
                    Path = imageUrl,
                    AddedDate = DateTime.UtcNow,
                    CattleId = cattle.Id,
                    Descriptors = request.Descriptors,
                    Keypoints = request.Keypoints
                };
                _context.CattleImages.Add(image);
            }
            else
            {
                var fullImage = new CattleFullImage
                {
                    Path = imageUrl,
                    AddedDate = DateTime.UtcNow,
                    CattleId = cattle.Id
                };
                _context.CattleFullImages.Add(fullImage);
            }

            await _context.SaveChangesAsync();

            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Upload failed: {ex.Message}" });
        }
    }

    private async Task UpdateCustomDataAsync(int cattleId, int userId, Dictionary<string, string> customData)
    {
        var existingValues = await _context.CustomDataValues
            .Include(cdv => cdv.CustomDataColumn)
            .Where(cdv => cdv.CattleId == cattleId && cdv.CustomDataColumn!.UserId == userId)
            .ToListAsync();

        foreach (var (columnName, value) in customData)
        {
            var column = await _context.CustomDataColumns
                .FirstOrDefaultAsync(cdc => cdc.ColumnName == columnName && cdc.UserId == userId);

            if (column == null)
            {
                column = new CustomDataColumn
                {
                    ColumnName = columnName,
                    DataType = "String",
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CustomDataColumns.Add(column);
                await _context.SaveChangesAsync();
            }

            var existingValue = existingValues.FirstOrDefault(ev => ev.CustomDataColumnId == column.Id);
            if (existingValue != null)
            {
                existingValue.Value = value;
            }
            else
            {
                _context.CustomDataValues.Add(new CustomDataValue
                {
                    CustomDataColumnId = column.Id,
                    CattleId = cattleId,
                    Value = value
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
