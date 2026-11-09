namespace Loxifi.CurlImpersonate.CodeGen
{
    /// <summary>
    /// A curl-impersonate browser target, e.g. <c>chrome131_android</c> exposed as <c>BrowserProfile.Chrome131Android</c>.
    /// </summary>
    /// <param name="Name">The <c>BrowserProfile</c> enum member name.</param>
    /// <param name="Target">The curl-impersonate target name passed to <c>curl_easy_impersonate</c>.</param>
    internal sealed record ProfileTarget(string Name, string Target)
    {
        /// <summary>
        /// Builds the sorted target list. The fingerprint itself is applied natively by
        /// <c>curl_easy_impersonate</c>, so only the target names are needed.
        /// </summary>
        public static List<ProfileTarget> FromTargetNames(IEnumerable<string> targetNames)
        {
            List<ProfileTarget> targets = targetNames
                .Select(FromTargetName)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();

            if (targets.Count == 0)
            {
                throw new InvalidOperationException("No impersonation targets found");
            }

            return targets;
        }

        public static ProfileTarget FromTargetName(string target)
        {
            string name = string.Concat(target.Split('_').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
            return new ProfileTarget(name, target);
        }
    }
}
