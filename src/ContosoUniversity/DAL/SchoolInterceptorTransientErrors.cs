namespace ContosoUniversity.DAL
{
    using System;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// EF Core replacement for the EF6 transient-error simulation interceptor. When a query is
    /// executed whose first parameter is the sentinel value "%Throw%" the parameters are rewritten
    /// to "%an%" and the first four executions fail with a transient error - exactly as the
    /// original sample did.
    /// <para>
    /// Note that the original did NOT retry these errors: DAL/SchoolConfiguration.cs explicitly
    /// installed EF6's non-retrying DefaultExecutionStrategy, so the simulated failure surfaced to
    /// the caller. The EF Core port keeps that behavior (no EnableRetryOnFailure in Program.cs),
    /// which is also required because the global MvcTransactionFilter opens a user-initiated
    /// transaction on every action.
    /// </para>
    /// <para>
    /// Registered as a SINGLETON and passed to <c>optionsBuilder.AddInterceptors(...)</c> so that
    /// <see cref="MaxTransientErrors"/> is counted process-wide, as it was under EF6's
    /// <c>DbInterception.Add</c>.
    /// </para>
    /// </summary>
    public class SchoolInterceptorTransientErrors : DbCommandInterceptor
    {
        private const int MaxTransientErrors = 4;

        private readonly ILogger<SchoolInterceptorTransientErrors> _logger;
        private int _counter = 0;

        public SchoolInterceptorTransientErrors(ILogger<SchoolInterceptorTransientErrors> logger)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            ThrowIfRequested(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ThrowIfRequested(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void ThrowIfRequested(DbCommand command)
        {
            var throwTransientErrors = false;
            if (command.Parameters.Count > 0 && command.Parameters[0].Value?.ToString() == "%Throw%")
            {
                throwTransientErrors = true;
                command.Parameters[0].Value = "%an%";
                if (command.Parameters.Count > 1)
                {
                    command.Parameters[1].Value = "%an%";
                }
            }

            if (throwTransientErrors && _counter < MaxTransientErrors)
            {
                _logger.LogInformation("Returning transient error for command: {CommandText}", command.CommandText);
                _counter++;

                // NOTE: the EF6 version fabricated a SqlException (error 20) through private
                // reflection. Microsoft.Data.SqlClient keeps those constructors internal, so a
                // TimeoutException is used instead - it is classified as transient by
                // SqlServerTransientExceptionDetector, i.e. the same transient classification the
                // original SqlException 20 carried.
                throw new TimeoutException("Simulated transient error (EF6 SqlException 20 equivalent).");
            }
        }
    }
}
