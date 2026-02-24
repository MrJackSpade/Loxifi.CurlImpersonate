namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// Represents the result of an HTTP request executed by curl.
    /// </summary>
    internal sealed class CurlResult
    {
        public required byte[] Body { get; init; }

        public required string RawHeaders { get; init; }
    }
}
