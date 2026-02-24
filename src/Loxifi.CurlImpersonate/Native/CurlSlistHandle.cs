using System.Runtime.InteropServices;

namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// SafeHandle wrapper for a curl_slist* linked list.
    /// </summary>
    internal sealed class CurlSlistHandle : SafeHandle
    {
        public CurlSlistHandle() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        public CurlSlistHandle(IntPtr existingHandle, bool ownsHandle) : base(existingHandle, ownsHandle)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Appends a string to the slist.
        /// </summary>
        public void Append(string value)
        {
            IntPtr newHandle = LibCurl.curl_slist_append(handle, value);
            if (newHandle == IntPtr.Zero)
            {
                throw new OutOfMemoryException("Failed to append to curl_slist");
            }
            SetHandle(newHandle);
        }

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
            {
                LibCurl.curl_slist_free_all(handle);
                handle = IntPtr.Zero;
            }
            return true;
        }
    }
}
