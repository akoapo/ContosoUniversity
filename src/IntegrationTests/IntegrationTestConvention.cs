namespace ContosoUniversity.IntegrationTests
{
    using System;
    using AutoFixture;

    public abstract class IntegrationTestConvention : FixieConvention
    {
        /// <summary>Old <c>Classes.AreInIntegrationTestNamespace()</c>.</summary>
        protected override bool IsTestClass(Type type)
        {
            return base.IsTestClass(type) && type.IsInIntegrationTestNamespace();
        }

        protected override ICustomization AutoFixtureCustomization
        {
            get { return new IntegrationTestsFixtureCustomization(); }
        }
    }
}
