#!/bin/bash
set -euo pipefail

# Builds the Linux native runtime for Loxifi.CurlImpersonate.
#
# Since curl-impersonate v2, the release ships a real shared library (libcurl-impersonate.so.4) that
# exports curl's whole API with default visibility. We ship it as-is, plus a thin shim
# (libcurl-impersonate.so) whose only job is to give .NET fixed-signature entry points for curl's
# VARIADIC functions (curl_easy_setopt / curl_easy_getinfo / curl_multi_setopt), which P/Invoke cannot
# call portably on Linux. Every other function is resolved straight from libcurl-impersonate.so.4: the
# shim lists it as NEEDED with RUNPATH $ORIGIN, and dlsym on the shim's handle searches its dependencies.
#
# The wrappers use names that do not exist in libcurl (curl_easy_setopt_long, ...), so a wrapper can never
# resolve to itself.
#
# Inputs:
#   CURL_IMPERSONATE_VERSION  version to build (default: the one recorded in the generated BrowserProfile.cs)
#   CURL_IMPERSONATE_ARCHIVE  path to an already-downloaded libcurl-impersonate-v<ver>.<arch>.tar.gz
#                             (default: download it from the GitHub release)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUNTIMES_DIR="$SCRIPT_DIR/../../runtimes"

ARCH=$(uname -m)
case "$ARCH" in
    x86_64)  RID="linux-x64";   DOWNLOAD_ARCH="x86_64-linux-gnu" ;;
    aarch64) RID="linux-arm64"; DOWNLOAD_ARCH="aarch64-linux-gnu" ;;
    *) echo "ERROR: unsupported architecture: $ARCH"; exit 1 ;;
esac
NATIVE_DIR="$RUNTIMES_DIR/$RID/native"

GENERATED_HEADER="$SCRIPT_DIR/../../src/Loxifi.CurlImpersonate/BrowserProfile.cs"
VERSION="${CURL_IMPERSONATE_VERSION:-$(grep -oiE 'curl-impersonate v[0-9]+\.[0-9]+\.[0-9]+' "$GENERATED_HEADER" | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+')}"
if [ -z "$VERSION" ]; then
    echo "ERROR: could not determine curl-impersonate version (set CURL_IMPERSONATE_VERSION)"
    exit 1
fi
ARCHIVE_NAME="libcurl-impersonate-v${VERSION}.${DOWNLOAD_ARCH}.tar.gz"
echo "Building Linux runtime for $RID from curl-impersonate v$VERSION"

TEMP_DIR=$(mktemp -d)
trap 'rm -rf "$TEMP_DIR"' EXIT
cd "$TEMP_DIR"

if [ -n "${CURL_IMPERSONATE_ARCHIVE:-}" ]; then
    echo "Using archive: $CURL_IMPERSONATE_ARCHIVE"
    cp "$CURL_IMPERSONATE_ARCHIVE" lib.tar.gz
else
    URL="https://github.com/lexiforest/curl-impersonate/releases/download/v${VERSION}/${ARCHIVE_NAME}"
    echo "Downloading $URL"
    curl -fsSL "$URL" -o lib.tar.gz
fi
tar -xzf lib.tar.gz

# The real library is the file the SONAME symlink points at (e.g. libcurl-impersonate.so.4.8.0).
SONAME_FILE="libcurl-impersonate.so.4"
if [ ! -e "$SONAME_FILE" ]; then
    echo "ERROR: $SONAME_FILE not found in $ARCHIVE_NAME (this script needs curl-impersonate v2+)"
    ls -la
    exit 1
fi
REAL_LIB="$(readlink -f "$SONAME_FILE")"
SONAME="$(readelf -d "$REAL_LIB" | sed -n 's/.*(SONAME).*\[\(.*\)\]/\1/p')"
if [ "$SONAME" != "$SONAME_FILE" ]; then
    echo "ERROR: expected SONAME $SONAME_FILE, library reports '$SONAME'"
    exit 1
fi

# Everything the C# side P/Invokes that is NOT a variadic wrapper must come from upstream.
UPSTREAM_SYMBOLS="curl_global_init curl_global_cleanup curl_easy_init curl_easy_cleanup curl_easy_perform
curl_easy_reset curl_easy_duphandle curl_easy_strerror curl_easy_impersonate curl_slist_append
curl_slist_free_all curl_version curl_multi_init curl_multi_cleanup curl_multi_add_handle
curl_multi_remove_handle curl_multi_perform curl_easy_setopt curl_easy_getinfo curl_multi_setopt"
# Capture nm output once: piping nm into `grep -q` under pipefail fails spuriously (nm gets SIGPIPE).
UPSTREAM_EXPORTS="$(nm -D --defined-only "$REAL_LIB")"
for sym in $UPSTREAM_SYMBOLS; do
    grep -q " T $sym\$" <<< "$UPSTREAM_EXPORTS" || { echo "ERROR: upstream library does not export $sym"; exit 1; }
done
echo "Upstream library exports all required functions"

cat > shim.c << 'SHIMEOF'
// Fixed-signature wrappers for libcurl's variadic functions, for P/Invoke on Linux.
// Everything else is used directly from libcurl-impersonate.so.4.
typedef void CURL;
typedef void CURLM;
typedef int CURLcode;
typedef int CURLMcode;
typedef int CURLoption;
typedef int CURLMoption;
typedef int CURLINFO;

extern CURLcode curl_easy_setopt(CURL *curl, CURLoption option, ...);
extern CURLcode curl_easy_getinfo(CURL *curl, CURLINFO info, ...);
extern CURLMcode curl_multi_setopt(CURLM *multi, CURLMoption option, ...);

#define EXPORT __attribute__((visibility("default")))

EXPORT CURLcode curl_easy_setopt_long(CURL *curl, CURLoption option, long value) { return curl_easy_setopt(curl, option, value); }
EXPORT CURLcode curl_easy_setopt_ptr(CURL *curl, CURLoption option, void *value) { return curl_easy_setopt(curl, option, value); }
EXPORT CURLcode curl_easy_setopt_str(CURL *curl, CURLoption option, const char *value) { return curl_easy_setopt(curl, option, value); }
EXPORT CURLcode curl_easy_setopt_cb(CURL *curl, CURLoption option, void *callback) { return curl_easy_setopt(curl, option, callback); }
EXPORT CURLcode curl_easy_getinfo_long(CURL *curl, CURLINFO info, long *value) { return curl_easy_getinfo(curl, info, value); }
EXPORT CURLcode curl_easy_getinfo_str(CURL *curl, CURLINFO info, char **value) { return curl_easy_getinfo(curl, info, value); }
EXPORT CURLcode curl_easy_getinfo_double(CURL *curl, CURLINFO info, double *value) { return curl_easy_getinfo(curl, info, value); }
EXPORT CURLMcode curl_multi_setopt_long(CURLM *multi, CURLMoption option, long value) { return curl_multi_setopt(multi, option, value); }
SHIMEOF

WRAPPERS="curl_easy_setopt_long curl_easy_setopt_ptr curl_easy_setopt_str curl_easy_setopt_cb
curl_easy_getinfo_long curl_easy_getinfo_str curl_easy_getinfo_double curl_multi_setopt_long"

echo "Linking shim against $SONAME_FILE"
gcc -shared -fPIC -fvisibility=hidden -O2 -o libcurl-impersonate.so shim.c \
    -L. -l:"$SONAME_FILE" -Wl,-rpath,'$ORIGIN' -Wl,--no-undefined -Wl,--as-needed

SHIM_DYNAMIC="$(readelf -d libcurl-impersonate.so)"
grep -q "NEEDED.*\[$SONAME_FILE\]" <<< "$SHIM_DYNAMIC" || { echo "ERROR: shim does not depend on $SONAME_FILE"; exit 1; }
grep -q 'RUNPATH.*\[\$ORIGIN\]' <<< "$SHIM_DYNAMIC" || { echo "ERROR: shim RUNPATH is not \$ORIGIN"; exit 1; }
SHIM_EXPORTS="$(nm -D --defined-only libcurl-impersonate.so)"
for sym in $WRAPPERS; do
    grep -q " T $sym\$" <<< "$SHIM_EXPORTS" || { echo "ERROR: shim does not export $sym"; exit 1; }
done
# A wrapper must never call itself: the shim may only reference the three real variadic functions.
UNDEFINED_CURL="$(nm -D --undefined-only libcurl-impersonate.so | awk '/ curl_/{print $2}' | sort | tr '\n' ' ')"
if [ "$UNDEFINED_CURL" != "curl_easy_getinfo curl_easy_setopt curl_multi_setopt " ]; then
    echo "ERROR: shim references unexpected curl symbols: $UNDEFINED_CURL"
    exit 1
fi

# Load test: the shim must resolve its dependency through $ORIGIN alone (no LD_LIBRARY_PATH).
mkdir -p loadtest && cp libcurl-impersonate.so loadtest/ && cp "$REAL_LIB" "loadtest/$SONAME_FILE"
LDD_OUTPUT="$(ldd loadtest/libcurl-impersonate.so)"
if grep -q "not found" <<< "$LDD_OUTPUT"; then
    echo "$LDD_OUTPUT"
    echo "ERROR: shim has unresolved dependencies"
    exit 1
fi

echo "Installing to $NATIVE_DIR"
mkdir -p "$NATIVE_DIR"
rm -f "$NATIVE_DIR"/libcurl-impersonate.so*
cp libcurl-impersonate.so "$NATIVE_DIR/libcurl-impersonate.so"
cp "$REAL_LIB" "$NATIVE_DIR/$SONAME_FILE"
cp "$RUNTIMES_DIR/win-x64/native/cacert.pem" "$NATIVE_DIR/cacert.pem"

echo ""
echo "Done:"
ls -la "$NATIVE_DIR"
