namespace FinalProject.NET.Services.Cloudinary
{
    public interface ICloudService
    {
        Task<string> UploadAsync(IFormFile file);

    }
}
