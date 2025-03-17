using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using MyApp.Core.Repositories.Implementations;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Services;
using Nest;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("tasks")  // Index name in Elasticsearch
    .PrettyJson()
    .DisableDirectStreaming();

var client = new ElasticClient(settings);
builder.Services.AddScoped<ElasticSearchService>();

builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddSingleton<IUserInterface, UserRepository>();
builder.Services.AddSingleton<NpgsqlConnection>((ServiceProvider) =>
{
    var connectionString =
    ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddSingleton(provider =>
{
    var configuration = builder.Configuration;
    var settings = new ElasticsearchClientSettings(new Uri(configuration["Elasticsearch:Uri"]))
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                    .DefaultIndex(configuration["Elasticsearch:DefaultIndex"])
                    .Authentication(new
                        BasicAuthentication(configuration["Elasticsearch:Username"],
                        configuration["Elasticsearch:Password"]
                        )).DisableDirectStreaming();
    return new ElasticsearchClient(settings);
});
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
}));

builder.Services.AddControllers();
builder.Services.AddScoped<NpgsqlConnection>((parameter) =>
{
    var ConnectionString = parameter.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(ConnectionString);
});



builder.Services.AddSingleton<NpgsqlConnection>((UserRepository) =>
{
    var connectionString = UserRepository.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(connectionString);
});
builder.Services.AddSingleton<IUserProfileInterface, UserProfileRepository>();
builder.Services.AddSingleton<IAdminInterface, AdminRepository>();
builder.Services.AddSingleton<IUserInterface, UserRepository>();
var services = builder.Services;
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(configuration);
});

// Register your repository
services.AddSingleton<IAdminInterface, AdminRepository>();

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

app.UseCors("corsapp");
app.MapControllers();



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.MapControllers();
app.UseCors("corsapp");
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
