using Loxifi.CurlImpersonate.CodeGen;
using SharpCompress.Archives.Tar;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Loxifi.CurlImpersonate Code Generator");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        // Determine paths
        string codeGenDir = AppContext.BaseDirectory;
        string projectRoot = FindProjectRoot(codeGenDir);
        string libraryDir = Path.Combine(projectRoot, "src", "Loxifi.CurlImpersonate");
        string runtimesDir = Path.Combine(projectRoot, "runtimes", "win-x64", "native");
        string linuxBuildScript = Path.Combine(projectRoot, "build", "linux", "build-shim.sh");

        Console.WriteLine($"Project root: {projectRoot}");
        Console.WriteLine($"Library dir: {libraryDir}");
        Console.WriteLine($"Runtimes dir: {runtimesDir}");
        Console.WriteLine();

        // Pick the release: `--version <tag>` pins one, otherwise the latest.
        using GitHubReleaseClient gitHubClient = new();
        GitHubRelease release = args.Length == 2 && args[0] == "--version"
            ? await gitHubClient.GetReleaseAsync(args[1])
            : await gitHubClient.GetLatestReleaseAsync();
        Console.WriteLine($"curl-impersonate release: {release.TagName}");

        string tempDir = Path.Combine(Path.GetTempPath(), "curl-impersonate-codegen", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            List<ProfileTarget> targets = await WindowsRuntime.UpdateAsync(gitHubClient, release, runtimesDir, tempDir);

            Console.WriteLine();
            Console.WriteLine("Downloading CA certificate bundle...");
            using (HttpClient httpClient = new())
            {
                byte[] caCertData = await httpClient.GetByteArrayAsync("https://curl.se/ca/cacert.pem");
                await File.WriteAllBytesAsync(Path.Combine(runtimesDir, "cacert.pem"), caCertData);
                Console.WriteLine($"  Downloaded: cacert.pem ({caCertData.Length / 1024} KB)");
            }

            GenerateSources(targets, libraryDir, release.TagName);

            // Build Linux library if WSL is available
            Console.WriteLine();
            Console.WriteLine("Checking for WSL to build Linux library...");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(linuxBuildScript))
            {
                bool wslAvailable = await CheckWslAvailableAsync();
                if (wslAvailable)
                {
                    Console.WriteLine("WSL detected. Building Linux library...");
                    bool linuxBuildSuccess = await BuildLinuxLibraryAsync(linuxBuildScript);
                    if (linuxBuildSuccess)
                    {
                        Console.WriteLine("Linux library built successfully!");
                    }
                    else
                    {
                        Console.WriteLine("WARNING: Linux library build failed. See errors above.");
                        Console.WriteLine("You can manually build it later by running: wsl bash build/linux/build-shim.sh");
                    }
                }
                else
                {
                    Console.WriteLine("WSL not available. Skipping Linux library build.");
                    Console.WriteLine("To build Linux library manually, install WSL and run: wsl bash build/linux/build-shim.sh");
                }
            }
            else if (!File.Exists(linuxBuildScript))
            {
                Console.WriteLine($"Linux build script not found at: {linuxBuildScript}");
            }

            return 0;
        }
        finally
        {
            // Cleanup temp directory
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        static void GenerateSources(List<ProfileTarget> targets, string libraryDir, string version)
        {
            Console.WriteLine();
            Console.WriteLine($"Found {targets.Count} impersonation targets:");
            foreach (ProfileTarget target in targets)
            {
                Console.WriteLine($"  {target.Name} -> {target.Target}");
            }

            string enumPath = Path.Combine(libraryDir, "BrowserProfile.cs");
            CodeGenerator.GenerateBrowserProfileEnum(targets, enumPath, version);
            Console.WriteLine($"  Generated: {enumPath}");

            string targetsPath = Path.Combine(libraryDir, "BrowserProfileTargets.cs");
            CodeGenerator.GenerateBrowserProfileTargets(targets, targetsPath, version);
            Console.WriteLine($"  Generated: {targetsPath}");
        }

        static string FindProjectRoot(string startDir)
        {
            string? dir = startDir;
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "Loxifi.CurlImpersonate.sln")))
                {
                    return dir;
                }

                // Also check if we're in a bin/<Configuration>/<TargetFramework> structure
                string potentialRoot = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "..", ".."));
                if (File.Exists(Path.Combine(potentialRoot, "Loxifi.CurlImpersonate.sln")))
                {
                    return potentialRoot;
                }

                dir = Path.GetDirectoryName(dir);
            }

            // Fall back to current directory
            return Environment.CurrentDirectory;
        }

        static async Task<bool> CheckWslAvailableAsync()
        {
            try
            {
                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wsl",
                        Arguments = "--status",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        static async Task<bool> BuildLinuxLibraryAsync(string buildScript)
        {
            try
            {
                // Convert Windows path to WSL path
                string wslPath = ConvertToWslPath(buildScript);

                // First fix line endings
                Console.WriteLine("  Fixing line endings...");
                using (Process sedProcess = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wsl",
                        Arguments = $"sed -i 's/\\r$//' \"{wslPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                })
                {
                    sedProcess.Start();
                    await sedProcess.WaitForExitAsync();
                }

                // Run the build script
                Console.WriteLine("  Running build script...");
                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wsl",
                        Arguments = $"bash \"{wslPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"  {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"  ERROR: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait up to 5 minutes for the build
                bool completed = await Task.Run(() => process.WaitForExit(300000));

                if (!completed)
                {
                    Console.WriteLine("  Build timed out after 5 minutes");
                    try { process.Kill(); } catch { }
                    return false;
                }

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Exception during build: {ex.Message}");
                return false;
            }
        }

        static string ConvertToWslPath(string windowsPath)
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
    }
}