using FleetRent.Domain.Interfaces;

namespace FleetRent.Infrastructure.Storage
{
    public class StorageService : IStorageService
    {
        private readonly string _storagePath = string.Empty;

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            if (contentType != "image/png" && contentType != "image/bmp")
                throw new ArgumentException("Invalid file format. Only PNG or BMP are allowed.");

            var safeName = Path.GetFileName(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeName}";
            var filePath = Path.Combine(_storagePath, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(stream);

            // Retorna caminho relativo pra API poder devolver uma URL se quiser
            return Path.Combine("uploads", uniqueFileName).Replace("\\", "/");
        }
    }
}