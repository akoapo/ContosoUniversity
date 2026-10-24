namespace ContosoUniversity.IntegrationTests
{
    using Fixie;

    /// <summary>
    /// Fixie 4 entry point. Fixie 1.0 auto-discovered every <c>Convention</c> subclass; Fixie 4
    /// requires explicit registration, so BOTH conventions are registered here — omitting either
    /// would silently drop every test class selected by that convention's class filter.
    /// </summary>
    public class TestProject : ITestProject
    {
        public void Configure(TestConfiguration configuration, TestEnvironment environment)
        {
            var fixturePerClass = new FixturePerClassConvention();
            configuration.Conventions.Add(fixturePerClass, fixturePerClass);

            var fixturePerMethod = new FixturePerMethodConvention();
            configuration.Conventions.Add(fixturePerMethod, fixturePerMethod);
        }
    }
}
