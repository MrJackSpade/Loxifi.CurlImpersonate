using System.Text.RegularExpressions;

namespace Loxifi.CurlImpersonate.CodeGen
{
    /// <summary>
    /// Parses curl-impersonate .bat files to extract browser profile settings.
    /// </summary>
    internal sealed partial class BatFileParser
    {
        /// <summary>
        /// Parses a .bat file and extracts the profile configuration.
        /// </summary>
        public ParsedProfile Parse(string batFilePath)
        {
            string content = File.ReadAllText(batFilePath);
            string fileName = Path.GetFileNameWithoutExtension(batFilePath);
            string profileName = ConvertToProfileName(fileName);

            ParsedProfile profile = new()
            {
                Name = profileName,
                FileName = fileName
            };

            // Extract ciphers
            Match ciphersMatch = CiphersRegex().Match(content);
            if (ciphersMatch.Success)
            {
                profile.Ciphers = ciphersMatch.Groups[1].Value;
            }

            // Extract curves
            Match curvesMatch = CurvesRegex().Match(content);
            if (curvesMatch.Success)
            {
                profile.Curves = curvesMatch.Groups[1].Value;
            }

            // Extract HTTP/2 settings
            Match http2SettingsMatch = Http2SettingsRegex().Match(content);
            if (http2SettingsMatch.Success)
            {
                profile.Http2Settings = http2SettingsMatch.Groups[1].Value;
            }

            // Extract HTTP/2 window update
            Match windowUpdateMatch = Http2WindowUpdateRegex().Match(content);
            if (windowUpdateMatch.Success)
            {
                profile.Http2WindowUpdate = int.Parse(windowUpdateMatch.Groups[1].Value);
            }

            // Extract HTTP/2 stream weight
            Match streamWeightMatch = Http2StreamWeightRegex().Match(content);
            if (streamWeightMatch.Success)
            {
                profile.Http2StreamWeight = int.Parse(streamWeightMatch.Groups[1].Value);
            }

            // Extract HTTP/2 stream exclusive
            profile.Http2StreamExclusive = content.Contains("--http2-stream-exclusive");

            // Extract compression
            profile.UseCompression = content.Contains("--compressed");

            // Extract ECH mode
            Match echMatch = EchRegex().Match(content);
            if (echMatch.Success)
            {
                profile.EchMode = echMatch.Groups[1].Value;
            }

            // Extract TLS version
            Match tlsVersionMatch = TlsVersionRegex().Match(content);
            if (tlsVersionMatch.Success)
            {
                profile.TlsVersion = tlsVersionMatch.Groups[1].Value;
            }

            // Extract ALPS
            profile.UseAlps = content.Contains("--alps");

            // Extract TLS permute extensions
            profile.UsePermuteTlsExtensions = content.Contains("--tls-permute-extensions");

            // Extract cert compression
            Match certCompressionMatch = CertCompressionRegex().Match(content);
            if (certCompressionMatch.Success)
            {
                profile.CertCompression = certCompressionMatch.Groups[1].Value;
            }

            // Extract TLS grease
            profile.UseTlsGrease = content.Contains("--tls-grease");

            // Extract TLS signed cert timestamps
            profile.UseTlsSignedCertTimestamps = content.Contains("--tls-signed-cert-ts");

            // Extract split cookies (can be --split-cookies or --http2-split-cookies)
            profile.SplitCookies = content.Contains("--split-cookies");

            // Extract HTTP/2
            profile.UseHttp2 = content.Contains("--http2");

            // Extract HTTP/2 pseudo headers order
            Match pseudoHeadersMatch = Http2PseudoHeadersOrderRegex().Match(content);
            if (pseudoHeadersMatch.Success)
            {
                profile.Http2PseudoHeadersOrder = pseudoHeadersMatch.Groups[1].Value;
            }

            // Extract HTTP/2 no server push
            profile.Http2NoServerPush = content.Contains("--http2-no-server-push");

            // Extract signature hashes
            Match signatureHashesMatch = SignatureHashesRegex().Match(content);
            if (signatureHashesMatch.Success)
            {
                profile.SignatureHashes = signatureHashesMatch.Groups[1].Value;
            }

            // Extract delegated credentials
            Match delegatedCredentialsMatch = DelegatedCredentialsRegex().Match(content);
            if (delegatedCredentialsMatch.Success)
            {
                profile.DelegatedCredentials = delegatedCredentialsMatch.Groups[1].Value;
            }

            // Extract TLS record size limit
            Match recordSizeLimitMatch = TlsRecordSizeLimitRegex().Match(content);
            if (recordSizeLimitMatch.Success)
            {
                profile.TlsRecordSizeLimit = int.Parse(recordSizeLimitMatch.Groups[1].Value);
            }

            // Extract TLS supported versions
            Match supportedVersionsMatch = TlsSupportedVersionsRegex().Match(content);
            if (supportedVersionsMatch.Success)
            {
                profile.TlsSupportedVersions = supportedVersionsMatch.Groups[1].Value;
            }

            // Extract TLS extension order
            Match extensionOrderMatch = TlsExtensionOrderRegex().Match(content);
            if (extensionOrderMatch.Success)
            {
                profile.TlsExtensionOrder = extensionOrderMatch.Groups[1].Value;
            }

            // Extract headers
            MatchCollection headerMatches = HeaderRegex().Matches(content);
            foreach (Match match in headerMatches)
            {
                string headerValue = match.Groups[1].Value;
                int colonIndex = headerValue.IndexOf(':');
                if (colonIndex > 0)
                {
                    string name = headerValue[..colonIndex].Trim();
                    string value = headerValue[(colonIndex + 1)..].Trim();
                    profile.Headers[name] = value;
                }
            }

            return profile;
        }

        [GeneratedRegex(@"--cert-compression\s+(\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex CertCompressionRegex();

        [GeneratedRegex(@"--ciphers\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex CiphersRegex();

        private static string ConvertToProfileName(string fileName)
        {
            // Remove 'curl_' prefix
            string name = fileName;
            if (name.StartsWith("curl_", StringComparison.OrdinalIgnoreCase))
            {
                name = name[5..];
            }

            // Convert to PascalCase
            string[] parts = name.Split('_');
            string result = string.Concat(parts.Select(p =>
            {
                if (string.IsNullOrEmpty(p))
                {
                    return p;
                }

                return char.ToUpper(p[0]) + p[1..];
            }));

            return result;
        }

        [GeneratedRegex(@"--curves\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex CurvesRegex();

        [GeneratedRegex(@"--delegated-credentials\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex DelegatedCredentialsRegex();

        [GeneratedRegex(@"--ech\s+(\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex EchRegex();

        [GeneratedRegex(@"-H\s+""([^""]+)""", RegexOptions.IgnoreCase)]
        private static partial Regex HeaderRegex();

        [GeneratedRegex(@"--http2-pseudo-headers-order\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex Http2PseudoHeadersOrderRegex();

        [GeneratedRegex(@"--http2-settings\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex Http2SettingsRegex();

        [GeneratedRegex(@"--http2-stream-weight\s+(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex Http2StreamWeightRegex();

        [GeneratedRegex(@"--http2-window-update\s+(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex Http2WindowUpdateRegex();

        [GeneratedRegex(@"--signature-hashes\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex SignatureHashesRegex();

        [GeneratedRegex(@"--tls-extension-order\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex TlsExtensionOrderRegex();

        [GeneratedRegex(@"--tls-record-size-limit\s+(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex TlsRecordSizeLimitRegex();

        [GeneratedRegex(@"--tls-supported-versions\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase)]
        private static partial Regex TlsSupportedVersionsRegex();

        [GeneratedRegex(@"--tlsv(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)]
        private static partial Regex TlsVersionRegex();
    }

    internal sealed class ParsedProfile
    {
        public string? CertCompression { get; set; }

        public string? Ciphers { get; set; }

        public string? Curves { get; set; }

        public string? DelegatedCredentials { get; set; }

        public string? EchMode { get; set; }

        public string FileName { get; set; } = string.Empty;

        public Dictionary<string, string> Headers { get; } = new();

        public bool Http2NoServerPush { get; set; }

        public string? Http2PseudoHeadersOrder { get; set; }

        public string? Http2Settings { get; set; }

        public bool Http2StreamExclusive { get; set; }

        public int? Http2StreamWeight { get; set; }

        public int? Http2WindowUpdate { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? SignatureHashes { get; set; }

        public bool SplitCookies { get; set; }

        public string? TlsExtensionOrder { get; set; }

        public int? TlsRecordSizeLimit { get; set; }

        public string? TlsSupportedVersions { get; set; }

        public string? TlsVersion { get; set; }

        public bool UseAlps { get; set; }

        public bool UseCompression { get; set; }

        public bool UseHttp2 { get; set; }

        public bool UsePermuteTlsExtensions { get; set; }

        public bool UseTlsGrease { get; set; }

        public bool UseTlsSignedCertTimestamps { get; set; }
    }
}