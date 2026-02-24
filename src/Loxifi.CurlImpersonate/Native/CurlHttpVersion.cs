namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// HTTP version preference for CURLOPT_HTTP_VERSION.
    /// </summary>
    internal enum CurlHttpVersion
    {
        /// <summary>
        /// Let libcurl decide.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use HTTP/1.0.
        /// </summary>
        Http10 = 1,

        /// <summary>
        /// Use HTTP/1.1.
        /// </summary>
        Http11 = 2,

        /// <summary>
        /// Attempt HTTP/2, fall back to HTTP/1.1.
        /// </summary>
        Http2 = 3,

        /// <summary>
        /// Use HTTP/2 over TLS only (HTTPS).
        /// </summary>
        Http2Tls = 4,

        /// <summary>
        /// Use HTTP/2 without TLS upgrade (h2c).
        /// </summary>
        Http2PriorKnowledge = 5,

        /// <summary>
        /// Attempt HTTP/3, fall back to earlier versions.
        /// </summary>
        Http3 = 30,

        /// <summary>
        /// Use HTTP/3 only.
        /// </summary>
        Http3Only = 31,
    }
}
