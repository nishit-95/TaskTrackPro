using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using MyApp.Core.Repositories;
using MyApp.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<INotification, NotificationRepository>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;
    if (string.IsNullOrEmpty(redisConnectionString))
    {
        throw new ArgumentNullException(nameof(redisConnectionString), "Redis connection string is not configured in appsettings.json.");
    }
    return ConnectionMultiplexer.Connect(redisConnectionString);
});
builder.Services.AddScoped<RedisService>();

builder.Services.AddScoped<RabbitMQService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();