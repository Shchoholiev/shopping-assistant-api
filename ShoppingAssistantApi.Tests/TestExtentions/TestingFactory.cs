using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using ShoppingAssistantApi.Persistance.PersistanceExtentions;

namespace ShoppingAssistantApi.Tests.TestExtentions;

public class TestingFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    private readonly MongoDbRunner _runner = MongoDbRunner.Start();

    private bool _isDataInitialaized = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dbConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "ConnectionStrings:MongoDb", _runner.ConnectionString }
            })
            .Build();

            config.AddConfiguration(dbConfig);
        });
    }

    public async Task InitialaizeData()
    {
        if (!_isDataInitialaized)
        {
            _isDataInitialaized = true;
            using var scope = Services.CreateScope();
            var initialaizer = new DbInitialaizer(scope.ServiceProvider);
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            await initialaizer.InitialaizeDb(cancellationToken);
        }
    }

    protected override void Dispose(bool disposing)
    {
        _runner.Dispose();
        base.Dispose(disposing);
    }
}