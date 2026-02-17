using Loxifi.CurlImpersonate.Exceptions;
using System.Diagnostics;
using System.Text;

namespace Loxifi.CurlImpersonate
{
    /// <summary>
    /// Handles execution of the curl-impersonate process.
    /// </summary>
    internal sealed class CurlProcess : IDisposable
    {
        private readonly string? _caCertPath;

        private readonly string _curlPath;

        private bool _disposed;

        public CurlProcess()
        {
            _curlPath = FindCurlExecutable();
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

        public async Task<CurlResult> ExecuteAsync(
            CurlRequest request,
            CancellationToken cancellationToken = default)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "curl-impersonate", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string bodyOutputFile = Path.Combine(tempDir, "body");
            string headerOutputFile = Path.Combine(tempDir, "headers");
            string? requestBodyFile = null;

            try
            {
                StringBuilder arguments = new();

                // Output files
                arguments.Append($"-o \"{bodyOutputFile}\" ");
                arguments.Append($"-D \"{headerOutputFile}\" ");

                // Method
                arguments.Append($"-X {request.Method} ");

                // URL
                arguments.Append($"\"{request.Url}\" ");

                // Profile-specific settings
                AppendProfileSettings(arguments, request.Profile);

                // Headers (user headers override profile headers)
                foreach (KeyValuePair<string, string> header in request.Headers)
                {
                    arguments.Append($"-H \"{EscapeHeaderValue(header.Key)}: {EscapeHeaderValue(header.Value)}\" ");
                }

                // Request body
                if (request.Body != null && request.Body.Length > 0)
                {
                    requestBodyFile = Path.Combine(tempDir, "request_body");
                    await File.WriteAllBytesAsync(requestBodyFile, request.Body, cancellationToken);
                    arguments.Append($"--data-binary \"@{requestBodyFile}\" ");
                }

                // Silent mode (no progress)
                arguments.Append("-s ");

                // Follow redirects (like HttpClient default)
                arguments.Append("-L ");

                // Timeout
                if (request.Timeout.TotalSeconds > 0)
                {
                    arguments.Append($"--max-time {(int)request.Timeout.TotalSeconds} ");
                }

                // SSL certificate handling
                if (request.Insecure)
                {
                    arguments.Append("-k ");
                }
                else if (_caCertPath != null)
                {
                    arguments.Append($"--cacert \"{_caCertPath}\" ");
                }

                // Cookie handling
                if (!string.IsNullOrEmpty(request.CookieHeader))
                {
                    arguments.Append($"--cookie \"{EscapeHeaderValue(request.CookieHeader)}\" ");
                }

                // Proxy handling
                if (!string.IsNullOrEmpty(request.ProxyUrl))
                {
                    arguments.Append($"--proxy \"{request.ProxyUrl}\" ");

                    if (!string.IsNullOrEmpty(request.ProxyUserPassword))
                    {
                        arguments.Append($"--proxy-user \"{EscapeHeaderValue(request.ProxyUserPassword)}\" ");
                    }
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = _curlPath,
                    Arguments = arguments.ToString().Trim(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_curlPath) ?? Environment.CurrentDirectory
                };

                using Process process = new() { StartInfo = startInfo };

                StringBuilder stderrBuilder = new();
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        stderrBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    throw new CurlException(
                        $"curl exited with code {process.ExitCode}",
                        process.ExitCode,
                        stderrBuilder.ToString());
                }

                // Read response
                byte[] responseBody = File.Exists(bodyOutputFile)
                    ? await File.ReadAllBytesAsync(bodyOutputFile, cancellationToken)
                    : Array.Empty<byte>();

                string responseHeaders = File.Exists(headerOutputFile)
                    ? await File.ReadAllTextAsync(headerOutputFile, cancellationToken)
                    : string.Empty;

                return new CurlResult
                {
                    Body = responseBody,
                    RawHeaders = responseHeaders
                };
            }
            finally
            {
                // Cleanup temp files
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, recursive: true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        private static void AppendProfileSettings(StringBuilder arguments, ProfileConfig profile)
        {
            if (!string.IsNullOrEmpty(profile.Ciphers))
            {
                arguments.Append($"--ciphers \"{profile.Ciphers}\" ");
            }

            if (!string.IsNullOrEmpty(profile.Curves))
            {
                arguments.Append($"--curves \"{profile.Curves}\" ");
            }

            if (!string.IsNullOrEmpty(profile.Http2Settings))
            {
                arguments.Append($"--http2-settings \"{profile.Http2Settings}\" ");
            }

            if (profile.Http2WindowUpdate.HasValue)
            {
                arguments.Append($"--http2-window-update {profile.Http2WindowUpdate.Value} ");
            }

            if (profile.Http2StreamWeight.HasValue)
            {
                arguments.Append($"--http2-stream-weight {profile.Http2StreamWeight.Value} ");
            }

            if (profile.Http2StreamExclusive)
            {
                arguments.Append("--http2-stream-exclusive 1 ");
            }

            if (profile.UseCompression)
            {
                arguments.Append("--compressed ");
            }

            if (!string.IsNullOrEmpty(profile.EchMode))
            {
                arguments.Append($"--ech {profile.EchMode} ");
            }

            if (!string.IsNullOrEmpty(profile.TlsVersion))
            {
                arguments.Append($"--tlsv{profile.TlsVersion} ");
            }

            if (profile.UseAlps)
            {
                arguments.Append("--alps ");
            }

            if (profile.UsePermuteTlsExtensions)
            {
                arguments.Append("--tls-permute-extensions ");
            }

            if (!string.IsNullOrEmpty(profile.CertCompression))
            {
                arguments.Append($"--cert-compression {profile.CertCompression} ");
            }

            if (profile.UseTlsGrease)
            {
                arguments.Append("--tls-grease ");
            }

            if (profile.UseTlsSignedCertTimestamps)
            {
                arguments.Append("--tls-signed-cert-ts ");
            }

            if (profile.SplitCookies)
            {
                arguments.Append("--split-cookies ");
            }

            if (profile.UseHttp2)
            {
                arguments.Append("--http2 ");
            }

            if (!string.IsNullOrEmpty(profile.Http2PseudoHeadersOrder))
            {
                arguments.Append($"--http2-pseudo-headers-order \"{profile.Http2PseudoHeadersOrder}\" ");
            }

            if (profile.Http2NoServerPush)
            {
                arguments.Append("--http2-no-server-push ");
            }

            if (!string.IsNullOrEmpty(profile.SignatureHashes))
            {
                arguments.Append($"--signature-hashes \"{profile.SignatureHashes}\" ");
            }

            if (!string.IsNullOrEmpty(profile.DelegatedCredentials))
            {
                arguments.Append($"--delegated-credentials \"{profile.DelegatedCredentials}\" ");
            }

            if (profile.TlsRecordSizeLimit.HasValue)
            {
                arguments.Append($"--tls-record-size-limit {profile.TlsRecordSizeLimit.Value} ");
            }

            if (!string.IsNullOrEmpty(profile.TlsSupportedVersions))
            {
                arguments.Append($"--tls-supported-versions \"{profile.TlsSupportedVersions}\" ");
            }

            if (!string.IsNullOrEmpty(profile.TlsExtensionOrder))
            {
                arguments.Append($"--tls-extension-order \"{profile.TlsExtensionOrder}\" ");
            }

            // Add profile headers
            foreach (KeyValuePair<string, string> header in profile.Headers)
            {
                arguments.Append($"-H \"{EscapeHeaderValue(header.Key)}: {EscapeHeaderValue(header.Value)}\" ");
            }
        }

        private static string EscapeHeaderValue(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string? FindCaCertificate()
        {
            string assemblyLocation = typeof(CurlProcess).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;

            // Check common locations for the CA certificate bundle
            string[] possiblePaths = new[]
            {
                Path.Combine(assemblyDir, "runtimes", "win-x64", "native", "cacert.pem"),
                Path.Combine(assemblyDir, "cacert.pem"),
                Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", "cacert.pem"),
                Path.Combine(AppContext.BaseDirectory, "cacert.pem"),
            };

            foreach (string? path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static string FindCurlExecutable()
        {
            // First check in the runtimes folder relative to the assembly
            string assemblyLocation = typeof(CurlProcess).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;

            // Check common locations for the curl executable
            string[] possiblePaths = new[]
            {
                Path.Combine(assemblyDir, "runtimes", "win-x64", "native", "curl.exe"),
                Path.Combine(assemblyDir, "curl.exe"),
                Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native", "curl.exe"),
                Path.Combine(AppContext.BaseDirectory, "curl.exe"),
            };

            foreach (string? path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Fall back to PATH
            return "curl.exe";
        }
    }

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

    internal sealed class CurlResult
    {
        public required byte[] Body { get; init; }

        public required string RawHeaders { get; init; }
    }
}