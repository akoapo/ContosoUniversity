namespace ContosoUniversity.IntegrationTests
{
    using AutoFixture;

    public class IntegrationTestsFixtureCustomization : AutoFixtureCustomization
    {
        protected override void CustomizeFixture(IFixture fixture)
        {
            var scope = new TestServiceScope(IntegrationTestContainerFactory.Services);
            var contextFixture = new TestContextFixture(scope);
            contextFixture.SetUp();

            //fixture.Customizations.Add(scope.GetInstance<IntegrationTestDefaultValueBuilder>());
            fixture.Register(() => contextFixture);
            fixture.Customizations.Add(new ContainerBuilder(scope)); // always last
        }
    }
}
