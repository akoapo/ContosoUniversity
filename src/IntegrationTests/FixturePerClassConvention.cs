namespace ContosoUniversity.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Fixie;

    /// <summary>
    /// Fixie 1.0 equivalent:
    /// <code>
    /// Classes.ConstructorHasArguments();
    /// ClassExecution.CreateInstancePerClass().Wrap&lt;DeleteData&gt;();
    /// </code>
    /// One instance is constructed per test class, and the data wipe wraps the whole class run.
    /// </summary>
    public class FixturePerClassConvention : IntegrationTestConvention
    {
        protected override bool IsTestClass(Type type)
        {
            return base.IsTestClass(type) && type.ConstructorHasArguments();
        }

        public override async Task Run(TestSuite testSuite)
        {
            foreach (var testClass in testSuite.TestClasses)
            {
                // ClassBehavior wrap point: once around the entire class run.
                await DeleteData.DeleteAllDataAsync();

                object instance;

                try
                {
                    instance = Construct(testClass.Type);
                }
                catch (Exception exception)
                {
                    await testClass.Fail(exception);
                    continue;
                }

                try
                {
                    foreach (var test in testClass.Tests)
                    {
                        foreach (var parameters in GetParameters(test.Method))
                        {
                            await test.Run(instance, parameters);
                        }
                    }
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
