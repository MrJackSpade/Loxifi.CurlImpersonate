using Loxifi.CurlImpersonate.Exceptions;

namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// Handles execution of HTTP requests using the native libcurl P/Invoke layer.
    /// </summary>
    internal sealed class CurlProcess : IDisposable
    {
        private readonly string? _caCertPath;
        private bool _disposed;

        public CurlProcess()
        {
            _caCertPath = FindCaCertificate();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        public Task<CurlResult> ExecuteAsync(
            CurlRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Execute(request), cancellationToken);
        }

        private static string? FindCaCertificate()
        {
            string assemblyLocation = typeof(CurlProcess).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;

            string[] possiblePaths =
            [
                Path.Combine(assemblyDir, "runtimes", "win-x64", "native", "cacert.pem"),
                Path.Combine(assemblyDir, "cacert.pem"),
                Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", "cacert.pem"),
                Path.Combine(AppContext.BaseDirectory, "cacert.pem"),
            ];

            foreach (string? path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private CurlResult Execute(CurlRequest request)
        {
            using CurlEasy curl = new();

            // Basic request setup
            curl.SetUrl(request.Url);
            curl.SetMethod(request.Method);
            curl.SetFollowLocation(true);

            // Timeout
            if (request.Timeout.TotalMilliseconds > 0)
            {
                curl.SetTimeoutMs((int)request.Timeout.TotalMilliseconds);
            }

            // SSL settings
            if (request.Insecure)
            {
                curl.SetSslVerifyPeer(false);
                curl.SetSslVerifyHost(false);
            }
            else if (_caCertPath != null)
            {
                curl.SetCaInfo(_caCertPath);
            }

            // Proxy
            if (!string.IsNullOrEmpty(request.ProxyUrl))
            {
                curl.SetProxy(request.ProxyUrl);

                if (!string.IsNullOrEmpty(request.ProxyUserPassword))
                {
                    curl.SetProxyUserPassword(request.ProxyUserPassword);
                }
            }

            // Cookies
            if (!string.IsNullOrEmpty(request.CookieHeader))
            {
                curl.SetCookie(request.CookieHeader);
            }

            // Apply profile settings
            ApplyProfileSettings(curl, request.Profile);

            // User headers (override profile headers)
            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                curl.AddHeader($"{header.Key}: {header.Value}");
            }

            // Request body
            if (request.Body != null && request.Body.Length > 0)
            {
                curl.SetRequestBody(request.Body);
            }

            // Perform the request
            try
            {
                CurlResponse response = curl.Perform(request.ResponseStream);

                return new CurlResult
                {
                    Body = response.Body,
                    RawHeaders = response.RawHeaders
                };
            }
            catch (CurlNativeException ex)
            {
                throw new CurlException(
                    $"curl error: {ex.Message}",
                    (int)ex.Code,
                    ex.Message);
            }
        }

        private static void ApplyProfileSettings(CurlEasy curl, ProfileConfig profile)
        {
            // TLS cipher suites
            if (!string.IsNullOrEmpty(profile.Ciphers))
            {
                curl.SetCiphers(profile.Ciphers);
            }

            // Elliptic curves
            if (!string.IsNullOrEmpty(profile.Curves))
            {
                curl.SetCurves(profile.Curves);
            }

            // HTTP/2 settings
            if (!string.IsNullOrEmpty(profile.Http2Settings))
            {
                curl.SetHttp2Settings(profile.Http2Settings);
            }

            if (profile.Http2WindowUpdate.HasValue)
            {
                curl.SetHttp2WindowUpdate(profile.Http2WindowUpdate.Value);
            }

            if (profile.Http2StreamWeight.HasValue)
            {
                curl.SetHttp2StreamWeight(profile.Http2StreamWeight.Value);
            }

            if (profile.Http2StreamExclusive)
            {
                curl.SetHttp2StreamExclusive(true);
            }

            if (!string.IsNullOrEmpty(profile.Http2PseudoHeadersOrder))
            {
                curl.SetHttp2PseudoHeadersOrder(profile.Http2PseudoHeadersOrder);
            }

            // Compression
            if (profile.UseCompression)
            {
                curl.SetAcceptEncoding("gzip, deflate, br");
            }

            // HTTP version
            if (profile.UseHttp2)
            {
                curl.SetHttpVersion(CurlHttpVersion.Http2);
            }

            // ALPS
            if (profile.UseAlps)
            {
                curl.SetEnableAlps(true);
            }

            // TLS extension permutation
            if (profile.UsePermuteTlsExtensions)
            {
                curl.SetPermuteExtensions(true);
            }

            // Certificate compression
            if (!string.IsNullOrEmpty(profile.CertCompression))
            {
                curl.SetCertCompression(profile.CertCompression);
            }

            // TLS GREASE
            if (profile.UseTlsGrease)
            {
                curl.SetTlsGrease(true);
            }

            // TLS signed certificate timestamps
            if (profile.UseTlsSignedCertTimestamps)
            {
                curl.SetTlsSignedCertTimestamps(true);
            }

            // Signature hashes
            if (!string.IsNullOrEmpty(profile.SignatureHashes))
            {
                curl.SetSignatureHashes(profile.SignatureHashes);
            }

            // Delegated credentials
            if (!string.IsNullOrEmpty(profile.DelegatedCredentials))
            {
                curl.SetDelegatedCredentials(profile.DelegatedCredentials);
            }

            // TLS record size limit
            if (profile.TlsRecordSizeLimit.HasValue)
            {
                curl.SetTlsRecordSizeLimit(profile.TlsRecordSizeLimit.Value);
            }

            // TLS extension order
            if (!string.IsNullOrEmpty(profile.TlsExtensionOrder))
            {
                curl.SetTlsExtensionOrder(profile.TlsExtensionOrder);
            }

            // Split cookies
            if (profile.SplitCookies)
            {
                curl.SetSplitCookies(true);
            }

            // Profile headers
            foreach (KeyValuePair<string, string> header in profile.Headers)
            {
                curl.AddHeader($"{header.Key}: {header.Value}");
            }
        }
    }
}
