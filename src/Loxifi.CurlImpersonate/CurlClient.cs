using Loxifi.CurlImpersonate.Native;
using System.Net;
using System.Text.RegularExpressions;

namespace Loxifi.CurlImpersonate
{
    /// <summary>
    /// An HTTP client that uses curl-impersonate to make requests with browser fingerprint impersonation.
    /// Provides an API similar to HttpClient for easy integration.
    /// </summary>
    public sealed partial class CurlClient : IDisposable
    {
        private readonly CurlProcess _curlProcess;

        private readonly Dictionary<string, string> _defaultHeaders = new();

        private readonly BrowserProfile _profile;

        private readonly ProfileConfig _profileConfig;

        private bool _disposed;

        /// <summary>
        /// Creates a new CurlClient with the specified browser profile.
        /// </summary>
        /// <param name="profile">The browser profile to impersonate.</param>
        public CurlClient(BrowserProfile profile)
        {
            _profile = profile;
            _profileConfig = BrowserProfileSettings.GetSettings(profile);
            _curlProcess = new CurlProcess();
        }

        /// <summary>
        /// Gets or sets the base address for requests.
        /// </summary>
        public Uri? BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets whether requests automatically follow redirect responses.
        /// Default is true.
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// Gets the cookie container used for storing cookies across requests.
        /// </summary>
        public CookieContainer CookieContainer { get; } = new();

        /// <summary>
        /// Gets the default request headers that will be sent with each request.
        /// </summary>
        public IDictionary<string, string> DefaultRequestHeaders => _defaultHeaders;

        /// <summary>
        /// Gets or sets whether to skip SSL certificate verification.
        /// WARNING: This is insecure and should only be used for testing.
        /// </summary>
        public bool Insecure { get; set; }

        /// <summary>
        /// Gets or sets the proxy to use for requests.
        /// </summary>
        public WebProxy? Proxy { get; set; }

        /// <summary>
        /// Gets or sets the request timeout. Default is 100 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// Gets or sets whether to automatically handle cookies.
        /// When true, cookies from responses are stored and sent with subsequent requests.
        /// Default is true.
        /// </summary>
        public bool UseCookies { get; set; } = true;

        /// <summary>
        /// Send a DELETE request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
        }

        /// <summary>
        /// Send a DELETE request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
        }

        /// <summary>
        /// Releases resources used by this CurlClient.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _curlProcess.Dispose();
        }

        /// <summary>
        /// Send a GET request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
        }

        /// <summary>
        /// Send a GET request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
        }

        /// <summary>
        /// Send a GET request and return the response body as a byte array.
        /// </summary>
        public async Task<byte[]> GetByteArrayAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Send a GET request and return the response body as a byte array.
        /// </summary>
        public async Task<byte[]> GetByteArrayAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Send a GET request and return the response body as a stream.
        /// </summary>
        public async Task<Stream> GetStreamAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// Send a GET request and return the response body as a stream.
        /// </summary>
        public async Task<Stream> GetStreamAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// Send a GET request and return the response body as a string.
        /// </summary>
        public async Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Send a GET request and return the response body as a string.
        /// </summary>
        public async Task<string> GetStringAsync(Uri requestUri, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Send a PATCH request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent? content, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content }, cancellationToken);
        }

        /// <summary>
        /// Send a PATCH request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent? content, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content }, cancellationToken);
        }

        /// <summary>
        /// Send a POST request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content }, cancellationToken);
        }

        /// <summary>
        /// Send a POST request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent? content, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content }, cancellationToken);
        }

        /// <summary>
        /// Send a PUT request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent? content, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content }, cancellationToken);
        }

        /// <summary>
        /// Send a PUT request to the specified URI.
        /// </summary>
        public Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent? content, CancellationToken cancellationToken = default)
        {
            return this.SendAsync(new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content }, cancellationToken);
        }

        /// <summary>
        /// Send a GET request and stream the response body directly into <paramref name="destination"/>
        /// without buffering it in memory. Use this for large downloads.
        /// </summary>
        public Task<HttpResponseMessage> DownloadAsync(string requestUri, Stream destination, CancellationToken cancellationToken = default)
        {
            return this.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), destination, cancellationToken);
        }

        /// <summary>
        /// Send a GET request and stream the response body directly into <paramref name="destination"/>
        /// without buffering it in memory. Use this for large downloads.
        /// </summary>
        public Task<HttpResponseMessage> DownloadAsync(Uri requestUri, Stream destination, CancellationToken cancellationToken = default)
        {
            return this.DownloadAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), destination, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request and stream the response body directly into <paramref name="destination"/>
        /// without buffering it in memory. The returned <see cref="HttpResponseMessage"/> has the status
        /// and headers populated but an empty <see cref="HttpResponseMessage.Content"/>.
        /// </summary>
        public Task<HttpResponseMessage> DownloadAsync(HttpRequestMessage request, Stream destination, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(destination);
            return this.SendAsyncCore(request, destination, cancellationToken);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return this.SendAsyncCore(request, responseStream: null, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request, Stream? responseStream, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            string url = this.BuildUrl(request.RequestUri);
            Uri requestUri = request.RequestUri!.IsAbsoluteUri ? request.RequestUri : new Uri(BaseAddress!, request.RequestUri);
            Dictionary<string, string> headers = this.MergeHeaders(request);
            byte[]? body = null;

            if (request.Content != null)
            {
                body = await request.Content.ReadAsByteArrayAsync(cancellationToken);

                // Add content headers
                if (request.Content.Headers.ContentType != null)
                {
                    headers["Content-Type"] = request.Content.Headers.ContentType.ToString();
                }
            }

            // Get cookies for this request
            string? cookieHeader = null;
            if (UseCookies)
            {
                string cookies = CookieContainer.GetCookieHeader(requestUri);
                if (!string.IsNullOrEmpty(cookies))
                {
                    cookieHeader = cookies;
                }
            }

            // Build proxy URL if set
            string? proxyUrl = null;
            string? proxyUserPassword = null;
            if (Proxy != null)
            {
                Uri? proxyUri = Proxy.GetProxy(requestUri);
                if (proxyUri != null && !Proxy.IsBypassed(requestUri))
                {
                    proxyUrl = proxyUri.ToString();
                    if (Proxy.Credentials != null)
                    {
                        NetworkCredential? credentials = Proxy.Credentials.GetCredential(proxyUri, "Basic");
                        if (credentials != null)
                        {
                            proxyUserPassword = $"{credentials.UserName}:{credentials.Password}";
                        }
                    }
                }
            }

            CurlRequest curlRequest = new()
            {
                AllowAutoRedirect = AllowAutoRedirect,
                Method = request.Method.Method,
                Url = url,
                Profile = _profileConfig,
                Headers = headers,
                Body = body,
                Timeout = Timeout,
                Insecure = Insecure,
                CookieHeader = cookieHeader,
                ProxyUrl = proxyUrl,
                ProxyUserPassword = proxyUserPassword,
                ResponseStream = responseStream
            };

            CurlResult result = await _curlProcess.ExecuteAsync(curlRequest, cancellationToken);
            HttpResponseMessage response = ParseResponse(result, requestUri);

            // Store cookies from all responses in redirect chain
            if (UseCookies)
            {
                this.ExtractCookiesFromAllResponses(result.RawHeaders, requestUri);
            }

            return response;
        }

        private static bool IsContentHeader(string name)
        {
            return name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-Language", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-Location", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-MD5", StringComparison.OrdinalIgnoreCase) ||
                   name.Equals("Content-Range", StringComparison.OrdinalIgnoreCase);
        }

        private static HttpResponseMessage ParseResponse(CurlResult result, Uri requestUri)
        {
            HttpResponseMessage response = new();

            // Parse headers
            string[] headerLines = result.RawHeaders.Split(["\r\n", "\n"], StringSplitOptions.None);
            string lastStatusLine = string.Empty;

            // Find the last status line (to handle redirects)
            foreach (string line in headerLines)
            {
                if (StatusLineRegex().IsMatch(line))
                {
                    lastStatusLine = line;
                }
            }

            // Parse status code from the last status line
            Match statusMatch = StatusLineRegex().Match(lastStatusLine);
            if (statusMatch.Success)
            {
                response.StatusCode = (HttpStatusCode)int.Parse(statusMatch.Groups[1].Value);
                response.ReasonPhrase = statusMatch.Groups[2].Value;
            }
            else
            {
                response.StatusCode = HttpStatusCode.OK;
            }

            // Parse header values (from last response block)
            bool inLastResponse = false;
            foreach (string line in headerLines)
            {
                if (StatusLineRegex().IsMatch(line))
                {
                    inLastResponse = line == lastStatusLine;
                    continue;
                }

                if (!inLastResponse || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    string name = line[..colonIndex].Trim();
                    string value = line[(colonIndex + 1)..].Trim();

                    // Try to add as content header first, then as response header
                    if (!response.Content?.Headers.TryAddWithoutValidation(name, value) ?? true)
                    {
                        response.Headers.TryAddWithoutValidation(name, value);
                    }
                }
            }

            // Set content
            response.Content = new ByteArrayContent(result.Body);

            // Re-parse content headers
            foreach (string line in headerLines)
            {
                if (StatusLineRegex().IsMatch(line))
                {
                    inLastResponse = line == lastStatusLine;
                    continue;
                }

                if (!inLastResponse || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    string name = line[..colonIndex].Trim();
                    string value = line[(colonIndex + 1)..].Trim();

                    if (IsContentHeader(name))
                    {
                        response.Content.Headers.TryAddWithoutValidation(name, value);
                    }
                    else
                    {
                        response.Headers.TryAddWithoutValidation(name, value);
                    }
                }
            }

            return response;
        }

        [GeneratedRegex(@"^HTTP/[\d.]+ (\d+)\s*(.*)$")]
        private static partial Regex StatusLineRegex();

        private string BuildUrl(Uri? requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            if (requestUri.IsAbsoluteUri)
            {
                return requestUri.ToString();
            }

            if (BaseAddress == null)
            {
                throw new InvalidOperationException("BaseAddress must be set to use relative URIs");
            }

            return new Uri(BaseAddress, requestUri).ToString();
        }

        private void ExtractCookiesFromAllResponses(string rawHeaders, Uri requestUri)
        {
            // Extract Set-Cookie headers from ALL responses in the chain (for redirects)
            string[] lines = rawHeaders.Split(["\r\n", "\n"], StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (line.StartsWith("Set-Cookie:", StringComparison.OrdinalIgnoreCase))
                {
                    string cookieValue = line["Set-Cookie:".Length..].Trim();
                    try
                    {
                        CookieContainer.SetCookies(requestUri, cookieValue);
                    }
                    catch
                    {
                        // Ignore invalid cookies
                    }
                }
            }
        }

        private Dictionary<string, string> MergeHeaders(HttpRequestMessage request)
        {
            Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);

            // Start with default headers
            foreach (KeyValuePair<string, string> header in _defaultHeaders)
            {
                headers[header.Key] = header.Value;
            }

            // Add request headers (override defaults)
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }

            return headers;
        }
    }
}
