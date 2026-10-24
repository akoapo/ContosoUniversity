namespace ContosoUniversity.DAL
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// EF Core replacement for the EF6 <c>IDbCommandInterceptor</c> based SQL logging.
    /// Registered with <c>optionsBuilder.AddInterceptors(...)</c>.
    /// </summary>
    public class SchoolInterceptorLogging : DbCommandInterceptor
    {
        private readonly ILogger<SchoolInterceptorLogging> _logger;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public SchoolInterceptorLogging(ILogger<SchoolInterceptorLogging> logger)
        {
            _logger = logger;
        }

        private void Start()
        {
            _stopwatch.Restart();
        }

        private void Executed(string method, DbCommand command)
        {
            _stopwatch.Stop();
            TraceApi("SQL Database", method, _stopwatch.Elapsed, "Command: {0}: ", command.CommandText);
        }

        private void Failed(DbCommand command, Exception exception)
        {
            _stopwatch.Stop();
            _logger.LogError(exception, "Error executing command: {CommandText}", command.CommandText);
        }

        private void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars)
        {
            var properties = string.Format(fmt, vars);
            var message = string.Concat("Component:", componentName, ";Method:", method, ";Timespan:",
                timespan.ToString(), ";Properties:", properties);
            _logger.LogInformation("{Message}", message);
        }

        // --- scalar ---------------------------------------------------------

        public override InterceptionResult<object> ScalarExecuting(DbCommand command,
            CommandEventData eventData, InterceptionResult<object> result)
        {
            Start();
            return base.ScalarExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            Start();
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object result)
        {
            Executed("SchoolInterceptor.ScalarExecuted", command);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override ValueTask<object> ScalarExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, object result, CancellationToken cancellationToken = default)
        {
            Executed("SchoolInterceptor.ScalarExecuted", command);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        // --- non query ------------------------------------------------------

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result)
        {
            Start();
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Start();
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        {
            Executed("SchoolInterceptor.NonQueryExecuted", command);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override ValueTask<int> NonQueryExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            Executed("SchoolInterceptor.NonQueryExecuted", command);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        // --- reader ---------------------------------------------------------

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Start();
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Start();
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData,
            DbDataReader result)
        {
            Executed("SchoolInterceptor.ReaderExecuted", command);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            Executed("SchoolInterceptor.ReaderExecuted", command);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        // --- failures (EF6 reported failures through interceptionContext.Exception) ------

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            Failed(command, eventData.Exception);
            base.CommandFailed(command, eventData);
        }

        public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Failed(command, eventData.Exception);
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }
    }
}
