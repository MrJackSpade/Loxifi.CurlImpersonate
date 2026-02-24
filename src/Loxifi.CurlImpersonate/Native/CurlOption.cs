namespace Loxifi.CurlImpersonate.Native
{
    /// <summary>
    /// CURLOPT constants for curl_easy_setopt.
    /// Includes both standard libcurl options and curl-impersonate extensions.
    /// </summary>
    internal enum CurlOption
    {
        // =================================================================
        // Standard libcurl options
        // =================================================================

        // Behavior options
        Verbose = 41,
        Header = 42,
        NoProgress = 43,
        NoSignal = 99,
        WildcardMatch = 197,

        // Callback options
        WriteFunction = 20011,
        WriteData = 10001,
        ReadFunction = 20012,
        ReadData = 10009,
        HeaderFunction = 20079,
        HeaderData = 10029,
        ProgressFunction = 20056,
        ProgressData = 10057,
        XferInfoFunction = 20219,
        XferInfoData = 10057,

        // Error options
        ErrorBuffer = 10010,
        StdErr = 10037,
        FailOnError = 45,
        KeepSendingOnError = 245,

        // Network options
        Url = 10002,
        Path = 10169,
        Port = 3,
        ProxyPort = 59,
        Proxy = 10004,
        ProxyType = 101,
        NoProxy = 10177,
        HttpProxyTunnel = 61,
        ConnectOnly = 141,
        Socks5Auth = 267,
        Socks5GssApiNec = 180,
        ProxyServiceName = 10235,
        HaProxyProtocol = 274,
        Interface = 10062,
        LocalPort = 139,
        LocalPortRange = 140,
        DnsCacheTimeout = 92,
        DnsUseGlobalCache = 91,
        DnsInterface = 10221,
        DnsLocalIp4 = 10222,
        DnsLocalIp6 = 10223,
        DnsServers = 10211,
        DnsShuffleAddresses = 275,
        BufferSize = 98,
        TcpFastOpen = 244,
        TcpNoDelay = 121,
        AddressScope = 171,
        TcpKeepalive = 213,
        TcpKeepidle = 214,
        TcpKeepintvl = 215,
        UnixSocketPath = 10231,
        AbstractUnixSocket = 10264,

        // Names and password options
        Netrc = 51,
        NetrcFile = 10118,
        Userpwd = 10005,
        ProxyUserpwd = 10006,
        Username = 10173,
        Password = 10174,
        LoginOptions = 10224,
        ProxyUsername = 10175,
        ProxyPassword = 10176,
        HttpAuth = 107,
        TlsAuthUsername = 10204,
        TlsAuthPassword = 10205,
        TlsAuthType = 10206,
        ProxyTlsAuthUsername = 10251,
        ProxyTlsAuthPassword = 10252,
        ProxyTlsAuthType = 10253,
        SaslAuthzid = 10289,
        SaslIr = 218,
        Xoauth2Bearer = 10220,
        DisallowUsernameInUrl = 278,

        // HTTP options
        AutoReferer = 58,
        AcceptEncoding = 10102,
        TransferEncoding = 207,
        FollowLocation = 52,
        UnrestrictedAuth = 105,
        MaxRedirs = 68,
        PostRedir = 161,
        Put = 54,
        Post = 47,
        PostFields = 10015,
        PostFieldSize = 60,
        PostFieldSizeLarge = 30120,
        CopyPostFields = 10165,
        HttpPost = 10024,
        Referer = 10016,
        UserAgent = 10018,
        HttpHeader = 10023,
        HeaderOpt = 229,
        ProxyHeader = 10228,
        Http200Aliases = 10104,
        Cookie = 10022,
        CookieFile = 10031,
        CookieJar = 10082,
        CookieSession = 96,
        CookieList = 10135,
        AltsvcCtrl = 286,
        Altsvc = 10287,
        HstsCtrl = 299,
        Hsts = 10300,
        HttpGet = 80,
        RequestTarget = 10266,
        HttpVersion = 84,
        Http09Allowed = 285,
        IgnoreContentLength = 136,
        HttpContentDecoding = 158,
        HttpTransferDecoding = 157,
        Expect100TimeoutMs = 227,
        TrailerFunction = 20283,
        TrailerData = 10284,
        PipewWait = 237,
        StreamDepends = 10240,
        StreamDependsE = 10241,
        StreamWeight = 239,

        // SMTP options
        MailFrom = 10186,
        MailRcpt = 10187,
        MailAuth = 10217,
        MailRcptAllowfails = 290,

        // TFTP options
        TftpBlocksize = 178,
        TftpNoOptions = 242,

        // FTP options
        FtpPort = 10017,
        Quote = 10028,
        PostQuote = 10039,
        PreQuote = 10093,
        AppendFile = 50,
        FtpUsePret = 188,
        FtpUseEprt = 106,
        FtpUseEpsv = 85,
        FtpCreateMissingDirs = 110,
        ServerResponseTimeout = 112,
        FtpAlternativeToUser = 10147,
        FtpSkipPasvIp = 137,
        FtpSslCcc = 154,
        FtpAccount = 10134,
        FtpFileMethod = 138,

        // RTSP options
        RtspRequest = 189,
        RtspSessionId = 10190,
        RtspStreamUri = 10191,
        RtspTransport = 10192,
        RtspClientCseq = 193,
        RtspServerCseq = 194,

        // Protocol options
        TransferText = 53,
        ProxyTransferMode = 166,
        Crlf = 27,
        Range = 10007,
        ResumeFrom = 21,
        ResumeFromLarge = 30116,
        Curlu = 10282,
        CustomRequest = 10036,
        Filetime = 69,
        DirListOnly = 48,
        NoBody = 44,
        InFileSize = 14,
        InFileSizeLarge = 30115,
        Upload = 46,
        UploadBufferSize = 280,
        MimePost = 10269,
        MaxFileSize = 114,
        MaxFileSizeLarge = 30117,
        TimeCondition = 33,
        TimeValue = 34,
        TimeValueLarge = 30270,

        // Connection options
        Timeout = 13,
        TimeoutMs = 155,
        LowSpeedLimit = 19,
        LowSpeedTime = 20,
        MaxSendSpeedLarge = 30145,
        MaxRecvSpeedLarge = 30146,
        MaxConnects = 71,
        FreshConnect = 74,
        ForbidReuse = 75,
        MaxAge = 288,
        ConnectTimeout = 78,
        ConnectTimeoutMs = 156,
        IpResolve = 113,
        ConnectTo = 10243,
        ResolveAddresses = 10203,
        DnsShuffleIpv6 = 275,
        HappyEyeballsTimeoutMs = 271,
        Upkeep = 281,

        // SSL and security options
        SslCert = 10025,
        SslCertType = 10086,
        SslKey = 10087,
        SslKeyType = 10088,
        KeyPasswd = 10026,
        SslEnableAlpn = 226,
        SslEnableNpn = 225,
        SslEngine = 10089,
        SslEngineDefault = 90,
        SslVersion = 32,
        SslVerifyPeer = 64,
        SslVerifyHost = 81,
        SslVerifyStatus = 232,
        Cainfo = 10065,
        Issuercert = 10170,
        IssuercertBlob = 40295,
        Capath = 10097,
        Crlfile = 10169,
        CertInfo = 172,
        Pinnedpublickey = 10230,
        ProxySslCert = 10254,
        ProxySslCertType = 10255,
        ProxySslKey = 10256,
        ProxySslKeyType = 10257,
        ProxyKeyPasswd = 10258,
        ProxySslVersion = 250,
        ProxySslVerifyPeer = 248,
        ProxySslVerifyHost = 249,
        ProxyCainfo = 10246,
        ProxyCapath = 10247,
        ProxyCrlfile = 10260,
        ProxyPinnedPublicKey = 10263,
        SslCipherList = 10083,
        Tls13Ciphers = 10276,
        ProxySslCipherList = 10259,
        ProxyTls13Ciphers = 10277,
        SslSessionIdCache = 150,
        SslOptions = 216,
        ProxySslOptions = 261,
        KrbLevel = 10063,
        GssApiDelegation = 210,
        SslEcCurves = 10298,

        // SSH options
        SshAuthTypes = 151,
        SshCompression = 268,
        SshHostPublicKeyMd5 = 10162,
        SshHostPublicKeySha256 = 10311,
        SshPublicKeyFile = 10152,
        SshPrivateKeyFile = 10153,
        SshKnownhosts = 10183,
        SshKeyFunction = 20184,
        SshKeyData = 10185,

        // Other options
        Private = 10103,
        Share = 10100,
        NewFilePerms = 159,
        NewDirectoryPerms = 160,

        // Telnet options
        TelnetOptions = 10070,

        // =================================================================
        // curl-impersonate custom options (999+)
        // Type offsets: LONG=0, STRINGPOINT=10000, SLISTPOINT=10000
        // =================================================================

        /// <summary>
        /// Master option for setting impersonation target (STRINGPOINT).
        /// </summary>
        Impersonate = 10000 + 999,

        /// <summary>
        /// Base headers used by impersonated browser (SLISTPOINT).
        /// </summary>
        HttpBaseHeader = 10000 + 1000,

        /// <summary>
        /// TLS signature hash algorithms --signature-hashes (STRINGPOINT).
        /// </summary>
        SslSigHashAlgs = 10000 + 1001,

        /// <summary>
        /// Enable ALPS in TLS --alps (LONG).
        /// </summary>
        SslEnableAlps = 1002,

        /// <summary>
        /// Certificate compression algorithms --cert-compression (STRINGPOINT).
        /// </summary>
        SslCertCompression = 10000 + 1003,

        /// <summary>
        /// TLS session ticket extension (LONG).
        /// </summary>
        SslEnableTicket = 1004,

        /// <summary>
        /// HTTP/2 pseudo-header ordering --http2-pseudo-headers-order (STRINGPOINT).
        /// </summary>
        Http2PseudoHeadersOrder = 10000 + 1005,

        /// <summary>
        /// HTTP/2 settings frame configuration --http2-settings (STRINGPOINT).
        /// </summary>
        Http2Settings = 10000 + 1006,

        /// <summary>
        /// BoringSSL extension permutation --tls-permute-extensions (LONG).
        /// </summary>
        SslPermuteExtensions = 1007,

        /// <summary>
        /// HTTP/2 initial window update --http2-window-update (LONG).
        /// </summary>
        Http2WindowUpdate = 1008,

        /// <summary>
        /// HTTP/2 streams settings --http2-stream-weight combined (STRINGPOINT).
        /// </summary>
        Http2Streams = 10000 + 1010,

        /// <summary>
        /// TLS GREASE support --tls-grease (LONG).
        /// </summary>
        TlsGrease = 1011,

        /// <summary>
        /// TLS extension ordering --tls-extension-order (STRINGPOINT).
        /// </summary>
        TlsExtensionOrder = 10000 + 1012,

        /// <summary>
        /// Stream exclusiveness flag --http2-stream-exclusive (LONG).
        /// </summary>
        StreamExclusive = 1013,

        /// <summary>
        /// Disable TLS key usage check (LONG).
        /// </summary>
        TlsKeyUsageNoCheck = 1014,

        /// <summary>
        /// TLS signed certificate timestamps --tls-signed-cert-ts (LONG).
        /// </summary>
        TlsSignedCertTimestamps = 1015,

        /// <summary>
        /// TLS status request (LONG).
        /// </summary>
        TlsStatusRequest = 1016,

        /// <summary>
        /// Firefox delegated credentials --delegated-credentials (STRINGPOINT).
        /// </summary>
        TlsDelegatedCredentials = 10000 + 1017,

        /// <summary>
        /// Firefox record size limit --tls-record-size-limit (LONG).
        /// </summary>
        TlsRecordSizeLimit = 1018,

        /// <summary>
        /// Firefox key shares limit (LONG).
        /// </summary>
        TlsKeySharesLimit = 1019,

        /// <summary>
        /// Use new ALPS code point --tls-use-new-alps-codepoint (LONG).
        /// </summary>
        TlsUseNewAlpsCodepoint = 1020,

        /// <summary>
        /// Disable HTTP/2 priority bit --http2-no-server-push (LONG).
        /// </summary>
        Http2NoPriority = 1021,

        /// <summary>
        /// Prevent proxy credential reuse (LONG).
        /// </summary>
        ProxyCredentialNoReuse = 1022,

        /// <summary>
        /// Split cookies into separate headers --split-cookies (LONG).
        /// </summary>
        SplitCookies = 1023,

        /// <summary>
        /// Multipart form-data boundary style (LONG).
        /// </summary>
        FormBoundary = 1024,

        /// <summary>
        /// HTTP/3 pseudo-header ordering (STRINGPOINT).
        /// </summary>
        Http3PseudoHeadersOrder = 10000 + 1025,

        /// <summary>
        /// HTTP/3 settings configuration (STRINGPOINT).
        /// </summary>
        Http3Settings = 10000 + 1026,

        /// <summary>
        /// QUIC transport parameters (STRINGPOINT).
        /// </summary>
        QuicTransportParameters = 10000 + 1027,
    }
}
