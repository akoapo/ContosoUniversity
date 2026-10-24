namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.IO;
    using MartinCostello.SqlLocalDb;
    using Microsoft.Data.SqlClient;
    using roundhouse;
    using Logger = roundhouse.infrastructure.logging.Logger;

    public static class LocalDbFactory
    {
        private static readonly Lazy<LocalDbInstance> _instance = new Lazy<LocalDbInstance>(ValueFactory);

        public static LocalDbInstance Instance
        {
            get { return _instance.Value; }
        }

        private static LocalDbInstance ValueFactory()
        {
            // System.Data.SqlLocalDb 1.14's static TemporarySqlLocalDbInstance.Create(true) became
            // MartinCostello.SqlLocalDb 6.0.0's ISqlLocalDbApi.CreateTemporaryInstance(deleteFiles).
            // The api and the temporary instance are rooted in the returned LocalDbInstance so the
            // instance is not deleted by its finalizer while the test process is alive.
            var api = new SqlLocalDbApi();
            var temporaryInstance = api.CreateTemporaryInstance(deleteFiles: true);

            var instance = new LocalDbInstance(api, temporaryInstance);

            var builder = instance.CreateConnectionStringBuilder();

            var databaseName = instance.Name;

            using (var conn = instance.CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                var fileName = Path.Combine(Environment.CurrentDirectory, databaseName + ".mdf");
                cmd.CommandText = string.Format("CREATE DATABASE [{0}] on (name='{0}', filename='{1}')", databaseName, fileName);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine(instance.Name);

            builder.InitialCatalog = databaseName;

            var connectionString = builder.ConnectionString;

            var migrate = new Migrate().Set(c =>
            {
                c.ConnectionString = connectionString;
                c.DatabaseName = databaseName;
                c.Silent = true;
                c.WithTransaction = true;
                c.SqlFilesDirectory = Path.Combine(Environment.CurrentDirectory, "DatabaseMigration");
            });
            migrate.Run();

            return instance;
        }

        /// <summary>
        /// Keeps the public surface the old <c>TemporarySqlLocalDbInstance</c> exposed to
        /// <see cref="DeleteData"/>, <see cref="IntegrationTestContainerFactory"/> and
        /// <see cref="TestContextFixture"/> (<c>Name</c>, <c>CreateConnectionStringBuilder()</c>,
        /// <c>CreateConnection()</c>) now that those members are extension methods on
        /// <see cref="ISqlLocalDbInstanceInfo"/> in MartinCostello.SqlLocalDb 6.0.0.
        /// </summary>
        public sealed class LocalDbInstance : IDisposable
        {
            private readonly SqlLocalDbApi api;
            private readonly TemporarySqlLocalDbInstance temporaryInstance;

            internal LocalDbInstance(SqlLocalDbApi api, TemporarySqlLocalDbInstance temporaryInstance)
            {
                this.api = api;
                this.temporaryInstance = temporaryInstance;
            }

            public string Name
            {
                get { return temporaryInstance.Name; }
            }

            public SqlConnectionStringBuilder CreateConnectionStringBuilder()
            {
                return InstanceInfo.CreateConnectionStringBuilder();
            }

            public SqlConnection CreateConnection()
            {
                return InstanceInfo.CreateConnection();
            }

            private ISqlLocalDbInstanceInfo InstanceInfo
            {
                get { return api.GetInstanceInfo(temporaryInstance.Name); }
            }

            public void Dispose()
            {
                temporaryInstance.Dispose();
                api.Dispose();
            }
        }

        private class ConsoleLogger : Logger
        {
            public void log_a_debug_event_containing(string message, params object[] args)
            {
                Console.WriteLine(message, args);
            }

            public void log_an_info_event_containing(string message, params object[] args)
            {
                Console.WriteLine(message, args);
            }

            public void log_a_warning_event_containing(string message, params object[] args)
            {
                Console.WriteLine(message, args);
            }

            public void log_an_error_event_containing(string message, params object[] args)
            {
                Console.WriteLine(message, args);
            }

            public void log_a_fatal_event_containing(string message, params object[] args)
            {
                Console.WriteLine(message, args);
            }

            public object underlying_type { get { return typeof (ConsoleLogger); } }
        }
    }
}
