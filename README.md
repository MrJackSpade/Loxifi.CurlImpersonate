# Loxifi.CurlImpersonate

A .NET wrapper around [curl-impersonate](https://github.com/lexiforest/curl-impersonate) that provides browser fingerprint impersonation for HTTP requests. This library implements an `HttpClient`-like API for seamless integration with existing .NET code while providing anti-fingerprinting capabilities.

## What is curl-impersonate?

[curl-impersonate](https://github.com/lexiforest/curl-impersonate) is a special build of curl that can impersonate the four major browsers: Chrome, Firefox, Safari, and Edge. Unlike standard HTTP clients, it mimics the TLS fingerprint, HTTP/2 settings, and headers of real browsers, making it much harder for servers to detect that requests are coming from an automated tool rather than a real browser.

## Installation

```bash
dotnet add package Loxifi.CurlImpersonate
```

Or via the NuGet Package Manager:

```
Install-Package Loxifi.CurlImpersonate
```

## Quick Start

```csharp
using Loxifi.CurlImpersonate;

// Create a client that impersonates Chrome 131
using var client = new CurlClient(BrowserProfile.Chrome131);

// Make requests just like HttpClient
HttpResponseMessage response = await client.GetAsync("https://example.com");
string content = await response.Content.ReadAsStringAsync();
```

## Features

- **HttpClient-compatible API** - Familiar methods like `GetAsync`, `PostAsync`, `SendAsync`
- **31 Browser Profiles** - Chrome, Firefox, Safari, Edge, and Tor profiles
- **Cookie Support** - Automatic cookie handling with `CookieContainer`
- **Redirect Control** - Follow redirects automatically or inspect 3xx responses directly
- **Proxy Support** - HTTP/HTTPS proxy with authentication
- **SSL Certificate Handling** - Includes CA certificate bundle for production use
- **Auto-updating Profiles** - Code generator tool to pull latest curl-impersonate releases

## Usage Examples

### Basic GET Request

```csharp
using var client = new CurlClient(BrowserProfile.Chrome131);

string html = await client.GetStringAsync("https://example.com");
```

### POST Request with JSON

```csharp
using var client = new CurlClient(BrowserProfile.Firefox135);

var content = new StringContent(
    "{\"username\":\"test\",\"password\":\"secret\"}",
    Encoding.UTF8,
    "application/json"
);

HttpResponseMessage response = await client.PostAsync("https://api.example.com/login", content);
```

### Inspecting Redirect Responses

Redirects are followed automatically by default. Disable automatic redirects to inspect the
original status code and `Location` header:

```csharp
using var client = new CurlClient(BrowserProfile.Chrome131)
{
    AllowAutoRedirect = false
};

HttpResponseMessage response = await client.GetAsync("https://example.com/redirect");

if (response.StatusCode == HttpStatusCode.Found)
{
    Uri? destination = response.Headers.Location;
}
```

### Custom Headers

```csharp
using var client = new CurlClient(BrowserProfile.Safari180);

// Add default headers sent with every request
client.DefaultRequestHeaders["Authorization"] = "Bearer your-token";
client.DefaultRequestHeaders["X-Custom-Header"] = "CustomValue";

var response = await client.GetAsync("https://api.example.com/data");
```

### Using Cookies

```csharp
using var client = new CurlClient(BrowserProfile.Chrome131);

// Add cookies manually
client.CookieContainer.Add(
    new Uri("https://example.com"),
    new Cookie("session", "abc123")
);

// Cookies from responses are automatically stored
await client.GetAsync("https://example.com/login");

// Subsequent requests include stored cookies
await client.GetAsync("https://example.com/dashboard");

// Disable cookie handling if needed
client.UseCookies = false;
```

### Using a Proxy

```csharp
using var client = new CurlClient(BrowserProfile.Chrome131);

// Simple proxy
client.Proxy = new WebProxy("http://proxy.example.com:8080");

// Proxy with authentication
client.Proxy = new WebProxy("http://proxy.example.com:8080")
{
    Credentials = new NetworkCredential("username", "password")
};

var response = await client.GetAsync("https://example.com");
```

### Base Address

```csharp
using var client = new CurlClient(BrowserProfile.Chrome131);
client.BaseAddress = new Uri("https://api.example.com");

// Relative URLs are combined with BaseAddress
var users = await client.GetStringAsync("/users");
var posts = await client.GetStringAsync("/posts");
```

### Timeout Configuration

```csharp
using var client = new CurlClient(BrowserProfile.Chrome131);
client.Timeout = TimeSpan.FromSeconds(60);
```

### Insecure Mode (Skip SSL Verification)

```csharp
// WARNING: Only use for testing with self-signed certificates
using var client = new CurlClient(BrowserProfile.Chrome131);
client.Insecure = true;
```

## Available Browser Profiles

Generated from curl-impersonate v2.3.2. Each profile applies that browser's exact TLS ClientHello, HTTP/2 settings and default headers.

### Chrome
- `Chrome99`, `Chrome99Android`, `Chrome100`, `Chrome101`, `Chrome104`, `Chrome107`
- `Chrome110`, `Chrome116`, `Chrome119`, `Chrome120`, `Chrome123`, `Chrome124`
- `Chrome131`, `Chrome131Android`, `Chrome133a`, `Chrome136`, `Chrome142`, `Chrome145`, `Chrome146`
- `Chrome150`, `Chrome151`, `Chrome151Macos`, `Chrome152`, `Chrome152Macos`

### Firefox
- `Firefox133`, `Firefox135`, `Firefox144`, `Firefox147`, `Firefox153`, `Firefox154`, `Firefox155`

### Safari
- `Safari153`, `Safari155`, `Safari170`, `Safari172Ios`, `Safari180`, `Safari180Ios`
- `Safari184`, `Safari184Ios`, `Safari260`, `Safari260Ios`, `Safari270`, `Safari270Ios`

### Edge
- `Edge99`, `Edge101`

### Tor
- `Tor145`

Headers set through `DefaultRequestHeaders` or on the request replace the browser's default header of the same name in place, so the browser's header order is kept. Use this to match a specific browser build, e.g. a newer Edge: `User-Agent`, `sec-ch-ua`, `sec-ch-ua-platform`.

## API Reference

### CurlClient

| Property | Type | Description |
|----------|------|-------------|
| `BaseAddress` | `Uri?` | Base URI for relative requests |
| `AllowAutoRedirect` | `bool` | Follow redirects automatically (default: true) |
| `Timeout` | `TimeSpan` | Request timeout (default: 100 seconds; `TimeSpan.Zero` disables it) |
| `DefaultRequestHeaders` | `IDictionary<string, string>` | Headers sent with every request |
| `CookieContainer` | `CookieContainer` | Cookie storage for the client |
| `UseCookies` | `bool` | Enable/disable cookie handling (default: true) |
| `Proxy` | `WebProxy?` | Proxy configuration |
| `Insecure` | `bool` | Skip SSL certificate verification (default: false) |

| Method | Description |
|--------|-------------|
| `GetAsync(string/Uri)` | Send GET request |
| `PostAsync(string/Uri, HttpContent)` | Send POST request |
| `PutAsync(string/Uri, HttpContent)` | Send PUT request |
| `DeleteAsync(string/Uri)` | Send DELETE request |
| `PatchAsync(string/Uri, HttpContent)` | Send PATCH request |
| `SendAsync(HttpRequestMessage)` | Send custom request |
| `GetStringAsync(string/Uri)` | GET and return string |
| `GetByteArrayAsync(string/Uri)` | GET and return byte array |
| `GetStreamAsync(string/Uri)` | GET and return stream |

## Updating Browser Profiles

The code generator pulls a curl-impersonate release from GitHub, updates the bundled Windows library, and regenerates the profile code.

```bash
# Windows
Update.bat

# Or manually (latest release, or pin one with --version)
dotnet run --project src/Loxifi.CurlImpersonate.CodeGen
dotnet run --project src/Loxifi.CurlImpersonate.CodeGen -- --version v2.3.2
```

This will:
1. Download the release's `libcurl-impersonate` (DLL) and `curl-impersonate` (CLI) Windows x64 archives
2. Extract `libcurl-impersonate.dll` into `runtimes/win-x64/native` and download a fresh `cacert.pem`
3. Read the supported impersonation targets from the CLI's `curl_<target>.bat` file names
4. Generate `BrowserProfile.cs` and `BrowserProfileTargets.cs`

The Linux library is built separately with `build/linux/build-shim.sh` on a Linux machine (see `build/linux/README.md`); it builds the version recorded in the generated `BrowserProfile.cs`.

## Releasing

`.github/workflows/publish.yml` tests every push and PR on Windows and Linux. Pushing a version tag publishes it to nuget.org through NuGet Trusted Publishing (no stored API key):

```bash
# after bumping <Version> in src/Loxifi.CurlImpersonate/Loxifi.CurlImpersonate.csproj
git tag v2.0.0
git push origin v2.0.0
```

The tag must match `<Version>` or the publish job fails. One-time setup on nuget.org: *Account > Trusted Publishing*, policy for owner `MrJackSpade`, repository `Loxifi.CurlImpersonate`, workflow file `publish.yml`.

## Platform Support

- **Windows x64** (`win-x64`)
- **Linux x64** (`linux-x64`)

The native libraries are included in the NuGet package and automatically copied to the output directory.

## How It Works

1. **Browser Profile Selection**: Each `BrowserProfile` maps to a curl-impersonate target name (e.g. `chrome146`).

2. **Native Impersonation**: Every request calls libcurl-impersonate's `curl_easy_impersonate` with that target, which applies the browser's TLS fingerprint, HTTP/2 settings and default headers natively. Your headers are then layered on top.

3. **Response Parsing**: The curl output is parsed and converted to standard `HttpResponseMessage` objects.

4. **Cookie Management**: Cookies are automatically extracted from responses and sent with subsequent requests.

## Credits

This project is a .NET wrapper around the excellent [curl-impersonate](https://github.com/lexiforest/curl-impersonate) project by [lexiforest](https://github.com/lexiforest).

curl-impersonate is a special build of curl that performs TLS and HTTP handshakes identical to real browsers. It is based on:
- [curl](https://github.com/curl/curl) - The underlying HTTP client
- Modified TLS libraries (BoringSSL/NSS) to mimic browser TLS fingerprints

## License

This project is licensed under the MIT License.

curl-impersonate is licensed under the [MIT License](https://github.com/lexiforest/curl-impersonate/blob/main/LICENSE).

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## See Also

- [curl-impersonate GitHub](https://github.com/lexiforest/curl-impersonate)
- [TLS Fingerprinting](https://engineering.salesforce.com/tls-fingerprinting-with-ja3-and-ja3s-247362855967/)
- [HTTP/2 Fingerprinting](https://github.com/AliasIO/Wappalyzer)
