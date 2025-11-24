using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace FinalProject.NET.Services.Cloudinary
{
    public class CloudinaryService : ICloudService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "lawyers",
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            // تحقق من نجاح الرفع
            if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK || result.SecureUrl == null)
                throw new Exception($"Cloudinary upload failed: {result?.Error?.Message ?? "Unknown error"}");

            return result.SecureUrl.ToString();
        }

    }
}
