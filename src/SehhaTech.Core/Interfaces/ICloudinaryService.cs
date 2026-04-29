namespace SehhaTech.Core.Interfaces;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder = "general");
    Task<bool> DeleteImageAsync(string publicId);
}

public class CloudinaryUploadResult
{
    public bool Success { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}