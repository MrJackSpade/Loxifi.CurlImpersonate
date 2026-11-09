using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Loxifi.CurlImpersonate.Tests
{
    /// <summary>
    /// Guards the impersonation itself: every profile maps to a target the native library accepts,
    /// the resulting fingerprint is the real browser's, and caller headers override the browser's defaults.
    /// </summary>
    public class ImpersonationTests
    {
        public static IEnumerable<object[]> AllProfiles()
        {
            return Enum.GetValues<BrowserProfile>().Select(p => new object[] { p });
        }

        [Theory]
        [MemberData(nameof(AllProfiles))]
        public async Task EveryProfile_TargetIsAcceptedByNativeLibrary(BrowserProfile profile)
        {
            using LoopbackServer server = new();
            using CurlClient client = new(profile);

            HttpResponseMessage response = await client.GetAsync(server.Url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrEmpty((await server.NextRequest)["User-Agent"]),
                $"{profile} sent no User-Agent; the browser's default headers were not applied");
        }

        [Fact]
        public async Task CallerHeader_ReplacesBrowserDefault_WithoutDuplicating()
        {
            using LoopbackServer server = new();
            using CurlClient client = new(BrowserProfile.Chrome146);
            client.DefaultRequestHeaders["User-Agent"] = "Override/1.0";

            await client.GetAsync(server.Url);

            WebHeaderCollection headers = await server.NextRequest;
            string[]? userAgents = headers.GetValues("User-Agent");
            Assert.NotNull(userAgents);
            Assert.Equal(["Override/1.0"], userAgents);
        }

        [Fact]
        public async Task ChromeProfile_ProducesChromeFingerprint()
        {
            using CurlClient client = new(BrowserProfile.Chrome146);

            using JsonDocument doc = JsonDocument.Parse(await client.GetStringAsync("https://tls.peet.ws/api/all"));
            JsonElement root = doc.RootElement;

            // JA4 "t13d1516h2_8daaf6152771_...": TLS 1.3, 15 ciphers, 16 extensions, h2 ALPN, Chrome's cipher hash.
            Assert.StartsWith("t13d1516h2_8daaf6152771_", root.GetProperty("tls").GetProperty("ja4").GetString());
            Assert.Equal("1:65536;2:0;4:6291456;6:262144|15663105|0|m,a,s,p",
                root.GetProperty("http2").GetProperty("akamai_fingerprint").GetString());

            string[] sent = root.GetProperty("http2").GetProperty("sent_frames").EnumerateArray()
                .Where(f => f.TryGetProperty("headers", out _))
                .SelectMany(f => f.GetProperty("headers").EnumerateArray().Select(h => h.GetString()!))
                .ToArray();
            Assert.Contains(sent, h => h.StartsWith("sec-ch-ua: \"Chromium\";v=\"146\"", StringComparison.Ordinal));
            Assert.Contains(sent, h => h.StartsWith("user-agent: Mozilla/5.0", StringComparison.Ordinal) && h.Contains("Chrome/146"));
        }

        /// <summary>Single-request HTTP server on 127.0.0.1 that records the request headers.</summary>
        private sealed class LoopbackServer : IDisposable
        {
            private readonly HttpListener _listener = new();

            public LoopbackServer()
            {
                int port = FreePort();
                Url = $"http://127.0.0.1:{port}/";
                _listener.Prefixes.Add(Url);
                _listener.Start();
                NextRequest = ServeOneAsync();
            }

            public string Url { get; }

            public Task<WebHeaderCollection> NextRequest { get; }

            public void Dispose()
            {
                _listener.Close();
            }

            private async Task<WebHeaderCollection> ServeOneAsync()
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                WebHeaderCollection headers = new();
                headers.Add(context.Request.Headers);
                context.Response.StatusCode = 200;
                context.Response.Close();
                return headers;
            }

            private static int FreePort()
            {
                using TcpListener probe = new(IPAddress.Loopback, 0);
                probe.Start();
                return ((IPEndPoint)probe.LocalEndpoint).Port;
            }
        }
    }
}
