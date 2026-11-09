using SharpCompress.Readers;

namespace Loxifi.CurlImpersonate.CodeGen
{
    /// <summary>
    /// Updates the bundled Windows x64 native runtime from a curl-impersonate release.
    /// </summary>
    /// <remarks>
    /// Since curl-impersonate v2 the release ships two Windows archives:
    /// <c>libcurl-impersonate-&lt;tag&gt;.x86_64-win32.tar.gz</c> (the DLL this library loads) and
    /// <c>curl-impersonate-&lt;tag&gt;.x86_64-win32.tar.gz</c> (the CLI, whose <c>curl_&lt;target&gt;.bat</c>
    /// wrappers are the list of supported impersonation targets). Only the DLL is shipped; the CLI
    /// archive is read for target names only.
    /// </remarks>
    internal static class WindowsRuntime
    {
        private const string LibraryFileName = "libcurl-impersonate.dll";

        public static async Task<List<ProfileTarget>> UpdateAsync(
            GitHubReleaseClient gitHubClient, GitHubRelease release, string runtimesDir, string tempDir)
        {
            GitHubAsset libraryAsset = FindAsset(release, "libcurl-impersonate");
            GitHubAsset cliAsset = FindAsset(release, "curl-impersonate");

            // Download everything before touching the runtimes directory.
            string libraryArchive = await DownloadAsync(gitHubClient, libraryAsset, tempDir);
            string cliArchive = await DownloadAsync(gitHubClient, cliAsset, tempDir);

            List<ProfileTarget> targets = ProfileTarget.FromTargetNames(ReadTargetNames(cliArchive));

            Directory.CreateDirectory(runtimesDir);
            foreach (string file in Directory.GetFiles(runtimesDir))
            {
                File.Delete(file);
            }

            ExtractFile(libraryArchive, LibraryFileName, Path.Combine(runtimesDir, LibraryFileName));
            Console.WriteLine($"  Extracted: {LibraryFileName}");
            return targets;
        }

        private static GitHubAsset FindAsset(GitHubRelease release, string product)
        {
            string name = $"{product}-{release.TagName}.x86_64-win32.tar.gz";
            return release.Assets.SingleOrDefault(a => a.Name == name)
                ?? throw new InvalidOperationException(
                    $"Release {release.TagName} has no asset '{name}'. Assets: {string.Join(", ", release.Assets.Select(a => a.Name))}");
        }

        private static async Task<string> DownloadAsync(GitHubReleaseClient gitHubClient, GitHubAsset asset, string tempDir)
        {
            Console.WriteLine($"Downloading: {asset.Name} ({asset.Size / 1024.0 / 1024.0:F2} MB)");
            string path = Path.Combine(tempDir, asset.Name);
            using Stream download = await gitHubClient.DownloadAssetAsync(asset.BrowserDownloadUrl);
            using FileStream file = File.Create(path);
            await download.CopyToAsync(file);
            return path;
        }

        private static List<string> ReadTargetNames(string cliArchive)
        {
            const string prefix = "curl_";
            const string suffix = ".bat";
            List<string> names = new();
            using FileStream stream = File.OpenRead(cliArchive);
            using IReader reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                string fileName = Path.GetFileName(reader.Entry.Key ?? string.Empty);
                if (!reader.Entry.IsDirectory && fileName.StartsWith(prefix, StringComparison.Ordinal) && fileName.EndsWith(suffix, StringComparison.Ordinal))
                {
                    names.Add(fileName[prefix.Length..^suffix.Length]);
                }
            }

            return names;
        }

        private static void ExtractFile(string archive, string fileName, string destination)
        {
            using FileStream stream = File.OpenRead(archive);
            using IReader reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory && Path.GetFileName(reader.Entry.Key ?? string.Empty) == fileName)
                {
                    using FileStream output = File.Create(destination);
                    reader.WriteEntryTo(output);
                    return;
                }
            }

            throw new InvalidOperationException($"{Path.GetFileName(archive)} does not contain {fileName}");
        }
    }
}
