using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.TestingHost;
using UnitTests.ClusterCollections;
using UnitTests.Fixtures;
using UrlShortenerWithOrleans.Interfaces;
using UrlShortenerWithOrleans.Services;

namespace AcceptanceTests
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

            AuthenticateGrainCall();

            var fullUrl = "test-full-url.com";
            await urlShortenerGrain.SetUrl(fullUrl);

            var shortenedUrlResult = await urlShortenerGrain.GetUrl();

            Assert.Equal(fullUrl, shortenedUrlResult);
        }

        [Fact]
        public async Task UrlShortenerGrain_Should_Not_Call_ShortenUrl_When_Not_Valid_Token()
        {
            const string shortenedRouteSegment = "test";
            var urlShortenerGrain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

            // AuthenticateGrainCall();

            var fullUrl = "test-full-url.com";

            await Assert.ThrowsAsync<Exception>(() => urlShortenerGrain.SetUrl(fullUrl));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData((string)null)]
        public async Task UrlShortenerGrain_Should_Not_Shorten_Empty_Url(string fullUrl)
        {
            string shortenedRouteSegment = Guid.NewGuid().ToString();

            var urlShortenerGrain = _cluster.GrainFactory.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

            AuthenticateGrainCall();

            await urlShortenerGrain.SetUrl(fullUrl);

            var shortenedUrlResult = await urlShortenerGrain.GetUrl();

            Assert.Empty(shortenedUrlResult);
        }

        private void AuthenticateGrainCall()
        {
            // get the primary silo handle
            InProcessSiloHandle inProcessSiloHandle = (InProcessSiloHandle)_cluster.Primary;

            // read injected authentication service
            var injectedService = inProcessSiloHandle.SiloHost.Services.GetService<IAuthenticationService>();

            // issue a new valid token
            var issuedToken = injectedService?.Authenticate();

            // set request context
            RequestContext.Set("bearerToken", "Bearer " + issuedToken);
        }
    }
}
