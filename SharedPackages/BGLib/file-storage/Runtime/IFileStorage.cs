using System.Threading.Tasks;

#nullable enable

public interface IFileStorage {

    Task SaveFileAsync(string fileName, string value, StoragePreference storageLocation);
    Task<string?> LoadFileAsync(string fileName, StoragePreference storageLocation);
    Task DeleteFileAsync(string fileName, StoragePreference storageLocation);
    Task<bool> FileExistsAsync(string fileName, StoragePreference storageLocation);
}
