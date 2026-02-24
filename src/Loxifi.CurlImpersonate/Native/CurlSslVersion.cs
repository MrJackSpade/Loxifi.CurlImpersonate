namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// SSL/TLS version preference for CURLOPT_SSLVERSION.
    /// </summary>
    internal enum CurlSslVersion
    {
        /// <summary>
        /// Let libcurl decide.
        /// </summary>
        Default = 0,

        /// <summary>
        /// TLS 1.0.
        /// </summary>
        TlsV10 = 4,

        /// <summary>
        /// TLS 1.1.
        /// </summary>
        TlsV11 = 5,

        /// <summary>
        /// TLS 1.2.
        /// </summary>
        TlsV12 = 6,

        /// <summary>
        /// TLS 1.3.
        /// </summary>
        TlsV13 = 7,

        /// <summary>
        /// Maximum TLS version mask.
        /// </summary>
        MaxDefault = 0x10000,
        MaxTlsV10 = 0x40000,
        MaxTlsV11 = 0x50000,
        MaxTlsV12 = 0x60000,
        MaxTlsV13 = 0x70000,
    }
}
