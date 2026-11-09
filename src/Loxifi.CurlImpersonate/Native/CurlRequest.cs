namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// Represents an HTTP request to be executed by curl.
    /// </summary>
    internal sealed class CurlRequest
    {
        public bool AllowAutoRedirect { get; init; } = true;

        public byte[]? Body { get; init; }

        public string? CookieHeader { get; init; }

        public Dictionary<string, string> Headers { get; init; } = new();

        public bool Insecure { get; init; }

        public required string Method { get; init; }

        /// <summary>
        /// curl-impersonate target name (e.g. "chrome146"), passed to curl_easy_impersonate.
        /// </summary>
        public required string ImpersonateTarget { get; init; }

        public string? ProxyUrl { get; init; }

        public string? ProxyUserPassword { get; init; }

        /// <summary>
        /// When set, the response body is written directly to this stream as it
        /// arrives instead of being buffered in memory. Used for streaming large
        /// downloads that would otherwise exceed the 2 GB MemoryStream limit.
        /// </summary>
        public Stream? ResponseStream { get; init; }

        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);

        public required string Url { get; init; }
    }
}
