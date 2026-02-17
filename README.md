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

### Chrome
- `Chrome99`, `Chrome99Android`
- `Chrome100`, `Chrome101`, `Chrome104`, `Chrome107`
- `Chrome110`, `Chrome116`, `Chrome119`, `Chrome120`
- `Chrome123`, `Chrome124`, `Chrome131`, `Chrome131Android`
- `Chrome133a`, `Chrome136`

### Firefox
- `Firefox133`, `Firefox135`

### Safari
- `Safari153`, `Safari155`, `Safari170`
- `Safari172iOS`, `Safari180`, `Safari180iOS`
- `Safari184`, `Safari184iOS`, `Safari260`, `Safari260iOS`

### Edge
- `Edge99`, `Edge101`

### Tor
- `Tor145`

## API Reference

### CurlClient

| Property | Type | Description |
|----------|------|-------------|
| `BaseAddress` | `Uri?` | Base URI for relative requests |
| `Timeout` | `TimeSpan` | Request timeout (default: 100 seconds) |
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

This library includes a code generator tool that automatically pulls the latest curl-impersonate release from GitHub and regenerates the browser profile code.

To update to the latest profiles:

```bash
# Windows
Update.bat

# Or manually
dotnet run --project src/Loxifi.CurlImpersonate.CodeGen
```

This will:
1. Download the latest curl-impersonate release
2. Extract the native binaries and bat files
3. Parse the bat files to extract browser fingerprint settings
4. Generate updated `BrowserProfile.cs` and `BrowserProfileSettings.cs`

## Platform Support

Currently supports:
- **Windows x64** (`win-x64`)

The native curl-impersonate binaries are included in the NuGet package and automatically copied to the output directory.

## How It Works

1. **Browser Profile Selection**: Each profile contains the exact TLS ciphers, curves, HTTP/2 settings, and headers used by that browser version.

2. **curl-impersonate Execution**: Requests are executed via the bundled curl-impersonate binary with the appropriate flags.

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
