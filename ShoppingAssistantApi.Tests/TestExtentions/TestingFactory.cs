using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Tests.TestExtentions;

public class TestingFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
{
    private MongoDbRunner? _runner;

    private bool _isDataInitialaized = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Mongo2Go is not supported on ARM64 so we need to use a real MongoDB instance
        Console.WriteLine($"[ARCH]: {RuntimeInformation.ProcessArchitecture}");

        var connectionString = string.Empty;
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            connectionString = "mongodb+srv://api:pUe2dLT8llwEgwzq@cluster0.3q6mxmw.mongodb.net/?retryWrites=true&w=majority";
        }
        else 
        {
            _runner = MongoDbRunner.Start();
            connectionString = _runner.ConnectionString;
        }

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dbConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "ConnectionStrings:MongoDb", connectionString }
                })
                .Build();

            config.AddConfiguration(dbConfig);
        });
    }

    public void InitialaizeDatabase()
    {
        if (_isDataInitialaized) return;

        using var scope = Services.CreateScope();
        var mongodbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        var initialaizer = new DbInitializer(mongodbContext);
        initialaizer.InitializeDb();

        _isDataInitialaized = true;
    }

    protected override void Dispose(bool disposing)
    {
        _runner?.Dispose();
        base.Dispose(disposing);
    }
}