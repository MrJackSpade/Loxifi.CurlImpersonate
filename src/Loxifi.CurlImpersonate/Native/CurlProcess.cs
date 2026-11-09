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

            // Browser fingerprint first: libcurl-impersonate configures TLS, HTTP/2 and the browser's
            // default headers in one call. Headers added below override defaults of the same name.
            curl.Impersonate(request.ImpersonateTarget, defaultHeaders: true);

            // Basic request setup
            curl.SetUrl(request.Url);
            curl.SetMethod(request.Method);
            curl.SetFollowLocation(request.AllowAutoRedirect);

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

            // User headers (override the impersonated browser's defaults)
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
    }
}
