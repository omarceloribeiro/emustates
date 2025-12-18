using System;

namespace Emustates.Infra.MagaluStorage
{
    public class MagaluStorageOptions
    {
        /// <summary>
        /// The Magalu object storage endpoint (e.g. "https://storage.magalucloud.com" or custom S3 compatible endpoint).
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Access Key ID (API key ID)
        /// </summary>
        public string AccessKeyId { get; set; }

        /// <summary>
        /// Secret access key (API secret)
        /// </summary>
        public string SecretAccessKey { get; set; }

        /// <summary>
        /// Default region (optional). For S3-compatible services this may be ignored.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Default bucket/container to use when none is provided.
        /// </summary>
        public string DefaultBucket { get; set; }

        /// <summary>
        /// Use HTTPS when building non-signed URLs.
        /// </summary>
        public bool UseHttps { get; set; } = true;

        /// <summary>
        /// When true, use path-style addressing instead of virtual-hosted-style (useful for S3-compatible endpoints).
        /// </summary>
        public bool ForcePathStyle { get; set; } = true;

        /// <summary>
        /// Timeout in milliseconds for requests (optional).
        /// </summary>
        public int TimeoutMilliseconds { get; set; } = 100000;
    }
}