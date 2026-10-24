namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Test-owned replacement for the deleted <c>StructureMapDependencyScope</c>.
    /// It wraps a root <see cref="IServiceProvider"/> plus an optional nested
    /// <see cref="IServiceScope"/> and preserves the old
    /// "CurrentNestedContainer ?? Container" resolution fallback.
    /// </summary>
    public class TestServiceScope : IDisposable
    {
        private readonly IServiceScopeFactory scopeFactory;

        public TestServiceScope(IServiceProvider rootServiceProvider)
        {
            if (rootServiceProvider == null)
            {
                throw new ArgumentNullException("rootServiceProvider");
            }

            RootServiceProvider = rootServiceProvider;
            scopeFactory = rootServiceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        /// <summary>Equivalent of the old <c>Container</c> property.</summary>
        public IServiceProvider RootServiceProvider { get; private set; }

        /// <summary>Equivalent of the old <c>CurrentNestedContainer</c> property.</summary>
        public IServiceScope CurrentNestedScope { get; private set; }

        /// <summary>The old <c>CurrentNestedContainer ?? Container</c> fallback.</summary>
        public IServiceProvider ServiceProvider
        {
            get
            {
                return CurrentNestedScope != null
                    ? CurrentNestedScope.ServiceProvider
                    : RootServiceProvider;
            }
        }

        /// <summary>
        /// Equivalent of <c>CreateNestedContainer()</c>: idempotent — if a nested scope
        /// already exists it is kept.
        /// </summary>
        public IServiceScope CreateNestedScope()
        {
            if (CurrentNestedScope != null)
            {
                return CurrentNestedScope;
            }

            CurrentNestedScope = scopeFactory.CreateScope();
            return CurrentNestedScope;
        }

        public void DisposeNestedScope()
        {
            if (CurrentNestedScope != null)
            {
                CurrentNestedScope.Dispose();
                CurrentNestedScope = null;
            }
        }

        public void Dispose()
        {
            DisposeNestedScope();
        }

        public T GetInstance<T>()
        {
            return (T)GetInstance(typeof(T));
        }

        /// <summary>
        /// Mirrors the old <c>DoGetInstance</c>: an abstraction must be registered (StructureMap
        /// threw <c>StructureMapConfigurationException</c> for an unknown plugin type, so an
        /// unresolvable abstraction fails fast and names the service, rather than returning null
        /// and surfacing as a NullReferenceException further downstream — use
        /// <see cref="TryGetInstance"/> for the null-when-unregistered behavior), while concrete
        /// types are built even when unregistered (StructureMap's auto-concrete-resolution,
        /// here ActivatorUtilities).
        /// </summary>
        public object GetInstance(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            var provider = ServiceProvider;

            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return provider.GetRequiredService(serviceType);
            }

            return provider.GetService(serviceType)
                   ?? ActivatorUtilities.CreateInstance(provider, serviceType);
        }

        /// <summary>Mirrors the old <c>TryGetInstance</c>: never throws, null when unregistered.</summary>
        public object TryGetInstance(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        /// <summary>Mirrors the old <c>GetServices</c>/<c>DoGetAllInstances</c>.</summary>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }
    }
}
