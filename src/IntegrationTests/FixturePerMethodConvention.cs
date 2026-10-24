namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Fixie;

    /// <summary>
    /// Fixie 1.0 equivalent:
    /// <code>
    /// Classes.ConstructorDoesntHaveArguments();
    /// ClassExecution.CreateInstancePerCase();
    /// FixtureExecution.Wrap&lt;DeleteData&gt;();
    /// </code>
    /// A fresh instance is constructed for every case, and the data wipe wraps EACH case
    /// (the old per-fixture wrap point, where a "fixture" was one instance + one case).
    /// </summary>
    public class FixturePerMethodConvention : IntegrationTestConvention
    {
        protected override bool IsTestClass(Type type)
        {
            return base.IsTestClass(type) && type.ConstructorDoesntHaveArguments();
        }

        public override async Task Run(TestSuite testSuite)
        {
            foreach (var testClass in testSuite.TestClasses)
            {
                foreach (var test in testClass.Tests)
                {
                    foreach (var parameters in GetParameters(test.Method))
                    {
                        object instance;

                        try
                        {
                            instance = Construct(testClass.Type);
                        }
                        catch (Exception exception)
                        {
                            await test.Fail(parameters, exception, null);
                            continue;
                        }

                        // FixtureBehavior wrap point: once per case, AFTER the instance is
                        // constructed (Fixie 1.0 created the instance for the case and then ran
                        // the fixture behaviors around it), BEFORE the case executes.
                        await DeleteData.DeleteAllDataAsync();

                        try
                        {
                            await test.Run(instance, parameters);
                        }
                        finally
                        {
                            var disposable = instance as IDisposable;

                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }
                    }
                }
            }
        }
    }
}
