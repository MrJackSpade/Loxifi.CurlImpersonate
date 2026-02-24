using System.Reflection;
using System.Runtime.InteropServices;

namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// Handles resolving the native libcurl library from the correct runtime path.
    /// </summary>
    internal static class NativeLibraryResolver
    {
        private static bool _initialized;
        private static readonly object _lock = new();

        /// <summary>
        /// Ensures the native library resolver is registered.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (_lock)
            {
                if (_initialized)
                {
                    return;
                }

                NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, ResolveDllImport);
                _initialized = true;
            }
        }

        private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != "libcurl")
            {
                return IntPtr.Zero;
            }

            // Try to load from various locations
            string[] possiblePaths = GetPossibleLibraryPaths();

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path) && NativeLibrary.TryLoad(path, out IntPtr handle))
                {
                    return handle;
                }
            }

            // Fall back to default search
            if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out IntPtr defaultHandle))
            {
                return defaultHandle;
            }

            return IntPtr.Zero;
        }

        private static string[] GetPossibleLibraryPaths()
        {
            string assemblyLocation = typeof(NativeLibraryResolver).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;

            string rid = GetRuntimeIdentifier();
            string libraryName = GetLibraryFileName();

            return
            [
                // Standard .NET runtime paths
                Path.Combine(assemblyDir, "runtimes", rid, "native", libraryName),
                Path.Combine(assemblyDir, libraryName),
                Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", libraryName),
                Path.Combine(AppContext.BaseDirectory, libraryName),

                // Development paths
                Path.Combine(assemblyDir, "..", "..", "..", "..", "..", "runtimes", rid, "native", libraryName),
            ];
        }

        private static string GetLibraryFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "libcurl-impersonate.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "libcurl-impersonate.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "libcurl-impersonate.dylib";
            }

            return "libcurl";
        }

        private static string GetRuntimeIdentifier()
        {
            string arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                Architecture.Arm => "arm",
                _ => "x64"
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"win-{arch}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"linux-{arch}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"osx-{arch}";
            }

            return $"win-{arch}";
        }
    }
}
