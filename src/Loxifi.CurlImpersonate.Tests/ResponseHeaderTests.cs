using System.Net;
using System.Net.Sockets;

namespace Loxifi.CurlImpersonate.Tests
{
    /// <summary>
    /// Every response header must reach the caller exactly once, in the collection .NET puts it in:
    /// content headers (Last-Modified, Expires, ...) on Content.Headers, the rest on Headers.
    /// </summary>
    public class ResponseHeaderTests
    {
        private static readonly DateTimeOffset LastModified = new(2026, 10, 27, 17, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset Expires = new(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ContentAndResponseHeaders_ArriveOnceInTheRightCollection(bool streamed)
        {
            int port = FreePort();
            using HttpListener listener = new();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();
            Task serve = ServeOneAsync(listener);

            using CurlClient client = new(BrowserProfile.Chrome146);
            HttpResponseMessage response;
            using MemoryStream body = new();
            if (streamed)
            {
                response = await client.DownloadAsync($"http://127.0.0.1:{port}/", body);
            }
            else
            {
                response = await client.GetAsync($"http://127.0.0.1:{port}/");
            }

            await serve;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(LastModified, response.Content.Headers.LastModified);
            Assert.Equal(Expires, response.Content.Headers.Expires);
            Assert.Equal(["de-DE"], response.Content.Headers.ContentLanguage);
            Assert.Equal(["custom-value"], response.Headers.GetValues("X-Custom"));
        }

        private static async Task ServeOneAsync(HttpListener listener)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            context.Response.StatusCode = 200;
            context.Response.Headers["Last-Modified"] = LastModified.ToString("R");
            context.Response.Headers["Expires"] = Expires.ToString("R");
            context.Response.Headers["Content-Language"] = "de-DE";
            context.Response.Headers["X-Custom"] = "custom-value";
            byte[] payload = "hello"u8.ToArray();
            await context.Response.OutputStream.WriteAsync(payload);
            context.Response.Close();
        }

        private static int FreePort()
        {
            using TcpListener probe = new(IPAddress.Loopback, 0);
            probe.Start();
            return ((IPEndPoint)probe.LocalEndpoint).Port;
        }
    }
}
