namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// Represents an HTTP request to be executed by curl.
    /// </summary>
    internal sealed class CurlRequest
    {
        public byte[]? Body { get; init; }

        public string? CookieHeader { get; init; }

        public Dictionary<string, string> Headers { get; init; } = new();

        public bool Insecure { get; init; }

        public required string Method { get; init; }

        public required ProfileConfig Profile { get; init; }

        public string? ProxyUrl { get; init; }

        public string? ProxyUserPassword { get; init; }

        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);

        public required string Url { get; init; }
    }
}
