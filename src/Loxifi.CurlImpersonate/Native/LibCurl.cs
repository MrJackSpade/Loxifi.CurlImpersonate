using System.Runtime.InteropServices;

namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// P/Invoke declarations for libcurl and curl-impersonate.
    /// </summary>
    internal static partial class LibCurl
    {
        private const string LibraryName = "libcurl";

        // =================================================================
        // Initialization and cleanup
        // =================================================================

        /// <summary>
        /// Initialize a CURL easy handle.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlHandle curl_easy_init();

        /// <summary>
        /// Clone a CURL easy handle.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlHandle curl_easy_duphandle(CurlHandle curl);

        /// <summary>
        /// Reset all options on a CURL easy handle to their defaults.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_easy_reset(CurlHandle curl);

        /// <summary>
        /// Cleanup and free a CURL easy handle.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_easy_cleanup(IntPtr curl);

        /// <summary>
        /// Initialize the curl library globally.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_global_init(long flags);

        /// <summary>
        /// Cleanup the curl library globally.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_global_cleanup();

        // =================================================================
        // curl_easy_setopt overloads
        // On Windows: use the standard curl_easy_setopt variadic function
        // On Linux: use typed variants from our shim
        // =================================================================

        /// <summary>
        /// Set an option with a long/int value.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_setopt_long")]
        private static extern CurlCode curl_easy_setopt_long_linux(CurlHandle curl, CurlOption option, long value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_setopt(CurlHandle curl, CurlOption option, long value);

        /// <summary>
        /// Set an option with a string value.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_setopt_str", CharSet = CharSet.Ansi)]
        private static extern CurlCode curl_easy_setopt_str_linux(CurlHandle curl, CurlOption option, string value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern CurlCode curl_easy_setopt(CurlHandle curl, CurlOption option, string value);

        /// <summary>
        /// Set an option with a pointer value (for callbacks, slists, etc).
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_setopt_ptr")]
        private static extern CurlCode curl_easy_setopt_ptr_linux(CurlHandle curl, CurlOption option, IntPtr value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_setopt(CurlHandle curl, CurlOption option, IntPtr value);

        /// <summary>
        /// Set an option with a callback delegate (for write callbacks).
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_setopt_cb")]
        private static extern CurlCode curl_easy_setopt_cb_linux(CurlHandle curl, CurlOption option, WriteCallback callback);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_setopt(CurlHandle curl, CurlOption option, WriteCallback callback);

        /// <summary>
        /// Set an option with a callback delegate (for header callbacks).
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_setopt_cb")]
        private static extern CurlCode curl_easy_setopt_cb_header_linux(CurlHandle curl, CurlOption option, HeaderCallback callback);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_setopt(CurlHandle curl, CurlOption option, HeaderCallback callback);

        // Public wrappers that select the appropriate implementation
        public static CurlCode SetOpt(CurlHandle curl, CurlOption option, long value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_setopt_long_linux(curl, option, value);
            }
            return curl_easy_setopt(curl, option, value);
        }

        public static CurlCode SetOpt(CurlHandle curl, CurlOption option, string value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_setopt_str_linux(curl, option, value);
            }
            return curl_easy_setopt(curl, option, value);
        }

        public static CurlCode SetOpt(CurlHandle curl, CurlOption option, IntPtr value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_setopt_ptr_linux(curl, option, value);
            }
            return curl_easy_setopt(curl, option, value);
        }

        public static CurlCode SetOpt(CurlHandle curl, CurlOption option, WriteCallback callback)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_setopt_cb_linux(curl, option, callback);
            }
            return curl_easy_setopt(curl, option, callback);
        }

        public static CurlCode SetOpt(CurlHandle curl, CurlOption option, HeaderCallback callback)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_setopt_cb_header_linux(curl, option, callback);
            }
            return curl_easy_setopt(curl, option, callback);
        }

        // =================================================================
        // Perform and info
        // =================================================================

        /// <summary>
        /// Perform the transfer.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CurlCode curl_easy_perform(CurlHandle curl);

        /// <summary>
        /// Get info as a string.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_getinfo_str")]
        private static extern CurlCode curl_easy_getinfo_str_linux(CurlHandle curl, CurlInfo info, out IntPtr value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_getinfo(CurlHandle curl, CurlInfo info, out IntPtr value);

        /// <summary>
        /// Get info as a long.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_getinfo_long")]
        private static extern CurlCode curl_easy_getinfo_long_linux(CurlHandle curl, CurlInfo info, out long value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_getinfo(CurlHandle curl, CurlInfo info, out long value);

        /// <summary>
        /// Get info as a double.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "curl_easy_getinfo_double")]
        private static extern CurlCode curl_easy_getinfo_double_linux(CurlHandle curl, CurlInfo info, out double value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CurlCode curl_easy_getinfo(CurlHandle curl, CurlInfo info, out double value);

        // Public wrappers for getinfo
        public static CurlCode GetInfo(CurlHandle curl, CurlInfo info, out IntPtr value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_getinfo_str_linux(curl, info, out value);
            }
            return curl_easy_getinfo(curl, info, out value);
        }

        public static CurlCode GetInfo(CurlHandle curl, CurlInfo info, out long value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_getinfo_long_linux(curl, info, out value);
            }
            return curl_easy_getinfo(curl, info, out value);
        }

        public static CurlCode GetInfo(CurlHandle curl, CurlInfo info, out double value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return curl_easy_getinfo_double_linux(curl, info, out value);
            }
            return curl_easy_getinfo(curl, info, out value);
        }

        // =================================================================
        // curl-impersonate extension
        // =================================================================

        /// <summary>
        /// Set impersonation options for a specific browser target.
        /// </summary>
        /// <param name="curl">The CURL handle.</param>
        /// <param name="target">Browser target (e.g., "chrome136", "firefox133").</param>
        /// <param name="defaultHeaders">Whether to use default headers (1) or not (0).</param>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern CurlCode curl_easy_impersonate(CurlHandle curl, string target, int defaultHeaders);

        // =================================================================
        // String list functions
        // =================================================================

        /// <summary>
        /// Append a string to a slist.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr curl_slist_append(IntPtr list, string value);

        /// <summary>
        /// Free all entries in a slist.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_slist_free_all(IntPtr list);

        // =================================================================
        // Error handling
        // =================================================================

        /// <summary>
        /// Get a human-readable error message for a CurlCode.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr curl_easy_strerror(CurlCode code);

        /// <summary>
        /// Get the error string for a CurlCode as a managed string.
        /// </summary>
        public static string GetErrorString(CurlCode code)
        {
            IntPtr ptr = curl_easy_strerror(code);
            return ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) ?? string.Empty : string.Empty;
        }

        // =================================================================
        // Version info
        // =================================================================

        /// <summary>
        /// Get the libcurl version string.
        /// </summary>
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr curl_version();

        /// <summary>
        /// Get the libcurl version as a managed string.
        /// </summary>
        public static string GetVersion()
        {
            IntPtr ptr = curl_version();
            return ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) ?? string.Empty : string.Empty;
        }

        // =================================================================
        // Global init flags
        // =================================================================

        public const long CURL_GLOBAL_SSL = 1 << 0;
        public const long CURL_GLOBAL_WIN32 = 1 << 1;
        public const long CURL_GLOBAL_ALL = CURL_GLOBAL_SSL | CURL_GLOBAL_WIN32;
        public const long CURL_GLOBAL_DEFAULT = CURL_GLOBAL_ALL;
    }

    // =================================================================
    // Callback delegates
    // =================================================================

    /// <summary>
    /// Callback for receiving data (CURLOPT_WRITEFUNCTION).
    /// </summary>
    /// <param name="ptr">Pointer to the data.</param>
    /// <param name="size">Size of each element.</param>
    /// <param name="nmemb">Number of elements.</param>
    /// <param name="userdata">User data pointer.</param>
    /// <returns>Number of bytes handled (should be size * nmemb on success).</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint WriteCallback(IntPtr ptr, nuint size, nuint nmemb, IntPtr userdata);

    /// <summary>
    /// Callback for receiving headers (CURLOPT_HEADERFUNCTION).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint HeaderCallback(IntPtr ptr, nuint size, nuint nmemb, IntPtr userdata);

    /// <summary>
    /// Callback for reading data (CURLOPT_READFUNCTION).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint ReadCallback(IntPtr ptr, nuint size, nuint nmemb, IntPtr userdata);

    /// <summary>
    /// Callback for progress reporting (CURLOPT_XFERINFOFUNCTION).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ProgressCallback(IntPtr clientp, long dltotal, long dlnow, long ultotal, long ulnow);
}
