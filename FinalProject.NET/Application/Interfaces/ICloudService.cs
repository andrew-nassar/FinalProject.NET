namespace FinalProject.NET.Application.Interfaces
{
    public interface ICloudService
    {
        Task<string> UploadAsync(IFormFile file);

    }
}
