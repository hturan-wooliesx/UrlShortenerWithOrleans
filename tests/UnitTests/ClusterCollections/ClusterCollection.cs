using UnitTests.Fixtures;

namespace UnitTests.ClusterCollections
{
    [CollectionDefinition(Name)]
    public sealed class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = nameof(ClusterCollection);
    }
}
