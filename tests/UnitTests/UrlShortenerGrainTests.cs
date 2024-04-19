using Orleans.TestingHost;
using UnitTests.ClusterCollections;
using UnitTests.Fixtures;
using UrlShortenerWithOrleans.Interfaces;

namespace UnitTests
{
    [Collection(ClusterCollection.Name)]
    public class UrlShortenerGrainTests(ClusterFixture fixture)
    {
        private readonly TestCluster _cluster = fixture.Cluster;

        [Fact]
        public async Task UrlShortenerGrain_Should_Return_ShortenedUrl()
        {
            const string shortenedRouteSegment = "test";
            var urlShortenerGrain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

            var fullUrl = "test-full-url.com";
            await urlShortenerGrain.SetUrl(fullUrl);

            var shortenedUrlResult = await urlShortenerGrain.GetUrl();

            Assert.Equal(fullUrl, shortenedUrlResult);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData((string)null)]
        public async Task UrlShortenerGrain_Should_Not_Shorten_Empty_Url(string fullUrl)
        {
            string shortenedRouteSegment = Guid.NewGuid().ToString();

            var urlShortenerGrain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

            await urlShortenerGrain.SetUrl(fullUrl);

            var shortenedUrlResult = await urlShortenerGrain.GetUrl();

            Assert.Empty(shortenedUrlResult);
        }
    }
}
