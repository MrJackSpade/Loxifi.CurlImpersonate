namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// CURLINFO constants for curl_easy_getinfo.
    /// </summary>
    internal enum CurlInfo
    {
        // String info
        EffectiveUrl = 0x100001,
        ContentType = 0x100012,
        PrivateData = 0x100015,
        FtpEntryPath = 0x100016,
        RedirectUrl = 0x100017,
        PrimaryIp = 0x100020,
        LocalIp = 0x100029,
        Scheme = 0x10002A,
        CaInfo = 0x10002B,
        CaPath = 0x10002C,

        // Long info
        ResponseCode = 0x200002,
        HeaderSize = 0x20000B,
        RequestSize = 0x20000C,
        SslVerifyResult = 0x20000D,
        FileTime = 0x20000E,
        RedirectCount = 0x200014,
        HttpConnectCode = 0x200016,
        HttpAuthAvail = 0x200017,
        ProxyAuthAvail = 0x200018,
        OsErrno = 0x200019,
        NumConnects = 0x20001A,
        ConditionUnmet = 0x20001C,
        RtspClientCseq = 0x20001D,
        RtspServerCseq = 0x20001E,
        RtspCseqRecv = 0x20001F,
        PrimaryPort = 0x200021,
        LocalPort = 0x200022,
        HttpVersion = 0x200026,
        ProxyError = 0x200028,
        ProxySslVerifyResult = 0x200027,
        Protocol = 0x200023,

        // Double info
        TotalTime = 0x300003,
        NameLookupTime = 0x300004,
        ConnectTime = 0x300005,
        PreTransferTime = 0x300006,
        SizeUpload = 0x300007,
        SizeDownload = 0x300008,
        SpeedDownload = 0x300009,
        SpeedUpload = 0x30000A,
        ContentLengthDownload = 0x30000F,
        ContentLengthUpload = 0x300010,
        StartTransferTime = 0x300011,
        RedirectTime = 0x300013,
        AppConnectTime = 0x300021,

        // Off_t info (64-bit)
        FileTimeT = 0x60002E,
        TotalTimeT = 0x60002F,
        NameLookupTimeT = 0x600030,
        ConnectTimeT = 0x600031,
        PreTransferTimeT = 0x600032,
        StartTransferTimeT = 0x600033,
        RedirectTimeT = 0x600034,
        AppConnectTimeT = 0x600035,
        RetryAfter = 0x600036,
        SizeUploadT = 0x600037,
        SizeDownloadT = 0x600038,
        SpeedDownloadT = 0x600039,
        SpeedUploadT = 0x60003A,
        ContentLengthDownloadT = 0x60003B,
        ContentLengthUploadT = 0x60003C,

        // Slist info
        SslEngines = 0x40001B,
        CookieList = 0x40001C,
        CertInfo = 0x40001D,
        TlsSession = 0x40001E,
        TlsSslPtr = 0x40001F,
    }
}
