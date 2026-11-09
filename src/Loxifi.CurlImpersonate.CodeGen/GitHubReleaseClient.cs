using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Loxifi.CurlImpersonate.CodeGen
{
    internal sealed class GitHubRelease
    {
        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = new();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Client for fetching release information from GitHub.
    /// </summary>
    internal sealed class GitHubReleaseClient : IDisposable
    {
        private const string RepoName = "curl-impersonate";

        private const string RepoOwner = "lexiforest";

        private readonly HttpClient _httpClient;

        public GitHubReleaseClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Loxifi.CurlImpersonate.CodeGen");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<Stream> DownloadAssetAsync(string downloadUrl, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        public Task<GitHubRelease> GetLatestReleaseAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync("releases/latest", cancellationToken);
        }

        public Task<GitHubRelease> GetReleaseAsync(string tag, CancellationToken cancellationToken = default)
        {
            return GetAsync($"releases/tags/{Uri.EscapeDataString(tag)}", cancellationToken);
        }

        private async Task<GitHubRelease> GetAsync(string path, CancellationToken cancellationToken)
        {
            string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/{path}";
            return await _httpClient.GetFromJsonAsync<GitHubRelease>(url, cancellationToken)
                ?? throw new InvalidOperationException($"Empty response from {url}");
        }
    }

    internal sealed class GitHubAsset
    {
        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}