using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.TestingHost;
using UrlShortenerWithOrleans.Filters;
using UrlShortenerWithOrleans.Services;

namespace UnitTests.Fixtures
{
    public sealed class ClusterFixture : IDisposable
    {
        public TestCluster Cluster { get; } = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<SiloConfiguration>()
            .AddSiloBuilderConfigurator<HostConfiguration>()
            .Build();

        public ClusterFixture() => Cluster.Deploy();

        void IDisposable.Dispose() => Cluster.StopAllSilos();
    }

    file sealed class SiloConfiguration : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.UseLocalhostClustering();
            
            siloBuilder.AddAdoNetGrainStorage("urls", options =>
            {
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = "Data Source=localhost\\MSSQLSERVER05;Initial Catalog=UrlShortenerWithOrleans;User ID=sa;Password=p@ssw0rd;Integrated Security=False;Trusted_Connection=True;";
            });
          
            siloBuilder.AddIncomingGrainCallFilter<AuthenticationGrainFilter>();

            siloBuilder.ConfigureServices(static services =>
            {
                // required to enable AuthenticatedOnlyAttribute work with test cluster
                services.AddHttpContextAccessor();
                services.AddTransient<IAuthenticationService, AuthenticationService>();
            });
        }
    }

    file sealed class HostConfiguration : IHostConfigurator
    {
         public void Configure(IHostBuilder hostBuilder)
         {
            hostBuilder.ConfigureAppConfiguration(opt =>
            {
                opt.AddJsonFile("appsettings.json");
            });
         }
    }
}
