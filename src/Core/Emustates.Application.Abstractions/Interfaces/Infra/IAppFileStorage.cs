using System.Threading.Tasks;

namespace Emustates.Application.Abstractions.Interfaces.Infra
{
    public interface IAppFileStorage
    {
        /// <summary>
        /// Upload raw bytes to the specified container/bucket and path. Returns when upload completes.
        /// </summary>
        Task UploadAsync(string container, string path, byte[] content, string contentType = null, bool overwrite = false);

        /// <summary>
        /// Upload a base64 encoded content to the specified container/bucket and path.
        /// </summary>
        Task UploadBase64Async(string container, string path, string base64Content, string contentType = null, bool overwrite = false);

        /// <summary>
        /// Upload a file from a local filesystem path to the specified container/bucket and destination path.
        /// </summary>
        Task UploadFromFilePathAsync(string container, string path, string sourceFilePath, bool overwrite = false);

        /// <summary>
        /// Download content as bytes. Returns null if the object does not exist.
        /// </summary>
        Task<byte[]?> DownloadAsync(string container, string path);

        /// <summary>
        /// Get a URL to access the object. If expiryInSeconds &gt; 0, returns a signed URL valid for the specified time.
        /// </summary>
        Task<string> GetUrlAsync(string container, string path, bool secure = true, int expiryInSeconds = 0);

        /// <summary>
        /// Check if an object exists at the specified path.
        /// </summary>
        Task<bool> ExistsAsync(string container, string path);

        /// <summary>
        /// Delete the object at the specified path. No-op if not found.
        /// </summary>
        Task DeleteAsync(string container, string path);

        /// <summary>
        /// List object paths under the specified prefix. Returns an array of paths.
        /// </summary>
        Task<string[]> ListFilesAsync(string container, string prefix = null, int? maxResults = null);
    }
}
