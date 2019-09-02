namespace University.Services.Implementations
{
    using System.IO;
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary cloudinary; // Singleton

        public CloudinaryService(Cloudinary cloudinary)
        {
            this.cloudinary = cloudinary;
        }

        public string UploadFile(byte[] fileBytes, string fileName, string cloudFileFolder)
        {
            if (fileBytes == null
                || fileBytes.Length == 0
                || string.IsNullOrWhiteSpace(fileName)
                || string.IsNullOrWhiteSpace(cloudFileFolder))
            {
                return null;
            }

            RawUploadResult rawUploadResult = null;
            using (var memoryStream = new MemoryStream(fileBytes))
            {
                var rawUploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, memoryStream),
                    UseFilename = true,
                    UniqueFilename = true,
                    Folder = $"{ServicesConstants.CloudProjectFolder}/{cloudFileFolder}"
                };

                rawUploadResult = this.cloudinary.Upload(rawUploadParams);
            }

            var url = rawUploadResult?.SecureUri.AbsoluteUri;
            return url;
        }
    }
}
