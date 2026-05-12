using System.Runtime.InteropServices;
using System.Text;

namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// High-level wrapper around the libcurl easy interface.
    /// Provides a managed API for making HTTP requests.
    /// </summary>
    internal sealed class CurlEasy : IDisposable
    {
        private static readonly object _globalInitLock = new();
        private static bool _globalInitialized;

        private readonly CurlHandle _handle;
        private readonly List<GCHandle> _pinnedHandles = new();

        private CurlSlistHandle? _headers;
        private bool _disposed;

        // Callback delegates - must be stored to prevent GC
        private WriteCallback? _writeCallback;
        private HeaderCallback? _headerCallback;

        // Response data collectors
        private MemoryStream? _responseBody;
        private Stream? _responseStream;
        private StringBuilder? _responseHeaders;

        public CurlEasy()
        {
            EnsureGlobalInit();
            _handle = LibCurl.curl_easy_init();
            if (_handle.IsInvalid)
            {
                throw new InvalidOperationException("Failed to initialize curl handle");
            }
        }

        /// <summary>
        /// Gets the last response code.
        /// </summary>
        public int ResponseCode { get; private set; }

        /// <summary>
        /// Sets the URL to request.
        /// </summary>
        public void SetUrl(string url)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Url, url));
        }

        /// <summary>
        /// Sets the HTTP method (via custom request).
        /// </summary>
        public void SetMethod(string method)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.CustomRequest, method));
        }

        /// <summary>
        /// Enables following redirects.
        /// </summary>
        public void SetFollowLocation(bool follow)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.FollowLocation, follow ? 1L : 0L));
        }

        /// <summary>
        /// Sets the maximum number of redirects to follow.
        /// </summary>
        public void SetMaxRedirects(int maxRedirects)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.MaxRedirs, maxRedirects));
        }

        /// <summary>
        /// Sets the request timeout in seconds.
        /// </summary>
        public void SetTimeout(int seconds)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Timeout, seconds));
        }

        /// <summary>
        /// Sets the request timeout in milliseconds.
        /// </summary>
        public void SetTimeoutMs(int milliseconds)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.TimeoutMs, milliseconds));
        }

        /// <summary>
        /// Sets SSL verification mode.
        /// </summary>
        public void SetSslVerifyPeer(bool verify)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslVerifyPeer, verify ? 1L : 0L));
        }

        /// <summary>
        /// Sets SSL host verification mode.
        /// </summary>
        public void SetSslVerifyHost(bool verify)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslVerifyHost, verify ? 2L : 0L));
        }

        /// <summary>
        /// Sets the CA certificate file path.
        /// </summary>
        public void SetCaInfo(string path)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Cainfo, path));
        }

        /// <summary>
        /// Sets the proxy URL.
        /// </summary>
        public void SetProxy(string proxyUrl)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Proxy, proxyUrl));
        }

        /// <summary>
        /// Sets proxy authentication.
        /// </summary>
        public void SetProxyUserPassword(string userPassword)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.ProxyUserpwd, userPassword));
        }

        /// <summary>
        /// Sets a cookie header.
        /// </summary>
        public void SetCookie(string cookie)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Cookie, cookie));
        }

        /// <summary>
        /// Adds a header to the request.
        /// </summary>
        public void AddHeader(string header)
        {
            ThrowIfDisposed();
            _headers ??= new CurlSlistHandle();
            _headers.Append(header);
        }

        /// <summary>
        /// Sets the request body.
        /// </summary>
        public void SetRequestBody(byte[] data)
        {
            ThrowIfDisposed();

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _pinnedHandles.Add(handle);

            CheckResult(LibCurl.SetOpt(_handle, CurlOption.PostFields, handle.AddrOfPinnedObject()));
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.PostFieldSize, data.Length));
        }

        /// <summary>
        /// Sets the HTTP version.
        /// </summary>
        public void SetHttpVersion(CurlHttpVersion version)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.HttpVersion, (long)version));
        }

        /// <summary>
        /// Sets accept encoding (compression).
        /// </summary>
        public void SetAcceptEncoding(string encoding)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.AcceptEncoding, encoding));
        }

        // =================================================================
        // curl-impersonate specific options
        // =================================================================

        /// <summary>
        /// Use curl-impersonate to impersonate a specific browser.
        /// </summary>
        public void Impersonate(string target, bool defaultHeaders = true)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.curl_easy_impersonate(_handle, target, defaultHeaders ? 1 : 0));
        }

        /// <summary>
        /// Sets TLS cipher list.
        /// </summary>
        public void SetCiphers(string ciphers)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslCipherList, ciphers));
        }

        /// <summary>
        /// Sets elliptic curves.
        /// </summary>
        public void SetCurves(string curves)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslEcCurves, curves));
        }

        /// <summary>
        /// Sets HTTP/2 settings.
        /// </summary>
        public void SetHttp2Settings(string settings)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Http2Settings, settings));
        }

        /// <summary>
        /// Sets HTTP/2 window update value.
        /// </summary>
        public void SetHttp2WindowUpdate(int value)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Http2WindowUpdate, value));
        }

        /// <summary>
        /// Sets HTTP/2 stream weight.
        /// </summary>
        public void SetHttp2StreamWeight(int weight)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.StreamWeight, weight));
        }

        /// <summary>
        /// Sets HTTP/2 stream exclusive flag.
        /// </summary>
        public void SetHttp2StreamExclusive(bool exclusive)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.StreamExclusive, exclusive ? 1L : 0L));
        }

        /// <summary>
        /// Sets HTTP/2 pseudo headers order.
        /// </summary>
        public void SetHttp2PseudoHeadersOrder(string order)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.Http2PseudoHeadersOrder, order));
        }

        /// <summary>
        /// Enables/disables ALPS.
        /// </summary>
        public void SetEnableAlps(bool enable)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslEnableAlps, enable ? 1L : 0L));
        }

        /// <summary>
        /// Enables/disables TLS extension permutation.
        /// </summary>
        public void SetPermuteExtensions(bool enable)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslPermuteExtensions, enable ? 1L : 0L));
        }

        /// <summary>
        /// Sets certificate compression algorithm.
        /// </summary>
        public void SetCertCompression(string algorithm)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslCertCompression, algorithm));
        }

        /// <summary>
        /// Enables/disables TLS GREASE.
        /// </summary>
        public void SetTlsGrease(bool enable)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.TlsGrease, enable ? 1L : 0L));
        }

        /// <summary>
        /// Enables/disables TLS signed certificate timestamps.
        /// </summary>
        public void SetTlsSignedCertTimestamps(bool enable)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.TlsSignedCertTimestamps, enable ? 1L : 0L));
        }

        /// <summary>
        /// Sets signature hash algorithms.
        /// </summary>
        public void SetSignatureHashes(string hashes)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SslSigHashAlgs, hashes));
        }

        /// <summary>
        /// Sets delegated credentials.
        /// </summary>
        public void SetDelegatedCredentials(string credentials)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.TlsDelegatedCredentials, credentials));
        }

        /// <summary>
        /// Sets TLS record size limit.
        /// </summary>
        public void SetTlsRecordSizeLimit(int limit)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.TlsRecordSizeLimit, limit));
        }

        /// <summary>
        /// Sets TLS extension order.
        /// </summary>
        public void SetTlsExtensionOrder(string order)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.TlsExtensionOrder, order));
        }

        /// <summary>
        /// Enables/disables cookie splitting.
        /// </summary>
        public void SetSplitCookies(bool enable)
        {
            ThrowIfDisposed();
            CheckResult(LibCurl.SetOpt(_handle, CurlOption.SplitCookies, enable ? 1L : 0L));
        }

        // =================================================================
        // Perform and response
        // =================================================================

        /// <summary>
        /// Performs the HTTP request and returns the response.
        /// </summary>
        /// <param name="responseStream">
        /// When supplied, the response body is written directly to this stream as it
        /// arrives and <see cref="CurlResponse.Body"/> is returned empty. When null,
        /// the body is buffered in memory.
        /// </param>
        public CurlResponse Perform(Stream? responseStream = null)
        {
            ThrowIfDisposed();

            // Set up response collectors
            _responseStream = responseStream;
            _responseBody = responseStream == null ? new MemoryStream() : null;
            _responseHeaders = new StringBuilder();

            // Set up callbacks
            SetupWriteCallback();
            SetupHeaderCallback();

            // Apply headers if any
            if (_headers != null && !_headers.IsInvalid)
            {
                CheckResult(LibCurl.SetOpt(_handle, CurlOption.HttpHeader, _headers.DangerousGetHandle()));
            }

            // Perform the request
            CurlCode result = LibCurl.curl_easy_perform(_handle);

            // Get response code
            LibCurl.GetInfo(_handle, CurlInfo.ResponseCode, out long responseCode);
            ResponseCode = (int)responseCode;

            if (result != CurlCode.Ok)
            {
                string errorMessage = LibCurl.GetErrorString(result);
                throw new CurlNativeException(result, errorMessage);
            }

            return new CurlResponse
            {
                StatusCode = ResponseCode,
                Body = _responseBody?.ToArray() ?? [],
                RawHeaders = _responseHeaders.ToString()
            };
        }

        /// <summary>
        /// Resets the handle for reuse.
        /// </summary>
        public void Reset()
        {
            ThrowIfDisposed();

            // Free pinned handles
            foreach (GCHandle handle in _pinnedHandles)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            _pinnedHandles.Clear();

            // Free headers
            _headers?.Dispose();
            _headers = null;

            // Reset callbacks
            _writeCallback = null;
            _headerCallback = null;
            _responseBody = null;
            _responseStream = null;
            _responseHeaders = null;

            // Reset the curl handle
            LibCurl.curl_easy_reset(_handle);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (GCHandle handle in _pinnedHandles)
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            _pinnedHandles.Clear();

            _headers?.Dispose();
            _handle.Dispose();
        }

        private static void CheckResult(CurlCode code)
        {
            if (code != CurlCode.Ok)
            {
                string errorMessage = LibCurl.GetErrorString(code);
                throw new CurlNativeException(code, errorMessage);
            }
        }

        private static void EnsureGlobalInit()
        {
            if (_globalInitialized)
            {
                return;
            }

            lock (_globalInitLock)
            {
                if (_globalInitialized)
                {
                    return;
                }

                // Ensure the native library resolver is registered first
                NativeLibraryResolver.EnsureInitialized();

                CurlCode result = LibCurl.curl_global_init(LibCurl.CURL_GLOBAL_DEFAULT);
                if (result != CurlCode.Ok)
                {
                    throw new InvalidOperationException($"Failed to initialize curl globally: {result}");
                }

                _globalInitialized = true;
            }
        }

        private void SetupHeaderCallback()
        {
            _headerCallback = (ptr, size, nmemb, userdata) =>
            {
                int length = (int)(size * nmemb);
                if (length > 0 && _responseHeaders != null)
                {
                    byte[] buffer = new byte[length];
                    Marshal.Copy(ptr, buffer, 0, length);
                    _responseHeaders.Append(Encoding.UTF8.GetString(buffer));
                }
                return (nuint)length;
            };

            CheckResult(LibCurl.SetOpt(_handle, CurlOption.HeaderFunction, _headerCallback));
        }

        private void SetupWriteCallback()
        {
            _writeCallback = (ptr, size, nmemb, userdata) =>
            {
                int length = (int)(size * nmemb);
                if (length > 0)
                {
                    Stream? sink = _responseStream ?? _responseBody;
                    if (sink != null)
                    {
                        byte[] buffer = new byte[length];
                        Marshal.Copy(ptr, buffer, 0, length);
                        try
                        {
                            sink.Write(buffer, 0, length);
                        }
                        catch
                        {
                            // Returning a short count aborts the transfer with CURLE_WRITE_ERROR.
                            return 0;
                        }
                    }
                }
                return (nuint)length;
            };

            CheckResult(LibCurl.SetOpt(_handle, CurlOption.WriteFunction, _writeCallback));
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }

    /// <summary>
    /// Response from a curl request.
    /// </summary>
    internal sealed class CurlResponse
    {
        public required byte[] Body { get; init; }
        public required string RawHeaders { get; init; }
        public required int StatusCode { get; init; }
    }

    /// <summary>
    /// Exception thrown when a native curl operation fails.
    /// </summary>
    internal sealed class CurlNativeException : Exception
    {
        public CurlNativeException(CurlCode code, string message)
            : base($"Curl error {code}: {message}")
        {
            Code = code;
        }

        public CurlCode Code { get; }
    }
}
