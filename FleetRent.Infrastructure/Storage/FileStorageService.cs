using FleetRent.Domain.Exceptions;
using FleetRent.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FleetRent.Infrastructure.Storage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadsFolderPath;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
            // Resolve the uploads directory relative to the application root.
            _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            // Ensure the directory is ready before handling any upload.
            EnsureUploadsFolderExists();
        }

        private void EnsureUploadsFolderExists()
        {
            try
            {
                if (!Directory.Exists(_uploadsFolderPath))
                {
                    Directory.CreateDirectory(_uploadsFolderPath);
                    _logger.LogInformation("Uploads folder created at {Path}.", _uploadsFolderPath);
                }
                else
                {
                    _logger.LogInformation("Uploads folder already exists at {Path}.", _uploadsFolderPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize uploads folder at {Path}.", _uploadsFolderPath);
                throw DomainException.From(DomainErrors.Storage.UploadDirectoryUnavailable, ex);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var allowedTypes = new[] { "image/png", "image/bmp" };

            if (!allowedTypes.Contains(contentType?.ToLowerInvariant() ?? ""))
            {
                throw new ArgumentException(
                    "Invalid file format. Only PNG and BMP are allowed.");
            }

            if (fileStream == null || fileStream.Length == 0)
            {
                throw new ArgumentException("File stream cannot be empty.");
            }

            try
            {
                var extension = GetFileExtension(contentType!);
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var fullFilePath = Path.Combine(_uploadsFolderPath, uniqueFileName);

                _logger.LogInformation("Uploading file {FileName} to {Path}.", uniqueFileName, fullFilePath);

                using (var outputStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await fileStream.CopyToAsync(outputStream);
                }

                var relativePath = Path.Combine("uploads", uniqueFileName)
                    .Replace("\\", "/");

                _logger.LogInformation(
                    "File {FileName} saved successfully at {Path}.", uniqueFileName, fullFilePath);

                return relativePath;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission error while saving file {FileName}.", fileName);
                throw DomainException.From(DomainErrors.Storage.UploadPermissionDenied, ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Upload folder not found while saving file {FileName}.", fileName);
                throw DomainException.From(DomainErrors.Storage.UploadDirectoryNotFound, ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O error while saving file {FileName}.", fileName);
                throw DomainException.From(DomainErrors.Storage.UploadIoFailure, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving file {FileName}.", fileName);
                throw DomainException.From(DomainErrors.Storage.UploadUnexpected, ex);
            }
        }

        private string GetFileExtension(string contentType) =>
            contentType?.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/bmp" => ".bmp",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                _ => ".bin"
            };
    }
}
