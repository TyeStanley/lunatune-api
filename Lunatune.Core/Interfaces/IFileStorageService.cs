namespace Lunatune.Core.Interfaces;

public interface IFileStorageService
{
    Task<Stream> GetFileAsync(string filePath);
    Task<string> GetSasTokenAsync(string filePath, TimeSpan expiryTime);
    Task<string> GetBlobUrlAsync(string filePath);
}
