using ShoppingAssistantApi.Application.ApplicationExtentions;
using ShoppingAssistantApi.Persistance.PersistanceExtentions;
using ShoppingAssistantApi.Infrastructure.InfrastructureExtentions;
using ShoppingAssistantApi.Api.ApiExtentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddJWTTokenAuthentication(builder.Configuration);
builder.Services.AddMapper();
builder.Services.AddInfrastructure();
builder.Services.AddServices();
builder.Services.AddGraphQl();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.AddGlobalUserMiddleware();

app.MapGraphQL();

app.MapControllers();
/*
using var scope = app.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;
using var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;
var initializer = new DbInitialaizer(serviceProvider);
initializer.InitialaizeDb(cancellationToken);
*/
app.Run();

public partial class Program { }