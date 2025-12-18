using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Emustates.Application.Abstractions.Interfaces.Infra;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Emustates.Infra.MagaluStorage
{
    public class MagaluCloudBlobStorage : IAppFileStorage
    {
        private readonly MagaluStorageOptions _options;
        private readonly ILogger<MagaluCloudBlobStorage> _logger;
        private readonly IAmazonS3 _s3Client;

        public MagaluCloudBlobStorage(IOptions<MagaluStorageOptions> options, ILogger<MagaluCloudBlobStorage> logger)
        {
            _options = options?.Value ?? new MagaluStorageOptions();
            _logger = logger;

            var config = new AmazonS3Config
            {
                ServiceURL = _options.ServiceUrl,
                //SignatureVersion = "4",
                UseHttp = !_options.UseHttps,
                ForcePathStyle = _options.ForcePathStyle,
                Timeout = TimeSpan.FromMinutes(10),
            };

            if (!string.IsNullOrWhiteSpace(_options.Region))
            {
                try { config.RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region); } catch { }
            }

            _s3Client = new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
        }

        public async Task UploadAsync(string container, string path, byte[] content, string contentType = null, bool overwrite = false)
        {
            var bucket = ResolveBucket(container);
            if (!await EnsureBucketExistsAsync(bucket).ConfigureAwait(false))
            {
                _logger?.LogError("Bucket does not exist and could not be created: {Bucket}", bucket);
                throw new InvalidOperationException("Bucket unavailable");
            }

            var key = path ?? string.Empty;

            if (!overwrite)
            {
                if (await ExistsAsync(bucket, key).ConfigureAwait(false))
                {
                    _logger?.LogWarning("Upload skipped because target exists and overwrite=false. Bucket: {Bucket}, Key: {Key}", bucket, key);
                    return;
                }
            }

            using var ms = new MemoryStream(content ?? Array.Empty<byte>());
            var put = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = ms,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(put).ConfigureAwait(false);
        }

        public Task UploadBase64Async(string container, string path, string base64Content, string contentType = null, bool overwrite = false)
        {
            var bytes = Convert.FromBase64String(base64Content ?? string.Empty);
            return UploadAsync(container, path, bytes, contentType, overwrite);
        }

        public Task UploadFromFilePathAsync(string container, string path, string sourceFilePath, bool overwrite = false)
        {
            var data = File.ReadAllBytes(sourceFilePath);
            var contentType = GetContentTypeFromFileName(sourceFilePath);
            return UploadAsync(container, path, data, contentType, overwrite);
        }

        public async Task<byte[]?> DownloadAsync(string container, string path)
        {
            var bucket = ResolveBucket(container);
            var key = path ?? string.Empty;

            try
            {
                var resp = await _s3Client.GetObjectAsync(bucket, key).ConfigureAwait(false);
                using var ms = new MemoryStream();
                await resp.ResponseStream.CopyToAsync(ms).ConfigureAwait(false);
                return ms.ToArray();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<string> GetUrlAsync(string container, string path, bool secure = true, int expiryInSeconds = 0)
        {
            var bucket = ResolveBucket(container);
            var key = path ?? string.Empty;

            if (expiryInSeconds > 0)
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucket,
                    Key = key,
                    Expires = DateTime.UtcNow.AddSeconds(expiryInSeconds),
                    Protocol = secure ? Protocol.HTTPS : Protocol.HTTP
                };
                return _s3Client.GetPreSignedURL(request);
            }

            // Return direct public URL (may not be accessible if object is private)
            var scheme = secure ? "https" : "http";
            var serviceUrl = _options.ServiceUrl?.TrimEnd('/') ?? string.Empty;
            // If ServiceUrl already contains scheme and host, prefer building from it
            if (serviceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || serviceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return $"{serviceUrl}/{bucket}/{key}";
            }

            return $"{scheme}://{serviceUrl}/{bucket}/{key}";
        }

        public async Task<bool> ExistsAsync(string container, string path)
        {
            var bucket = ResolveBucket(container);
            var key = path ?? string.Empty;
            try
            {
                // Use GetObjectMetadataAsync to check existence
                await _s3Client.GetObjectMetadataAsync(bucket, key).ConfigureAwait(false);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error checking existence for {Bucket}/{Key}", bucket, key);
                return false;
            }
        }

        public async Task<string[]> ListFilesAsync(string container, string prefix = null, int? maxResults = null)
        {
            var bucket = ResolveBucket(container);
            var request = new ListObjectsV2Request
            {
                BucketName = bucket,
                Prefix = prefix ?? string.Empty,
                MaxKeys = maxResults ?? 1000
            };

            var results = new System.Collections.Generic.List<string>();
            ListObjectsV2Response resp;
            do
            {
                resp = await _s3Client.ListObjectsV2Async(request).ConfigureAwait(false);
                foreach (var s in resp.S3Objects)
                {
                    results.Add(s.Key);
                }
                request.ContinuationToken = resp.NextContinuationToken;
            } while (resp.IsTruncated == true);

            return results.ToArray();
        }

        public async Task DeleteAsync(string container, string path)
        {
            var bucket = ResolveBucket(container);
            var key = path ?? string.Empty;
            try
            {
                await _s3Client.DeleteObjectAsync(bucket, key).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete object {Bucket}/{Key}", bucket, key);
                throw;
            }
        }

        private string ResolveBucket(string container) => string.IsNullOrWhiteSpace(container) ? (_options.DefaultBucket ?? string.Empty) : container;

        private async Task<bool> EnsureBucketExistsAsync(string bucket)
        {
            if (string.IsNullOrWhiteSpace(bucket)) return false;
            try
            {
                var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucket).ConfigureAwait(false);
                if (!exists)
                {
                    await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucket }).ConfigureAwait(false);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error ensuring bucket exists: {Bucket}", bucket);
                return false;
            }
        }

        private static string GetContentTypeFromFileName(string fileName)
        {
            try
            {
                var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                return ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".pdf" => "application/pdf",
                    ".txt" => "text/plain",
                    ".html" or ".htm" => "text/html",
                    _ => "application/octet-stream",
                };
            }
            catch { return "application/octet-stream"; }
        }
    }
}
