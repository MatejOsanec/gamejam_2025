using System.Threading.Tasks;
using Zenject;

#nullable enable

[ZenjectAllowDuringValidation]
public class NoFileStorage : IFileStorage {

    public Task SaveFileAsync(string fileName, string value, StoragePreference storageLocation) {

        return Task.CompletedTask;
    }

    public Task<string?> LoadFileAsync(string fileName, StoragePreference storageLocation) {

        return Task.FromResult<string?>(null);
    }

    public Task DeleteFileAsync(string fileName, StoragePreference storageLocation) {

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string fileName, StoragePreference storageLocation) {

        return Task.FromResult(false);
    }
}
