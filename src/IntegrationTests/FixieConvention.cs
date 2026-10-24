namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Fixie;

    /// <summary>
    /// Fixie 4 replacement for the Fixie 1.0 <c>Convention</c> base class. The old
    /// <c>Classes</c> / <c>ClassExecution</c> / <c>Parameters</c> DSL maps onto
    /// <see cref="IDiscovery"/> (class + method selection) and <see cref="IExecution"/>
    /// (instance construction and case invocation).
    ///
    /// Preserved behavior:
    ///  - <c>Classes.NameEndsWith("Tests")</c>      → <see cref="TestClasses"/>
    ///  - <c>ClassExecution.UsingFactory(factory)</c> → <see cref="Construct"/>
    ///  - <c>Parameters.Add(GetParameters)</c>      → <see cref="GetParameters"/>
    /// </summary>
    public abstract class FixieConvention : IDiscovery, IExecution
    {
        public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
        {
            return concreteClasses
                .Where(type => type.Name.EndsWith("Tests", StringComparison.Ordinal))
                .Where(IsTestClass);
        }

        public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
        {
            return publicMethods;
        }

        public abstract Task Run(TestSuite testSuite);

        /// <summary>
        /// Additional class filter contributed by derived conventions. Replaces the chained
        /// <c>Classes.Where(...)</c> calls of the Fixie 1.0 DSL; each override must call
        /// <c>base.IsTestClass(type)</c> so that the filters compose exactly as the old
        /// chained expression did.
        /// </summary>
        protected virtual bool IsTestClass(Type type)
        {
            return true;
        }

        protected abstract ICustomization AutoFixtureCustomization { get; }

        protected IFixture AutoFixture
        {
            get { return AutoFixtureFactory.Instance.BuildWith(AutoFixtureCustomization); }
        }

        /// <summary>Old <c>Parameters.Add(GetParameters)</c> source.</summary>
        protected IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            return method.ResolveParametersWith(AutoFixture);
        }

        /// <summary>Old <c>ClassExecution.UsingFactory(CustomFactory)</c>.</summary>
        protected object Construct(Type type)
        {
            return new SpecimenContext(AutoFixture).Resolve(type);
        }
    }
}
