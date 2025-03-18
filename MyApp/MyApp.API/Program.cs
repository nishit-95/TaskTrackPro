using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using MyApp.Core.Repositories.Implementations;
using MyApp.Core.Repositories.Interfaces;
using MyApp.Core.Services;
using MyApp.MVC.Models;
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
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRedisService, RedisService>();   
builder.Services.AddScoped<IUserProfileInterface, UserProfileRepository>();
builder.Services.AddScoped<IAdminInterface, AdminRepository>();



builder.Services.AddScoped<NpgsqlConnection>((ServiceProvider) =>
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



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
}));
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));



builder.Services.AddControllers();





var services = builder.Services;
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var redisConnectionString = configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(redisConnectionString);
});



builder.Services.AddSingleton<IDatabase>(provider =>
{
    var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Redis Server Address
    options.InstanceName = "Session_"; // Prefix for session keys in Redis
});





// Register your repository
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHostedService<RabbitMQConsumerService>();

// ✅ Configure Swagger (for API testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });
});

// ✅ Configure PostgreSQL Connection String
var connectionString = "Host=cipg01;Port=5432;Username=postgres;Password=123456;Database=Group_E_TaskTrack";

// ✅ Register Repository & Service for Dependency Injection
// builder.Services.AddSingleton<IAdminInterface>(new AdminRepository(connectionString));

// ✅ Configure CORS Policy (Fixes Fetch Errors)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});






// ✅ Build app
var app = builder.Build();

// ✅ Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforce HTTPS
}

// ✅ Enable Swagger UI in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin API V1");
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.UseAuthorization();

// ✅ Route for API Controllers
app.MapControllers();

// ✅ Route for MVC Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapControllers();
app.UseCors("corsapp");
app.UseStaticFiles();

app.Run();

