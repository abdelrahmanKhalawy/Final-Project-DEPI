using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehhaTech.Core.Interfaces;

namespace SehhaTech.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "general")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was sent." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "File size exceeds the maximum allowed size (5 MB)." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { message = "Unsupported file format. Allowed: jpg, jpeg, png, webp" });

        using var stream = file.OpenReadStream();
        var result = await _cloudinaryService.UploadImageAsync(stream, file.FileName, folder);

        if (!result.Success)
            return StatusCode(500, new { message = "Image upload failed.", error = result.Error });

        return Ok(new
        {
            url = result.Url,
            publicId = result.PublicId
        });
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return BadRequest(new { message = "publicId is required." });

        var deleted = await _cloudinaryService.DeleteImageAsync(publicId);

        if (!deleted)
            return StatusCode(500, new { message = "Image deletion failed." });

        return Ok(new { message = "Image deleted successfully." });
    }
}