namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.Kernel;

    /// <summary>
    /// Fixie 4 replacements for the Fixie 1.0 <c>Fixie.Conventions.ClassExpression</c>
    /// extension methods. The old DSL filtered an expression object; the Fixie 4
    /// <see cref="Fixie.IDiscovery"/> model filters <see cref="Type"/> sequences, so each
    /// former <c>ClassExpression</c> helper is re-expressed as a <see cref="Type"/> predicate
    /// (plus a sequence-level overload preserving the original "filter the candidate classes"
    /// usage).
    /// </summary>
    public static class FixieExtensions
    {
        public static IEnumerable<object[]> ResolveParametersWith(this MethodInfo methodInfo, IFixture fixture)
        {
            return Enumerable.Repeat(
                methodInfo
                .GetParameters()
                .Select(parameterInfo => new SpecimenContext(fixture).Resolve(parameterInfo))
                .ToArray(),
                1);
        }

        /// <summary>
        /// Former <c>ClassExpression.IsInNamespaceOf&lt;T&gt;()</c>, which delegated to Fixie 1.0's
        /// <c>InTheSameNamespaceAs(typeof(T))</c>: the class' namespace equals, or is nested
        /// underneath, the namespace of <typeparamref name="T"/>.
        /// </summary>
        public static bool IsInNamespaceOf<T>(this Type type)
        {
            var target = typeof(T).Namespace;

            if (target == null)
            {
                return type.Namespace == null;
            }

            return type.Namespace != null
                   && (type.Namespace == target || type.Namespace.StartsWith(target + ".", StringComparison.Ordinal));
        }

        public static IEnumerable<Type> IsInNamespaceOf<T>(this IEnumerable<Type> types)
        {
            return types.Where(type => type.IsInNamespaceOf<T>());
        }

        /// <summary>Former <c>ClassExpression.AreInIntegrationTestNamespace()</c>.</summary>
        public static bool IsInIntegrationTestNamespace(this Type type)
        {
            return type.Namespace != null && type.Namespace.Contains("IntegrationTests");
        }

        public static IEnumerable<Type> AreInIntegrationTestNamespace(this IEnumerable<Type> types)
        {
            return types.Where(type => type.IsInIntegrationTestNamespace());
        }

        /// <summary>Former <c>ClassExpression.ConstructorHasArguments()</c>.</summary>
        public static bool ConstructorHasArguments(this Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Any(x => x.GetParameters().Any());
        }

        public static IEnumerable<Type> ConstructorHasArguments(this IEnumerable<Type> types)
        {
            return types.Where(type => type.ConstructorHasArguments());
        }

        /// <summary>Former <c>ClassExpression.ConstructorDoesntHaveArguments()</c>.</summary>
        public static bool ConstructorDoesntHaveArguments(this Type type)
        {
            return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .All(x => x.GetParameters().Length == 0);
        }

        public static IEnumerable<Type> ConstructorDoesntHaveArguments(this IEnumerable<Type> types)
        {
            return types.Where(type => type.ConstructorDoesntHaveArguments());
        }
    }
}
