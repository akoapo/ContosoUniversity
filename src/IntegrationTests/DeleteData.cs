namespace ContosoUniversity.IntegrationTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Respawn;
    using Respawn.Graph;

    /// <summary>
    /// Per-test data wipe. Fixie 4 removed the <c>FixtureBehavior</c>/<c>ClassBehavior</c>
    /// interfaces, so the behavior is exposed as a reusable async helper invoked from both
    /// of the original wrap points: per-class (<see cref="FixturePerClassConvention"/>) and
    /// per-case (<see cref="FixturePerMethodConvention"/>).
    /// </summary>
    public static class DeleteData
    {
        private static readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);

        // Respawn 7 replacement for the old static Respawn 0.1 Checkpoint — created once and
        // cached for the process lifetime, exactly as the checkpoint was.
        private static Respawner respawner;

        public static async Task DeleteAllDataAsync()
        {
            var localDbInstance = LocalDbFactory.Instance;
            var builder = localDbInstance.CreateConnectionStringBuilder();
            builder.InitialCatalog = localDbInstance.Name;

            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();

                var current = await GetRespawnerAsync(connection);

                await current.ResetAsync(connection);
            }
        }

        private static async Task<Respawner> GetRespawnerAsync(SqlConnection connection)
        {
            if (respawner != null)
            {
                return respawner;
            }

            await gate.WaitAsync();

            try
            {
                if (respawner == null)
                {
                    respawner = await Respawner.CreateAsync(
                        connection,
                        new RespawnerOptions
                        {
                            TablesToIgnore = new[]
                            {
                                new Table("sysdiagrams"),
                            },
                            SchemasToExclude = new[]
                            {
                                "RoundhousE"
                            }
                        });
                }
            }
            finally
            {
                gate.Release();
            }

            return respawner;
        }
    }
}
