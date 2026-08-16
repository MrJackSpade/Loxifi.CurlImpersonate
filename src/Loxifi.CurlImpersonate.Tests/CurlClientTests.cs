using System.Net;
using System.Text.Json;

namespace Loxifi.CurlImpersonate.Tests
{
    public class CurlClientTests
    {
        private static CurlClient CreateClient(BrowserProfile profile = BrowserProfile.Chrome131, bool insecure = false)
        {
            CurlClient client = new(profile);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.Insecure = insecure;
            return client;
        }

        #region Production SSL Tests (using CA bundle)

        [Theory]
        [InlineData(BrowserProfile.Chrome99)]
        [InlineData(BrowserProfile.Chrome131)]
        [InlineData(BrowserProfile.Firefox133)]
        [InlineData(BrowserProfile.Safari180)]
        [InlineData(BrowserProfile.Edge101)]
        public async Task GetAsync_DifferentProfiles_WithCaCert_AllWork(BrowserProfile profile)
        {
            using CurlClient client = CreateClient(profile);

            HttpResponseMessage response = await client.GetAsync("https://httpbin.org/get");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAsync_WithCaCert_ReturnsSuccessResponse()
        {
            using CurlClient client = CreateClient();

            HttpResponseMessage response = await client.GetAsync("https://httpbin.org/get");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetStringAsync_WithCaCert_ReturnsContent()
        {
            using CurlClient client = CreateClient();

            string content = await client.GetStringAsync("https://httpbin.org/get");

            Assert.NotEmpty(content);
            Assert.Contains("httpbin.org", content);
        }

        [Fact]
        public async Task PostAsync_WithCaCert_SendsRequestBody()
        {
            using CurlClient client = CreateClient();

            StringContent content = new("{\"test\":\"value\"}", System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("https://httpbin.org/post", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"test\"", responseContent);
            Assert.Contains("\"value\"", responseContent);
        }

        #endregion Production SSL Tests (using CA bundle)

        #region Browser Fingerprint Tests

        [Fact]
        public async Task GetAsync_WithChrome131_SendsCorrectUserAgent()
        {
            using CurlClient client = CreateClient(BrowserProfile.Chrome131);

            string content = await client.GetStringAsync("https://httpbin.org/headers");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement headers = json.RootElement.GetProperty("headers");
            string? userAgent = headers.GetProperty("User-Agent").GetString();

            Assert.NotNull(userAgent);
            Assert.Contains("Chrome/131", userAgent);
        }

        [Fact]
        public async Task GetAsync_WithFirefox_SendsFirefoxUserAgent()
        {
            using CurlClient client = CreateClient(BrowserProfile.Firefox135);

            string content = await client.GetStringAsync("https://httpbin.org/headers");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement headers = json.RootElement.GetProperty("headers");
            string? userAgent = headers.GetProperty("User-Agent").GetString();

            Assert.NotNull(userAgent);
            Assert.Contains("Firefox", userAgent);
        }

        [Fact]
        public async Task GetAsync_WithSafari_SendsSafariUserAgent()
        {
            using CurlClient client = CreateClient(BrowserProfile.Safari180);

            string content = await client.GetStringAsync("https://httpbin.org/headers");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement headers = json.RootElement.GetProperty("headers");
            string? userAgent = headers.GetProperty("User-Agent").GetString();

            Assert.NotNull(userAgent);
            Assert.Contains("Safari", userAgent);
        }

        #endregion Browser Fingerprint Tests

        #region HTTP Method Tests

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess()
        {
            using CurlClient client = CreateClient();

            HttpResponseMessage response = await client.DeleteAsync("https://httpbin.org/delete");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAsync_WithBaseAddress_CombinesUrls()
        {
            using CurlClient client = CreateClient();
            client.BaseAddress = new Uri("https://httpbin.org");

            HttpResponseMessage response = await client.GetAsync("/get");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAsync_WithCustomHeader_SendsHeader()
        {
            using CurlClient client = CreateClient();
            client.DefaultRequestHeaders["X-Custom-Header"] = "TestValue";

            string content = await client.GetStringAsync("https://httpbin.org/headers");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement headers = json.RootElement.GetProperty("headers");
            string? customHeader = headers.GetProperty("X-Custom-Header").GetString();

            Assert.Equal("TestValue", customHeader);
        }

        [Fact]
        public async Task GetByteArrayAsync_ReturnsByteArray()
        {
            using CurlClient client = CreateClient();

            byte[] bytes = await client.GetByteArrayAsync("https://httpbin.org/bytes/100");

            Assert.Equal(100, bytes.Length);
        }

        [Fact]
        public async Task PatchAsync_ReturnsSuccess()
        {
            using CurlClient client = CreateClient();

            StringContent content = new("test data");
            HttpResponseMessage response = await client.PatchAsync("https://httpbin.org/patch", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PutAsync_ReturnsSuccess()
        {
            using CurlClient client = CreateClient();

            StringContent content = new("test data");
            HttpResponseMessage response = await client.PutAsync("https://httpbin.org/put", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion HTTP Method Tests

        #region Redirect Tests

        [Fact]
        public async Task GetAsync_DefaultBehavior_FollowsRedirect()
        {
            using CurlClient client = CreateClient();

            HttpResponseMessage response = await client.GetAsync("https://httpbin.org/redirect-to?url=/get");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAsync_AutoRedirectDisabled_ReturnsRedirectResponse()
        {
            using CurlClient client = CreateClient();
            client.AllowAutoRedirect = false;

            HttpResponseMessage response = await client.GetAsync("https://httpbin.org/redirect-to?url=/get");

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/get", response.Headers.Location?.OriginalString);
        }

        #endregion Redirect Tests

        #region Insecure Mode Tests

        [Fact]
        public async Task GetAsync_InsecureMode_ReturnsSuccessResponse()
        {
            using CurlClient client = CreateClient(insecure: true);

            HttpResponseMessage response = await client.GetAsync("https://httpbin.org/get");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion Insecure Mode Tests

        #region Cookie Tests

        [Fact]
        public async Task Cookies_AreSentWithRequest()
        {
            using CurlClient client = CreateClient();
            Uri uri = new("https://httpbin.org");

            // Add a cookie to the container
            client.CookieContainer.Add(uri, new Cookie("TestCookie", "TestValue"));

            string content = await client.GetStringAsync("https://httpbin.org/cookies");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement cookies = json.RootElement.GetProperty("cookies");

            Assert.True(cookies.TryGetProperty("TestCookie", out JsonElement cookieValue));
            Assert.Equal("TestValue", cookieValue.GetString());
        }

        [Fact]
        public async Task Cookies_AreStoredFromResponse()
        {
            using CurlClient client = CreateClient();
            Uri uri = new("https://httpbin.org");

            // Request that sets a cookie
            await client.GetAsync("https://httpbin.org/cookies/set/ResponseCookie/ResponseValue");

            // Check that the cookie was stored
            CookieCollection cookies = client.CookieContainer.GetCookies(uri);
            Cookie? cookie = cookies["ResponseCookie"];

            Assert.NotNull(cookie);
            Assert.Equal("ResponseValue", cookie.Value);
        }

        [Fact]
        public async Task Cookies_CanBeDisabled()
        {
            using CurlClient client = CreateClient();
            Uri uri = new("https://httpbin.org");

            // Add a cookie but disable cookie handling
            client.CookieContainer.Add(uri, new Cookie("DisabledCookie", "DisabledValue"));
            client.UseCookies = false;

            string content = await client.GetStringAsync("https://httpbin.org/cookies");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement cookies = json.RootElement.GetProperty("cookies");

            // Cookie should not be sent
            Assert.False(cookies.TryGetProperty("DisabledCookie", out _));
        }

        [Fact]
        public async Task Cookies_MultipleCookiesAreSent()
        {
            using CurlClient client = CreateClient();
            Uri uri = new("https://httpbin.org");

            // Add multiple cookies
            client.CookieContainer.Add(uri, new Cookie("Cookie1", "Value1"));
            client.CookieContainer.Add(uri, new Cookie("Cookie2", "Value2"));

            string content = await client.GetStringAsync("https://httpbin.org/cookies");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement cookies = json.RootElement.GetProperty("cookies");

            Assert.True(cookies.TryGetProperty("Cookie1", out JsonElement cookie1));
            Assert.Equal("Value1", cookie1.GetString());
            Assert.True(cookies.TryGetProperty("Cookie2", out JsonElement cookie2));
            Assert.Equal("Value2", cookie2.GetString());
        }

        [Fact]
        public async Task Cookies_PersistAcrossRequests()
        {
            using CurlClient client = CreateClient();

            // First request sets a cookie
            await client.GetAsync("https://httpbin.org/cookies/set/PersistentCookie/PersistentValue");

            // Second request should send the cookie
            string content = await client.GetStringAsync("https://httpbin.org/cookies");
            JsonDocument json = JsonDocument.Parse(content);
            JsonElement cookies = json.RootElement.GetProperty("cookies");

            Assert.True(cookies.TryGetProperty("PersistentCookie", out JsonElement cookieValue));
            Assert.Equal("PersistentValue", cookieValue.GetString());
        }

        #endregion Cookie Tests

        #region Proxy Tests

        [Fact]
        public void Proxy_CanBeSet()
        {
            using CurlClient client = CreateClient();

            WebProxy proxy = new("http://proxy.example.com:8080");
            client.Proxy = proxy;

            Assert.Equal(proxy, client.Proxy);
        }

        [Fact]
        public void Proxy_WithCredentials_CanBeSet()
        {
            using CurlClient client = CreateClient();

            WebProxy proxy = new("http://proxy.example.com:8080")
            {
                Credentials = new NetworkCredential("user", "password")
            };
            client.Proxy = proxy;

            Assert.NotNull(client.Proxy);
            Assert.NotNull(client.Proxy.Credentials);
        }

        // Note: Actual proxy tests require a working proxy server
        // These tests verify the proxy configuration is properly stored

        #endregion Proxy Tests

        #region Profile Enumeration Tests

        [Fact]
        public void BrowserProfile_HasChromeProfiles()
        {
            string[] profiles = Enum.GetNames<BrowserProfile>();
            List<string> chromeProfiles = profiles.Where(p => p.StartsWith("Chrome")).ToList();

            Assert.True(chromeProfiles.Count >= 10, "Should have at least 10 Chrome profiles");
        }

        [Fact]
        public void BrowserProfile_HasExpectedProfiles()
        {
            string[] profiles = Enum.GetNames<BrowserProfile>();

            Assert.Contains("Chrome131", profiles);
            Assert.Contains("Firefox135", profiles);
            Assert.Contains("Safari180", profiles);
            Assert.Contains("Edge101", profiles);
            Assert.Contains("Tor145", profiles);
            Assert.True(profiles.Length >= 30, "Should have at least 30 browser profiles");
        }

        [Fact]
        public void BrowserProfile_HasFirefoxProfiles()
        {
            string[] profiles = Enum.GetNames<BrowserProfile>();
            List<string> firefoxProfiles = profiles.Where(p => p.StartsWith("Firefox")).ToList();

            Assert.True(firefoxProfiles.Count >= 2, "Should have at least 2 Firefox profiles");
        }

        [Fact]
        public void BrowserProfile_HasSafariProfiles()
        {
            string[] profiles = Enum.GetNames<BrowserProfile>();
            List<string> safariProfiles = profiles.Where(p => p.StartsWith("Safari")).ToList();

            Assert.True(safariProfiles.Count >= 5, "Should have at least 5 Safari profiles");
        }

        #endregion Profile Enumeration Tests
    }
}
