using System;
using AutoFixture.Kernel;

namespace ContosoUniversity.IntegrationTests
{
    /// <summary>
    /// AutoFixture specimen builder that resolves services from the current test scope
    /// (previously the StructureMap nested container).
    /// </summary>
    public class ContainerBuilder : ISpecimenBuilder
    {
        private readonly TestServiceScope scope;

        public ContainerBuilder(TestServiceScope scope)
        {
            this.scope = scope;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var type = request as Type;

            if (type == null || type.IsPrimitive)
            {
                return new NoSpecimen();
            }

            // TryGetInstance semantics: GetService on the nested scope (root fallback),
            // never GetRequiredService — unresolvable services must yield NoSpecimen.
            var service = scope.TryGetInstance(type);

            return service ?? new NoSpecimen();
        }
    }
}
