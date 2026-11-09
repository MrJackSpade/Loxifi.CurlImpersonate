using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Loxifi.CurlImpersonate.Tests
{
    /// <summary>
    /// Helper class for running tests in WSL.
    /// </summary>
    public static class WslTestHelper
    {
        private static bool? _isWslAvailable;
        private static string? _wslDistro;

        /// <summary>
        /// Gets whether WSL is available on this system.
        /// </summary>
        public static bool IsWslAvailable
        {
            get
            {
                if (_isWslAvailable.HasValue)
                {
                    return _isWslAvailable.Value;
                }

                _isWslAvailable = CheckWslAvailability();
                return _isWslAvailable.Value;
            }
        }

        /// <summary>
        /// Gets the default WSL distribution name, or null if WSL is not available.
        /// </summary>
        public static string? WslDistro => _wslDistro;

        /// <summary>
        /// Checks if the Linux native library has been built and is available.
        /// </summary>
        public static bool IsLinuxLibraryAvailable
        {
            get
            {
                if (!IsWslAvailable)
                {
                    return false;
                }

                string? libPath = FindLinuxLibrary();
                return libPath != null;
            }
        }

        /// <summary>
        /// Builds the Linux shim library using WSL.
        /// </summary>
        /// <returns>True if the build succeeded.</returns>
        public static bool BuildLinuxLibrary()
        {
            if (!IsWslAvailable)
            {
                return false;
            }

            string? repoRoot = FindRepoRoot();
            if (repoRoot == null)
            {
                return false;
            }

            string buildScript = Path.Combine(repoRoot, "build", "linux", "build-shim.sh");
            if (!File.Exists(buildScript))
            {
                return false;
            }

            // Convert Windows path to WSL path
            string wslPath = ConvertToWslPath(buildScript);

            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "wsl",
                    Arguments = $"bash \"{wslPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(psi);
                if (process == null)
                {
                    return false;
                }

                process.WaitForExit(TimeSpan.FromMinutes(5));
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Runs a command in WSL and returns the output.
        /// </summary>
        public static (int ExitCode, string Output, string Error) RunInWsl(string command, int timeoutSeconds = 60)
        {
            if (!IsWslAvailable)
            {
                return (-1, "", "WSL is not available");
            }

            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "wsl",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(psi);
                if (process == null)
                {
                    return (-1, "", "Failed to start WSL process");
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(timeoutSeconds * 1000);

                return (process.ExitCode, output, error);
            }
            catch (Exception ex)
            {
                return (-1, "", ex.Message);
            }
        }

        private static bool CheckWslAvailability()
        {
            // Only check on Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "wsl",
                    Arguments = "--status",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(psi);
                if (process == null)
                {
                    return false;
                }

                process.WaitForExit(5000);

                if (process.ExitCode != 0)
                {
                    return false;
                }

                // Try to get the default distro
                psi.Arguments = "--list --quiet";
                using Process? listProcess = Process.Start(psi);
                if (listProcess != null)
                {
                    string output = listProcess.StandardOutput.ReadToEnd();
                    listProcess.WaitForExit(5000);

                    // Clean up the distro names (remove null chars from UTF-16 encoding)
                    string[] distros = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => new string(line.Where(c => c != '\0').ToArray()).Trim())
                        .Where(name => name.Length > 0)
                        .ToArray();
                    if (distros.Length > 0)
                    {
                        _wslDistro = distros[0];
                    }
                }

                // The wsl.exe launcher can exist with no Linux distribution installed (e.g. GitHub's
                // windows-latest image); WSL is only usable when at least one distro is present.
                return _wslDistro != null;
            }
            catch
            {
                return false;
            }
        }

        private static string ConvertToWslPath(string windowsPath)
        {
            // Convert C:\path\to\file to /mnt/c/path/to/file
            if (windowsPath.Length >= 2 && windowsPath[1] == ':')
            {
                char drive = char.ToLower(windowsPath[0]);
                string rest = windowsPath[2..].Replace('\\', '/');
                return $"/mnt/{drive}{rest}";
            }

            return windowsPath.Replace('\\', '/');
        }

        private static string? FindLinuxLibrary()
        {
            string? repoRoot = FindRepoRoot();
            if (repoRoot == null)
            {
                return null;
            }

            string[] possiblePaths =
            [
                Path.Combine(repoRoot, "runtimes", "linux-x64", "native", "libcurl-impersonate.so"),
                Path.Combine(repoRoot, "runtimes", "linux-arm64", "native", "libcurl-impersonate.so"),
            ];

            return possiblePaths.FirstOrDefault(File.Exists);
        }

        private static string? FindRepoRoot()
        {
            string? dir = AppContext.BaseDirectory;

            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir, "runtimes")) &&
                    Directory.Exists(Path.Combine(dir, "src")))
                {
                    return dir;
                }

                // Check for .git folder
                if (Directory.Exists(Path.Combine(dir, ".git")))
                {
                    return dir;
                }

                dir = Path.GetDirectoryName(dir);
            }

            return null;
        }
    }
}
