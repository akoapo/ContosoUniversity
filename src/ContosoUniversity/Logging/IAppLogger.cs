namespace ContosoUniversity.Logging
{
    using System;

    /// <summary>
    /// The application's logging facade. Renamed from <c>ILogger</c> to <c>IAppLogger</c> so it no
    /// longer collides with <see cref="Microsoft.Extensions.Logging.ILogger"/>; the member surface
    /// is unchanged from the original interface.
    /// </summary>
    public interface IAppLogger
    {
        void Information(string message);
        void Information(string fmt, params object[] vars);
        void Information(Exception exception, string fmt, params object[] vars);

        void Warning(string message);
        void Warning(string fmt, params object[] vars);
        void Warning(Exception exception, string fmt, params object[] vars);

        void Error(string message);
        void Error(string fmt, params object[] vars);
        void Error(Exception exception, string fmt, params object[] vars);

        void TraceApi(string componentName, string method, TimeSpan timespan);
        void TraceApi(string componentName, string method, TimeSpan timespan, string properties);
        void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars);
    }
}
