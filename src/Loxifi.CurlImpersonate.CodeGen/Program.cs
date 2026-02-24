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

        // Fetch latest release
        Console.WriteLine("Fetching latest curl-impersonate release...");
        using GitHubReleaseClient gitHubClient = new();
        GitHubRelease release = await gitHubClient.GetLatestReleaseAsync();
        Console.WriteLine($"Latest release: {release.TagName}");

        // Find the Windows x64 asset
        GitHubAsset? windowsAsset = release.Assets.FirstOrDefault(a =>
            a.Name.Contains("win32") && a.Name.Contains("x86_64") && a.Name.EndsWith(".tar.gz"));

        if (windowsAsset == null)
        {
            Console.WriteLine("ERROR: Could not find Windows x64 asset in release.");
            Console.WriteLine("Available assets:");
            foreach (GitHubAsset asset in release.Assets)
            {
                Console.WriteLine($"  - {asset.Name}");
            }

            return 1;
        }

        Console.WriteLine($"Downloading: {windowsAsset.Name} ({windowsAsset.Size / 1024 / 1024:F2} MB)");

        // Download and extract
        string tempDir = Path.Combine(Path.GetTempPath(), "curl-impersonate-codegen", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string archivePath = Path.Combine(tempDir, windowsAsset.Name);

            using (Stream downloadStream = await gitHubClient.DownloadAssetAsync(windowsAsset.BrowserDownloadUrl))
            using (FileStream fileStream = File.Create(archivePath))
            {
                await downloadStream.CopyToAsync(fileStream);
            }

            Console.WriteLine("Extracting archive...");

            // Ensure runtimes directory exists
            Directory.CreateDirectory(runtimesDir);

            // Clear existing files in runtimes directory
            foreach (string file in Directory.GetFiles(runtimesDir))
            {
                File.Delete(file);
            }

            // First decompress gzip to tar
            string tarPath = Path.Combine(tempDir, "archive.tar");
            using (FileStream gzipFileStream = File.OpenRead(archivePath))
            using (GZipStream gzipStream = new(gzipFileStream, CompressionMode.Decompress))
            using (FileStream tarFileStream = File.Create(tarPath))
            {
                gzipStream.CopyTo(tarFileStream);
            }

            Console.WriteLine("  Decompressed to tar file");

            // Now extract the tar archive (only necessary files)
            using (TarArchive tarArchive = TarArchive.Open(tarPath))
            {
                Console.WriteLine($"  Entry count: {tarArchive.Entries.Count}");

                foreach (TarArchiveEntry entry in tarArchive.Entries)
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }

                    string entryKey = entry.Key ?? string.Empty;

                    // Get just the filename without any directory prefixes
                    string fileName = Path.GetFileName(entryKey);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    // Only extract necessary files:
                    // - curl.exe (the main executable)
                    // - *.dll (required libraries)
                    // - curl_*.bat (browser profile scripts)
                    bool isRequired = fileName.Equals("curl.exe", StringComparison.OrdinalIgnoreCase) ||
                                     fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                                     fileName.StartsWith("curl_", StringComparison.OrdinalIgnoreCase) &&
                                      fileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase);

                    if (!isRequired)
                    {
                        continue;
                    }

                    string destPath = Path.Combine(runtimesDir, fileName);

                    using Stream entryStream = entry.OpenEntryStream();
                    using FileStream destStream = File.Create(destPath);
                    entryStream.CopyTo(destStream);

                    Console.WriteLine($"  Extracted: {fileName}");
                }
            }

            Console.WriteLine($"Extracted files to: {runtimesDir}");

            // List extracted files
            string[] extractedFiles = Directory.GetFiles(runtimesDir);
            Console.WriteLine($"Total files extracted: {extractedFiles.Length}");

            // Download CA certificate bundle
            Console.WriteLine();
            Console.WriteLine("Downloading CA certificate bundle...");
            string caCertPath = Path.Combine(runtimesDir, "cacert.pem");
            try
            {
                using HttpClient httpClient = new();
                byte[] caCertData = await httpClient.GetByteArrayAsync("https://curl.se/ca/cacert.pem");
                await File.WriteAllBytesAsync(caCertPath, caCertData);
                Console.WriteLine($"  Downloaded: cacert.pem ({caCertData.Length / 1024} KB)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  WARNING: Failed to download CA certificate bundle: {ex.Message}");
                Console.WriteLine("  SSL verification may fail without a CA bundle.");
            }

            // Find and parse bat files
            Console.WriteLine();
            Console.WriteLine("Parsing bat files...");

            string[] batFiles = Directory.GetFiles(runtimesDir, "curl_*.bat");
            Console.WriteLine($"Found {batFiles.Length} bat files");

            BatFileParser parser = new();
            List<ParsedProfile> profiles = new();

            foreach (string batFile in batFiles)
            {
                try
                {
                    ParsedProfile profile = parser.Parse(batFile);
                    profiles.Add(profile);
                    Console.WriteLine($"  Parsed: {profile.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ERROR parsing {Path.GetFileName(batFile)}: {ex.Message}");
                }
            }

            // Generate code
            Console.WriteLine();
            Console.WriteLine("Generating code...");

            CodeGenerator generator = new();

            string enumPath = Path.Combine(libraryDir, "BrowserProfile.cs");
            CodeGenerator.GenerateBrowserProfileEnum(profiles, enumPath, release.TagName);
            Console.WriteLine($"  Generated: {enumPath}");

            string settingsPath = Path.Combine(libraryDir, "BrowserProfileSettings.cs");
            generator.GenerateBrowserProfileSettings(profiles, settingsPath, release.TagName);
            Console.WriteLine($"  Generated: {settingsPath}");

            Console.WriteLine();
            Console.WriteLine($"Successfully generated code for {profiles.Count} browser profiles!");
            Console.WriteLine($"Version: {release.TagName}");

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

        static string FindProjectRoot(string startDir)
        {
            string? dir = startDir;
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "Loxifi.CurlImpersonate.sln")))
                {
                    return dir;
                }

                // Also check if we're in bin/Debug/net8.0 structure
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