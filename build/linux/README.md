# Linux Build Environment Setup

This directory contains scripts for building the Linux native library shim for curl-impersonate.

## Prerequisites

### WSL (Windows Subsystem for Linux)

If building from Windows, you need WSL installed with a Linux distribution:

```powershell
# Install WSL (run as Administrator)
wsl --install

# Or install a specific distribution
wsl --install -d Ubuntu
```

### Build Dependencies

The build needs gcc, binutils (`nm`, `readelf`), `ldd` and curl:

```bash
# Ubuntu/Debian
sudo apt install -y build-essential curl

# Fedora/RHEL
sudo dnf install -y gcc binutils curl

# Arch Linux
sudo pacman -S base-devel curl
```

## Building the Library

### From Windows (via WSL)

```bash
# From the repository root
wsl bash build/linux/build-shim.sh
```

### From Linux

```bash
# From the repository root
bash build/linux/build-shim.sh
```

To build from an archive you already downloaded (e.g. on a machine whose network is slow), pass it in:

```bash
CURL_IMPERSONATE_ARCHIVE=/path/to/libcurl-impersonate-v2.3.2.x86_64-linux-gnu.tar.gz bash build/linux/build-shim.sh
```

### What the Build Script Does

1. Gets `libcurl-impersonate-v<version>.<arch>.tar.gz` from the [curl-impersonate releases](https://github.com/lexiforest/curl-impersonate/releases) (the version recorded in the generated `BrowserProfile.cs`, or `CURL_IMPERSONATE_VERSION`)
2. Checks that upstream's shared library `libcurl-impersonate.so.4` exports every function the C# side calls
3. Builds a thin shim, `libcurl-impersonate.so`, exporting fixed-signature wrappers for curl's variadic functions (`curl_easy_setopt_long`, `curl_easy_getinfo_str`, ...), which P/Invoke cannot call portably on Linux. The shim depends on `libcurl-impersonate.so.4` with RUNPATH `$ORIGIN`; every other function resolves from upstream's library directly
4. Verifies the shim (dependency, RUNPATH, exports, and that it references only the real variadic functions, so no wrapper can call itself) and that it loads with no unresolved dependencies
5. Installs both libraries and `cacert.pem` to `runtimes/linux-x64/native/` (or `linux-arm64` on ARM)

## Output

- `runtimes/linux-x64/native/libcurl-impersonate.so` - the shim the .NET library loads
- `runtimes/linux-x64/native/libcurl-impersonate.so.4` - upstream curl-impersonate, loaded by the shim
- `runtimes/linux-x64/native/cacert.pem` - CA certificates (copied from the Windows runtime)

## Running Tests

### Prerequisites for Testing

To run .NET tests in WSL, install the .NET SDK:

```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-10.0
```

### Running Tests

```bash
# From the repository root in WSL
cd /mnt/c/path/to/Loxifi.CurlImpersonate
dotnet test
```

## Architecture Support

- **x86_64 (x64)**: Fully supported
- **aarch64 (ARM64)**: Supported (build script auto-detects architecture)

## Troubleshooting

### "gcc: command not found"

Install build tools:
```bash
sudo apt install build-essential
```

### Library loads but symbols not found

Check the pair together - the shim exports the wrappers, `libcurl-impersonate.so.4` exports the rest:
```bash
nm -D --defined-only runtimes/linux-x64/native/libcurl-impersonate.so runtimes/linux-x64/native/libcurl-impersonate.so.4 | grep -E " T curl_(easy_init|easy_perform|easy_impersonate|easy_setopt_long|easy_setopt_str)$"
ldd runtimes/linux-x64/native/libcurl-impersonate.so   # libcurl-impersonate.so.4 must resolve next to it
```
