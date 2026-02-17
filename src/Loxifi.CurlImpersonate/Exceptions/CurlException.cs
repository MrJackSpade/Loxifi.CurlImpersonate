namespace Loxifi.CurlImpersonate.Exceptions
{
    /// <summary>
    /// Exception thrown when curl process execution fails.
    /// </summary>
    public class CurlException : Exception
    {
        /// <summary>
        /// Creates a new CurlException.
        /// </summary>
        public CurlException(string message, int exitCode, string standardError)
            : base(message)
        {
            ExitCode = exitCode;
            StandardError = standardError;
        }

        /// <summary>
        /// Creates a new CurlException with an inner exception.
        /// </summary>
        public CurlException(string message, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = -1;
            StandardError = string.Empty;
        }

        /// <summary>
        /// The exit code returned by the curl process.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// The standard error output from curl.
        /// </summary>
        public string StandardError { get; }
    }
}