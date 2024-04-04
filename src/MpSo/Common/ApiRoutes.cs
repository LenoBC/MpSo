namespace MpSo.Common;

public static class ApiRoutes
{
    public const string Root = "api";
    public const string Version = "v{version:apiVersion}";
    public const string Base = Root + "/" + Version;

    public static class Tags
    {
        public const string Get = Base + "/tags";
        public const string RefreshFromSoApi = Base + "/tags/refresh-from-so-api";
    }
}
