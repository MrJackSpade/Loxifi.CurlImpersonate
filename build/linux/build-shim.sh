#!/bin/bash
set -e

# Build script to create a shared library shim from the static libcurl-impersonate.a
# The static library has HIDDEN visibility on symbols, so we create wrapper functions
# that re-export them with default visibility for P/Invoke to work.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUNTIMES_DIR="$SCRIPT_DIR/../../runtimes"

# Detect architecture
ARCH=$(uname -m)
case "$ARCH" in
    x86_64)
        RID="linux-x64"
        DOWNLOAD_ARCH="x86_64-linux-gnu"
        ;;
    aarch64)
        RID="linux-arm64"
        DOWNLOAD_ARCH="aarch64-linux-gnu"
        ;;
    *)
        echo "Unsupported architecture: $ARCH"
        exit 1
        ;;
esac

NATIVE_DIR="$RUNTIMES_DIR/$RID/native"
VERSION="1.5.0"
DOWNLOAD_URL="https://github.com/lexiforest/curl-impersonate/releases/download/v${VERSION}/libcurl-impersonate-v${VERSION}.${DOWNLOAD_ARCH}.tar.gz"

echo "Building libcurl-impersonate shim for $RID..."

# Create temp directory
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

cd "$TEMP_DIR"

# Download static library
echo "Downloading static library from $DOWNLOAD_URL..."
curl -sL "$DOWNLOAD_URL" -o lib.tar.gz
tar -xzf lib.tar.gz

# Check what we have
if [ ! -f "libcurl-impersonate.a" ]; then
    echo "Error: libcurl-impersonate.a not found in archive"
    ls -la
    exit 1
fi

echo "Static library size: $(du -h libcurl-impersonate.a | cut -f1)"

# Create a C shim that wraps the hidden symbols and exports them with default visibility
# We use unique internal names (_hidden_*) to avoid symbol conflicts
echo "Creating shim.c with wrapper functions..."
cat > shim.c << 'SHIMEOF'
// Shim to re-export libcurl-impersonate symbols with default visibility
// The static library has HIDDEN visibility, so we create wrapper functions
// We reference the hidden symbols with internal names to avoid conflicts

#include <stddef.h>
#include <stdint.h>

// Opaque types
typedef void CURL;
typedef void CURLM;
typedef void CURLSH;
typedef int CURLcode;
typedef int CURLMcode;
typedef int CURLoption;
typedef int CURLINFO;
typedef int CURLMoption;

struct curl_slist {
    char *data;
    struct curl_slist *next;
};

// Callback types
typedef size_t (*curl_write_callback)(char *ptr, size_t size, size_t nmemb, void *userdata);
typedef size_t (*curl_read_callback)(char *buffer, size_t size, size_t nitems, void *userdata);

// Export macro for visibility
#define EXPORT __attribute__((visibility("default")))

// Declare the hidden functions from the static library using internal asm names
// The static library exports symbols with HIDDEN visibility
extern CURL *_hidden_curl_easy_init(void) __asm__("curl_easy_init");
extern CURLcode _hidden_curl_easy_setopt(CURL *curl, CURLoption option, ...) __asm__("curl_easy_setopt");
extern CURLcode _hidden_curl_easy_perform(CURL *curl) __asm__("curl_easy_perform");
extern void _hidden_curl_easy_cleanup(CURL *curl) __asm__("curl_easy_cleanup");
extern CURLcode _hidden_curl_easy_getinfo(CURL *curl, CURLINFO info, ...) __asm__("curl_easy_getinfo");
extern CURL *_hidden_curl_easy_duphandle(CURL *curl) __asm__("curl_easy_duphandle");
extern void _hidden_curl_easy_reset(CURL *curl) __asm__("curl_easy_reset");
extern const char *_hidden_curl_easy_strerror(CURLcode code) __asm__("curl_easy_strerror");
extern CURLcode _hidden_curl_easy_impersonate(CURL *curl, const char *target, int default_headers) __asm__("curl_easy_impersonate");

extern struct curl_slist *_hidden_curl_slist_append(struct curl_slist *list, const char *string) __asm__("curl_slist_append");
extern void _hidden_curl_slist_free_all(struct curl_slist *list) __asm__("curl_slist_free_all");

extern CURLcode _hidden_curl_global_init(long flags) __asm__("curl_global_init");
extern void _hidden_curl_global_cleanup(void) __asm__("curl_global_cleanup");
extern char *_hidden_curl_version(void) __asm__("curl_version");

extern CURLM *_hidden_curl_multi_init(void) __asm__("curl_multi_init");
extern CURLMcode _hidden_curl_multi_cleanup(CURLM *multi_handle) __asm__("curl_multi_cleanup");
extern CURLMcode _hidden_curl_multi_add_handle(CURLM *multi_handle, CURL *curl_handle) __asm__("curl_multi_add_handle");
extern CURLMcode _hidden_curl_multi_remove_handle(CURLM *multi_handle, CURL *curl_handle) __asm__("curl_multi_remove_handle");
extern CURLMcode _hidden_curl_multi_perform(CURLM *multi_handle, int *running_handles) __asm__("curl_multi_perform");
extern CURLMcode _hidden_curl_multi_setopt(CURLM *multi_handle, CURLMoption option, ...) __asm__("curl_multi_setopt");

// Wrapper functions with default visibility and explicit exported names
EXPORT CURL *curl_easy_init(void) {
    return _hidden_curl_easy_init();
}

EXPORT CURLcode curl_easy_perform(CURL *curl) {
    return _hidden_curl_easy_perform(curl);
}

EXPORT void curl_easy_cleanup(CURL *curl) {
    _hidden_curl_easy_cleanup(curl);
}

EXPORT void curl_easy_reset(CURL *curl) {
    _hidden_curl_easy_reset(curl);
}

EXPORT CURL *curl_easy_duphandle(CURL *curl) {
    return _hidden_curl_easy_duphandle(curl);
}

EXPORT const char *curl_easy_strerror(CURLcode code) {
    return _hidden_curl_easy_strerror(code);
}

EXPORT CURLcode curl_easy_impersonate(CURL *curl, const char *target, int default_headers) {
    return _hidden_curl_easy_impersonate(curl, target, default_headers);
}

EXPORT struct curl_slist *curl_slist_append(struct curl_slist *list, const char *string) {
    return _hidden_curl_slist_append(list, string);
}

EXPORT void curl_slist_free_all(struct curl_slist *list) {
    _hidden_curl_slist_free_all(list);
}

EXPORT CURLcode curl_global_init(long flags) {
    return _hidden_curl_global_init(flags);
}

EXPORT void curl_global_cleanup(void) {
    _hidden_curl_global_cleanup();
}

EXPORT char *curl_version(void) {
    return _hidden_curl_version();
}

EXPORT CURLM *curl_multi_init(void) {
    return _hidden_curl_multi_init();
}

EXPORT CURLMcode curl_multi_cleanup(CURLM *multi_handle) {
    return _hidden_curl_multi_cleanup(multi_handle);
}

EXPORT CURLMcode curl_multi_add_handle(CURLM *multi_handle, CURL *curl_handle) {
    return _hidden_curl_multi_add_handle(multi_handle, curl_handle);
}

EXPORT CURLMcode curl_multi_remove_handle(CURLM *multi_handle, CURL *curl_handle) {
    return _hidden_curl_multi_remove_handle(multi_handle, curl_handle);
}

EXPORT CURLMcode curl_multi_perform(CURLM *multi_handle, int *running_handles) {
    return _hidden_curl_multi_perform(multi_handle, running_handles);
}

// Typed variants for setopt (variadic functions can't be easily wrapped)
EXPORT CURLcode curl_easy_setopt_long(CURL *curl, CURLoption option, long value) {
    return _hidden_curl_easy_setopt(curl, option, value);
}

EXPORT CURLcode curl_easy_setopt_ptr(CURL *curl, CURLoption option, void *value) {
    return _hidden_curl_easy_setopt(curl, option, value);
}

EXPORT CURLcode curl_easy_setopt_str(CURL *curl, CURLoption option, const char *value) {
    return _hidden_curl_easy_setopt(curl, option, value);
}

EXPORT CURLcode curl_easy_setopt_cb(CURL *curl, CURLoption option, void *callback) {
    return _hidden_curl_easy_setopt(curl, option, callback);
}

// Typed variants for getinfo
EXPORT CURLcode curl_easy_getinfo_long(CURL *curl, CURLINFO info, long *value) {
    return _hidden_curl_easy_getinfo(curl, info, value);
}

EXPORT CURLcode curl_easy_getinfo_str(CURL *curl, CURLINFO info, char **value) {
    return _hidden_curl_easy_getinfo(curl, info, value);
}

EXPORT CURLcode curl_easy_getinfo_double(CURL *curl, CURLINFO info, double *value) {
    return _hidden_curl_easy_getinfo(curl, info, value);
}

// Typed variant for multi_setopt
EXPORT CURLMcode curl_multi_setopt_long(CURLM *multi, CURLMoption option, long value) {
    return _hidden_curl_multi_setopt(multi, option, value);
}
SHIMEOF

# Create version script to only export our symbols (hide internal ones)
cat > exports.map << 'EOF'
{
    global:
        curl_easy_init;
        curl_easy_perform;
        curl_easy_cleanup;
        curl_easy_reset;
        curl_easy_duphandle;
        curl_easy_strerror;
        curl_easy_impersonate;
        curl_slist_append;
        curl_slist_free_all;
        curl_global_init;
        curl_global_cleanup;
        curl_version;
        curl_multi_init;
        curl_multi_cleanup;
        curl_multi_add_handle;
        curl_multi_remove_handle;
        curl_multi_perform;
        curl_easy_setopt_long;
        curl_easy_setopt_ptr;
        curl_easy_setopt_str;
        curl_easy_setopt_cb;
        curl_easy_getinfo_long;
        curl_easy_getinfo_str;
        curl_easy_getinfo_double;
        curl_multi_setopt_long;
    local:
        *;
};
EOF

echo "Compiling shim..."
gcc -c -fPIC -fvisibility=hidden -o shim.o shim.c

echo "Creating shared library..."
# Link shim first, then static library, allowing our wrappers to take precedence
# Use -z muldefs to allow multiple definitions (our wrappers will be used)
gcc -shared -fPIC -o libcurl-impersonate.so \
    -Wl,-z,muldefs \
    -Wl,--version-script=exports.map \
    shim.o \
    -Wl,--whole-archive libcurl-impersonate.a -Wl,--no-whole-archive \
    -lpthread -ldl -lm -lz 2>&1 || {
    echo "Build failed!"
    exit 1
}

# Verify the shared library
echo "Verifying shared library..."
if ! file libcurl-impersonate.so | grep -q "shared object"; then
    echo "Error: Failed to create shared library"
    exit 1
fi

# Check for required symbols
echo "Checking for required symbols..."
REQUIRED_SYMBOLS="curl_easy_init curl_easy_perform curl_easy_cleanup curl_easy_impersonate curl_slist_append curl_slist_free_all curl_easy_setopt_long curl_easy_setopt_str curl_easy_setopt_ptr curl_easy_getinfo_long"
MISSING=0
for sym in $REQUIRED_SYMBOLS; do
    if nm -D libcurl-impersonate.so 2>/dev/null | grep -q " T $sym\$"; then
        echo "  ✓ $sym"
    elif nm -D libcurl-impersonate.so 2>/dev/null | grep -q " T $sym"; then
        echo "  ✓ $sym (found)"
    else
        echo "  ✗ $sym (missing)"
        MISSING=1
    fi
done

if [ $MISSING -eq 1 ]; then
    echo ""
    echo "Debug: All exported symbols containing 'curl':"
    nm -D libcurl-impersonate.so 2>/dev/null | grep -i curl | head -30
    echo ""
    echo "Build failed - required symbols not exported"
    exit 1
fi

# Copy to runtimes directory
echo "Installing to $NATIVE_DIR..."
mkdir -p "$NATIVE_DIR"
cp libcurl-impersonate.so "$NATIVE_DIR/"

# Also copy cacert.pem if available
if [ -f "$RUNTIMES_DIR/win-x64/native/cacert.pem" ]; then
    cp "$RUNTIMES_DIR/win-x64/native/cacert.pem" "$NATIVE_DIR/"
    echo "Copied cacert.pem to $NATIVE_DIR/"
fi

echo ""
echo "Done! Shared library installed to $NATIVE_DIR/libcurl-impersonate.so"
echo "Size: $(du -h "$NATIVE_DIR/libcurl-impersonate.so" | cut -f1)"
echo ""
echo "Exported symbols:"
nm -D "$NATIVE_DIR/libcurl-impersonate.so" 2>/dev/null | grep " T curl" | head -20
