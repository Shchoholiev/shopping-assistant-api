using ShoppingAssistantApi.Application.ApplicationExtentions;
using ShoppingAssistantApi.Persistance.PersistanceExtentions;
using ShoppingAssistantApi.Infrastructure.InfrastructureExtentions;
using ShoppingAssistantApi.Api.ApiExtentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appConfig = Environment.GetEnvironmentVariable("APP_CONFIG") ?? builder.Configuration.GetConnectionString("AppConfig");
builder.Configuration.AddAzureAppConfiguration(appConfig);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddJWTTokenAuthentication(builder.Configuration);
builder.Services.AddMapper();
builder.Services.AddInfrastructure();
builder.Services.AddServices();
builder.Services.AddHttpClient(builder.Configuration);
builder.Services.AddGraphQl();
builder.Services.AddControllers();
builder.Services.AddCorsAllowAny();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("allowAnyOrigin");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.AddGlobalUserMiddleware();

app.MapGraphQL();

app.MapControllers();

// using var scope = app.Services.CreateScope();
// var serviceProvider = scope.ServiceProvider;
// var initializer = new DbInitialaizer(serviceProvider);
// await initializer.InitialaizeDb(CancellationToken.None);

app.Run();

public partial class Program { }