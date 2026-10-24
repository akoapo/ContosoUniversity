namespace ContosoUniversity.Logging
{
    using System;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Reimplementation of the original <c>Logger</c> on top of
    /// <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/> instead of
    /// <c>System.Diagnostics.Trace</c>. Message text, severities and the TraceApi
    /// "Component:...;Method:...;Timespan:...;Properties:..." format are preserved exactly.
    /// </summary>
    public class Logger : IAppLogger
    {
        private readonly ILogger<Logger> _logger;

        public Logger(ILogger<Logger> logger)
        {
            _logger = logger;
        }

        public void Information(string message)
        {
            _logger.LogInformation("{Message}", message);
        }

        public void Information(string fmt, params object[] vars)
        {
            _logger.LogInformation("{Message}", Format(fmt, vars));
        }

        public void Information(Exception exception, string fmt, params object[] vars)
        {
            _logger.LogInformation(exception, "{Message}", FormatExceptionMessage(exception, fmt, vars));
        }

        public void Warning(string message)
        {
            _logger.LogWarning("{Message}", message);
        }

        public void Warning(string fmt, params object[] vars)
        {
            _logger.LogWarning("{Message}", Format(fmt, vars));
        }

        public void Warning(Exception exception, string fmt, params object[] vars)
        {
            _logger.LogWarning(exception, "{Message}", FormatExceptionMessage(exception, fmt, vars));
        }

        public void Error(string message)
        {
            _logger.LogError("{Message}", message);
        }

        public void Error(string fmt, params object[] vars)
        {
            _logger.LogError("{Message}", Format(fmt, vars));
        }

        public void Error(Exception exception, string fmt, params object[] vars)
        {
            _logger.LogError(exception, "{Message}", FormatExceptionMessage(exception, fmt, vars));
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan)
        {
            TraceApi(componentName, method, timespan, string.Empty);
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars)
        {
            TraceApi(componentName, method, timespan, string.Format(fmt, vars));
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan, string properties)
        {
            string message = string.Concat("Component:", componentName, ";Method:", method, ";Timespan:",
                timespan.ToString(), ";Properties:", properties);
            _logger.LogInformation("{Message}", message);
        }

        private static string Format(string fmt, object[] vars)
        {
            return vars == null || vars.Length == 0 ? fmt : string.Format(fmt, vars);
        }

        private static string FormatExceptionMessage(Exception exception, string fmt, object[] vars)
        {
            var sb = new StringBuilder();
            sb.Append(Format(fmt, vars));
            sb.Append(" Exception: ");
            sb.Append(exception.ToString());
            return sb.ToString();
        }
    }
}
