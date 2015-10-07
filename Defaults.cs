namespace hcClient
{
    static class Defaults
    {
        public const string SettingsFileName = @"hcClient.ini";

        public const string LocalServer = "127.0.0.1";
        public const int TcpPort = 7231;
        public const int IoBufferSize = 255;
        public const double ReconnectTimerInterval = 5000;
        public const double WatchdogTimerInterval = 60000;
        public const int DataCacheSize = 1024;
    }
}
