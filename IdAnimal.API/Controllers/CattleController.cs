using IdAnimal.API.Extensions;
using IdAnimal.API.Data;
using IdAnimal.API.Services;
using IdAnimal.Shared.DTOs;
using IdAnimal.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace IdAnimal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CattleController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICloudStorageService _cloudStorage;
    private readonly ISnoutAnalysisService _snoutService;

    public CattleController(
        AppDbContext context, 
        ICloudStorageService cloudStorage,
        ISnoutAnalysisService snoutService)
    {
        _context = context;
        _cloudStorage = cloudStorage;
        _snoutService = snoutService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CattleDto>>> GetAll([FromQuery] int? establishmentId = null)
    {
        var userId = User.GetId();
        var query = _context.Cattle
            .Include(c => c.Establishment)
            .Include(c => c.Images)
            .Include(c => c.Videos)
            .Include(c => c.CustomDataValues)
            .ThenInclude(cdv => cdv.CustomDataColumn)
            .Where(c => c.Establishment!.UserId == userId);

        if (establishmentId.HasValue)
        {
            query = query.Where(c => c.EstablishmentId == establishmentId.Value);
        }

        var cattleEntities = await query.ToListAsync();

        var cattleDtos = cattleEntities.Select(c => new CattleDto
        {
            Id = c.Id,
            UserId = userId,
            GlobalId = c.GlobalId,
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
            MainImageUrl = c.Images.Select(i => i.Path).FirstOrDefault() 
                       ?? c.FullImages.Select(i => i.Path).FirstOrDefault(),
            CustomData = c.CustomDataValues
                .Where(cdv => cdv.CustomDataColumn != null)
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
        var userId = User.GetId();
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
            GlobalId = cattle.GlobalId,
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
        var userId = User.GetId();
        var establishment = await _context.Establishments
            .FirstOrDefaultAsync(e => e.Id == dto.EstablishmentId && e.UserId == userId);

        if (establishment == null)
        {
            return BadRequest(new { message = "Invalid establishment" });
        }

        var cattle = new Cattle
        {
            Caravan = dto.Caravan,
            UserId = userId,
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
        var userId = User.GetId();
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
        var userId = User.GetId();
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
    public async Task<IActionResult> UploadImage(
        [FromForm] Guid cattleGlobalId,
        [FromForm] string imageType, 
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        var userId = User.GetId();
        var cattle = await _context.Cattle
            .Include(c => c.Establishment)
            .FirstOrDefaultAsync(c => c.GlobalId == cattleGlobalId && c.Establishment!.UserId == userId);

        if (cattle == null)
        {
            return NotFound(new { message = "Cattle not found" });
        }

        try
        {
            string? descriptorsJson = null;
            string? keypointsJson = null;
            
            // 2. Read file stream into byte array
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();

            // if (imageType.Equals("Snout", StringComparison.OrdinalIgnoreCase))
            // {
            //     var analysisResult = _snoutService.Analyze(imageBytes);

            //     if (analysisResult == null)
            //     {
            //         return BadRequest(new { message = "No snout detected in the uploaded image." });
            //     }

            //     descriptorsJson = analysisResult.DescriptorsJson;
            //     keypointsJson = analysisResult.KeypointsJson;
            // }

            var fileName = $"{cattle.Caravan}_{imageType}_{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}";
            var folder = $"cattle/{cattle.Caravan}/{imageType.ToLower()}";
            
            var imageUrl = await _cloudStorage.UploadImageAsync(Convert.ToBase64String(imageBytes), folder, fileName);

            if (imageType.Equals("Snout", StringComparison.OrdinalIgnoreCase))
            {
                var image = new CattleImage
                {
                    Path = imageUrl,
                    AddedDate = DateTime.UtcNow,
                    CattleId = cattle.Id,
                    Descriptors = descriptorsJson,
                    Keypoints = keypointsJson,
                    UserId = userId
                };
                _context.CattleImages.Add(image);
            }
            else
            {
                var fullImage = new CattleFullImage
                {
                    Path = imageUrl,
                    UserId = userId,
                    AddedDate = DateTime.UtcNow,
                    CattleId = cattle.Id
                };
                _context.CattleFullImages.Add(fullImage);
            }

            await _context.SaveChangesAsync();

            var response = new 
            { 
                imageUrl = imageUrl,
                descriptors = descriptorsJson != null ? JsonSerializer.Deserialize<object>(descriptorsJson) : new List<object>(),
                keypoints = keypointsJson != null ? JsonSerializer.Deserialize<object>(keypointsJson) : new List<object>()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Upload failed: {ex.Message}" });
        }
    }

    [HttpPost("compare")]
    public IActionResult CompareDescriptors([FromBody] CompareDescriptorsDto request)
    {
        if (request.Descriptors1 == null || request.Descriptors2 == null || 
            request.Descriptors1.Count == 0 || request.Descriptors2.Count == 0)
        {
            return BadRequest(new { error = "Both descriptor lists must be provided and non-empty." });
        }
        return Ok(new { good_matches = 0, matches = 0 });
        // try
        // {
        //     var result = _snoutService.Compare(request.Descriptors1, request.Descriptors2);
        //     return Ok(new { good_matches = result.GoodMatchCount, matches = result.Matches });
        // }
        // catch (Exception ex)
        // {
        //     // Logging would go here
        //     return BadRequest(new { error = $"Comparison failed: {ex.Message}" });
        // }
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

public class CompareDescriptorsDto
{
    public List<List<float>> Descriptors1 { get; set; } = new();
    public List<List<float>> Descriptors2 { get; set; } = new();
}