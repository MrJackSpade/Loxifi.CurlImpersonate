using System.Runtime.InteropServices;

namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// SafeHandle wrapper for a CURL* easy handle.
    /// </summary>
    internal sealed class CurlHandle : SafeHandle
    {
        public CurlHandle() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
            {
                LibCurl.curl_easy_cleanup(handle);
                handle = IntPtr.Zero;
            }
            return true;
        }
    }
}
