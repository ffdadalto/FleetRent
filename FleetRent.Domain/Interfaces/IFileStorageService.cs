namespace FleetRent.Domain.Interfaces
{
    public interface IFileStorageService
    {        
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    }
}
