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

Install the required build tools in your Linux environment (WSL or native Linux):

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install -y build-essential curl zlib1g-dev

# Fedora/RHEL
sudo dnf install -y gcc make curl zlib-devel

# Arch Linux
sudo pacman -S base-devel curl zlib
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

### What the Build Script Does

1. Downloads the pre-built static `libcurl-impersonate.a` from the [curl-impersonate releases](https://github.com/lexiforest/curl-impersonate/releases)
2. Creates a C shim that wraps the hidden symbols with exported wrappers
3. Links everything into a shared library `libcurl-impersonate.so`
4. Copies the resulting library to `runtimes/linux-x64/native/` (or `linux-arm64` on ARM)

## Output

After a successful build, you'll find:

- `runtimes/linux-x64/native/libcurl-impersonate.so` - The shared library for x64 Linux
- `runtimes/linux-x64/native/cacert.pem` - CA certificates (copied from Windows runtime if available)

## Running Tests

### Prerequisites for Testing

To run .NET tests in WSL, install the .NET SDK:

```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0
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

### "Cannot find -lz"

Install zlib development package:
```bash
sudo apt install zlib1g-dev
```

### "gcc: command not found"

Install build tools:
```bash
sudo apt install build-essential
```

### Library loads but symbols not found

Ensure the library was built correctly by checking exported symbols:
```bash
nm -D runtimes/linux-x64/native/libcurl-impersonate.so | grep curl
```

You should see symbols like:
- `curl_easy_init`
- `curl_easy_perform`
- `curl_easy_setopt_long`
- `curl_easy_setopt_str`
- `curl_easy_impersonate`
