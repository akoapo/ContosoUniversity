namespace ContosoUniversity.IntegrationTests
{
    using System;
    using ContosoUniversity.DAL;
    using ContosoUniversity.Infrastructure.Tags;
    using ContosoUniversity.Logging;
    using FluentValidation;
    using HtmlTags.Conventions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Replaces the old StructureMap <c>IoC.Initialize()</c> based factory. The
    /// DependencyResolution/ bootstrapper no longer exists in ContosoUniversity — DI is
    /// composed in <see cref="ContosoUniversity.Program"/> — so the registrations the
    /// integration tests exercise are reproduced here against
    /// Microsoft.Extensions.DependencyInjection, built once per process via <see cref="Lazy{T}"/>.
    /// </summary>
    public class IntegrationTestContainerFactory
    {
        private static readonly Lazy<InnerFactory> LazyInnerFactory = new Lazy<InnerFactory>(() => new InnerFactory());

        /// <summary>The root service provider (once per process).</summary>
        public static IServiceProvider Services
        {
            get { return LazyInnerFactory.Value.Services; }
        }

        private class InnerFactory
        {
            public InnerFactory()
            {
                var connString = LocalDbFactory.Instance.CreateConnectionStringBuilder();
                connString.InitialCatalog = "ContosoUniversity";

                var services = new ServiceCollection();

                // ---- Logging infrastructure (Program.cs gets this from the web host) ----
                services.AddLogging();

                // ---- Data access: Program.cs registers the EF6 DbInterception instances as
                //      singletons so their state is process-wide; same here. ----
                services.AddSingleton<SchoolInterceptorLogging>();
                services.AddSingleton<SchoolInterceptorTransientErrors>();

                // The old test registration was
                //   For<SchoolContext>().Use(() => new SchoolContext(connString)).Transient()
                // inside a StructureMap nested container; the EF Core equivalent is a scoped
                // DbContext resolved from the per-test IServiceScope (see TestServiceScope).
                services.AddDbContext<SchoolContext>((sp, o) =>
                {
                    // No EnableRetryOnFailure — matches Program.cs (the tests drive
                    // BeginTransaction()/CloseTransaction() explicitly).
                    o.UseSqlServer(connString.ToString());
                    o.AddInterceptors(
                        sp.GetRequiredService<SchoolInterceptorLogging>(),
                        sp.GetRequiredService<SchoolInterceptorTransientErrors>());
                });

                // ---- Mapping ----
                services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ContosoUniversity.Program).Assembly));

                // ---- Messaging (handlers the tests send commands through) ----
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ContosoUniversity.Program>());

                // ---- Validation ----
                services.AddValidatorsFromAssemblyContaining<ContosoUniversity.Program>();

                // ---- HtmlTags conventions ----
                services.AddSingleton<HtmlConventionLibrary>(HtmlConventionLibraryFactory.Create());

                // ---- Application logging facade ----
                services.AddSingleton<IAppLogger, Logger>();

                // Program.cs additionally registers the MVC pipeline (controllers, filters, model
                // binders, Razor view locations, FluentValidation auto-validation adapters) and
                // AddHttpContextAccessor. Those are ASP.NET Core request-pipeline services with no
                // meaning in this class-library test host and no test resolves them, so they are
                // intentionally not reproduced — nothing the tests exercise is dropped.
                Services = services.BuildServiceProvider();
            }

            public IServiceProvider Services { get; private set; }
        }
    }
}
