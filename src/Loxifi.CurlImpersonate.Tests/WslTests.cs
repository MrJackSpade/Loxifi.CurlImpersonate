using System.Net;

namespace Loxifi.CurlImpersonate.Tests
{
    /// <summary>
    /// Tests that run in WSL to verify Linux compatibility.
    /// These tests are skipped if WSL is not available or the Linux library hasn't been built.
    /// </summary>
    [Trait("Category", "WSL")]
    public class WslTests
    {
        [SkippableFact]
        public void WslIsAvailable()
        {
            Skip.IfNot(WslTestHelper.IsWslAvailable, "WSL is not available");

            Assert.True(WslTestHelper.IsWslAvailable);
            Assert.NotNull(WslTestHelper.WslDistro);
        }

        [SkippableFact]
        public void CanRunCommandInWsl()
        {
            Skip.IfNot(WslTestHelper.IsWslAvailable, "WSL is not available");

            var (exitCode, output, _) = WslTestHelper.RunInWsl("echo 'Hello from WSL'");

            Assert.Equal(0, exitCode);
            Assert.Contains("Hello from WSL", output);
        }

        [SkippableFact]
        public void CanCheckDotNetInWsl()
        {
            Skip.IfNot(WslTestHelper.IsWslAvailable, "WSL is not available");

            var (exitCode, output, error) = WslTestHelper.RunInWsl("dotnet --version");

            // Skip if dotnet isn't installed in WSL
            Skip.If(exitCode != 0, $"dotnet not available in WSL: {error}");

            Assert.Equal(0, exitCode);
            Assert.NotEmpty(output.Trim());
        }

        [SkippableFact]
        public void CanBuildLinuxLibrary()
        {
            Skip.IfNot(WslTestHelper.IsWslAvailable, "WSL is not available");

            // Check if gcc is available
            var (gccExitCode, _, gccError) = WslTestHelper.RunInWsl("which gcc");
            Skip.If(gccExitCode != 0, $"gcc not available in WSL: {gccError}");

            bool result = WslTestHelper.BuildLinuxLibrary();

            Assert.True(result, "Failed to build Linux library");
            Assert.True(WslTestHelper.IsLinuxLibraryAvailable, "Linux library not found after build");
        }

        [SkippableFact]
        public void LinuxLibraryExportsRequiredSymbols()
        {
            Skip.IfNot(WslTestHelper.IsWslAvailable, "WSL is not available");
            Skip.IfNot(WslTestHelper.IsLinuxLibraryAvailable, "Linux library not built");

            // Use nm to check for required symbols
            // Note: We use typed variants for setopt/getinfo because variadic functions can't be wrapped
            string[] requiredSymbols =
            [
                "curl_easy_init",
                "curl_easy_perform",
                "curl_easy_cleanup",
                "curl_easy_impersonate",
                "curl_slist_append",
                "curl_slist_free_all",
                "curl_easy_setopt_long",
                "curl_easy_setopt_str",
                "curl_easy_setopt_ptr",
                "curl_easy_getinfo_long",
                "curl_global_init",
                "curl_global_cleanup"
            ];

            string? libPath = GetWslLibraryPath();
            Skip.If(libPath == null, "Could not determine library path");

            // The shim exports the variadic wrappers; everything else comes from libcurl-impersonate.so.4,
            // which the shim loads from its own directory ($ORIGIN). Check the pair together.
            var (exitCode, output, _) = WslTestHelper.RunInWsl($"nm -D --defined-only {libPath} {libPath}.4");
            Skip.If(exitCode != 0, "Could not read library symbols");

            foreach (string symbol in requiredSymbols)
            {
                Assert.Contains(symbol, output);
            }
        }

        private static string? GetWslLibraryPath()
        {
            // Get the repo root and convert to WSL path
            string? dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir, "runtimes")) &&
                    Directory.Exists(Path.Combine(dir, "src")))
                {
                    string libPath = Path.Combine(dir, "runtimes", "linux-x64", "native", "libcurl-impersonate.so");
                    if (File.Exists(libPath))
                    {
                        // Convert Windows path to WSL path
                        if (libPath.Length >= 2 && libPath[1] == ':')
                        {
                            char drive = char.ToLower(libPath[0]);
                            string rest = libPath[2..].Replace('\\', '/');
                            return $"/mnt/{drive}{rest}";
                        }
                    }
                    break;
                }
                if (Directory.Exists(Path.Combine(dir, ".git")))
                {
                    string libPath = Path.Combine(dir, "runtimes", "linux-x64", "native", "libcurl-impersonate.so");
                    if (File.Exists(libPath))
                    {
                        // Convert Windows path to WSL path
                        if (libPath.Length >= 2 && libPath[1] == ':')
                        {
                            char drive = char.ToLower(libPath[0]);
                            string rest = libPath[2..].Replace('\\', '/');
                            return $"/mnt/{drive}{rest}";
                        }
                    }
                    break;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        [SkippableFact]
        public async Task CanRunTestsInWsl()
        {
            Skip.IfNot(WslTestHelper.IsWslAvailable, "WSL is not available");
            Skip.IfNot(WslTestHelper.IsLinuxLibraryAvailable, "Linux library not built");

            // Check if dotnet is available
            var (dotnetExitCode, _, dotnetError) = WslTestHelper.RunInWsl("dotnet --version");
            Skip.If(dotnetExitCode != 0, $"dotnet not available in WSL: {dotnetError}");

            // Run the basic tests in WSL
            var (exitCode, output, error) = WslTestHelper.RunInWsl(
                "cd /mnt/c/git/Loxifi.CurlImpersonate && dotnet test --filter 'FullyQualifiedName~GetAsync_WithCaCert_ReturnsSuccessResponse' --no-build",
                timeoutSeconds: 120);

            // Output test results for debugging
            if (exitCode != 0)
            {
                await Console.Out.WriteLineAsync($"WSL test output:\n{output}");
                await Console.Error.WriteLineAsync($"WSL test errors:\n{error}");
            }

            Assert.Equal(0, exitCode);
        }
    }
}
