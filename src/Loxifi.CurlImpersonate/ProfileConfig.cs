namespace Loxifi.CurlImpersonate
{
    /// <summary>
    /// Configuration settings for a browser profile, extracted from curl-impersonate bat files.
    /// </summary>
    public sealed class ProfileConfig
    {
        /// <summary>
        /// Certificate compression algorithm (--cert-compression brotli/zlib).
        /// </summary>
        public string? CertCompression { get; init; }

        /// <summary>
        /// TLS cipher suites to use (--ciphers).
        /// </summary>
        public string? Ciphers { get; init; }

        /// <summary>
        /// Elliptic curves to use (--curves).
        /// </summary>
        public string? Curves { get; init; }

        /// <summary>
        /// Delegated credentials (--delegated-credentials).
        /// </summary>
        public string? DelegatedCredentials { get; init; }

        /// <summary>
        /// ECH mode (--ech grease, --ech true, etc).
        /// </summary>
        public string? EchMode { get; init; }

        /// <summary>
        /// HTTP headers to send with each request.
        /// </summary>
        public Dictionary<string, string> Headers { get; init; } = new();

        /// <summary>
        /// Whether to use HTTP/2 no server push (--http2-no-server-push).
        /// </summary>
        public bool Http2NoServerPush { get; init; }

        /// <summary>
        /// Whether to use HTTP/2 pseudo header order (--http2-pseudo-headers-order).
        /// </summary>
        public string? Http2PseudoHeadersOrder { get; init; }

        /// <summary>
        /// HTTP/2 settings (--http2-settings).
        /// </summary>
        public string? Http2Settings { get; init; }

        /// <summary>
        /// HTTP/2 stream exclusive flag (--http2-stream-exclusive).
        /// </summary>
        public bool Http2StreamExclusive { get; init; }

        /// <summary>
        /// HTTP/2 stream weight (--http2-stream-weight).
        /// </summary>
        public int? Http2StreamWeight { get; init; }

        /// <summary>
        /// HTTP/2 window update value (--http2-window-update).
        /// </summary>
        public int? Http2WindowUpdate { get; init; }

        /// <summary>
        /// Signature hashes to use (--signature-hashes).
        /// </summary>
        public string? SignatureHashes { get; init; }

        /// <summary>
        /// Whether to split cookies (--http2-split-cookies).
        /// </summary>
        public bool SplitCookies { get; init; }

        /// <summary>
        /// Whether to use TLS extension order (--tls-extension-order).
        /// </summary>
        public string? TlsExtensionOrder { get; init; }

        /// <summary>
        /// Record size limit (--tls-record-size-limit).
        /// </summary>
        public int? TlsRecordSizeLimit { get; init; }

        /// <summary>
        /// Whether to use TLS supported versions (--tls-supported-versions).
        /// </summary>
        public string? TlsSupportedVersions { get; init; }

        /// <summary>
        /// Minimum TLS version (--tlsv1.2, --tlsv1.3).
        /// </summary>
        public string? TlsVersion { get; init; }

        /// <summary>
        /// Whether to use ALPS extension (--alps).
        /// </summary>
        public bool UseAlps { get; init; }

        /// <summary>
        /// Whether to use compression (--compressed).
        /// </summary>
        public bool UseCompression { get; init; }

        /// <summary>
        /// Whether to use HTTP/2 (--http2).
        /// </summary>
        public bool UseHttp2 { get; init; }

        /// <summary>
        /// Whether to permute TLS extensions (--tls-permute-extensions).
        /// </summary>
        public bool UsePermuteTlsExtensions { get; init; }

        /// <summary>
        /// Whether to use TLS grease (--tls-grease).
        /// </summary>
        public bool UseTlsGrease { get; init; }

        /// <summary>
        /// Whether to use signed certificate timestamps (--tls-signed-cert-ts).
        /// </summary>
        public bool UseTlsSignedCertTimestamps { get; init; }
    }
}