using Orleans.TestingHost;

namespace UnitTests.Fixtures
{
    public sealed class ClusterFixture : IDisposable
    {
        public TestCluster Cluster { get; } = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<SiloConfiguration>()
            .Build();

        public ClusterFixture() => Cluster.Deploy();

        void IDisposable.Dispose() => Cluster.StopAllSilos();
    }

    file sealed class SiloConfiguration : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.UseLocalhostClustering();
            // for acceptance tests
            // siloBuilder.AddAdoNetGrainStorage("urls", "conn-string");
            siloBuilder.AddMemoryGrainStorage("urls");
        }
    }
}
