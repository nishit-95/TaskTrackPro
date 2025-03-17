using Microsoft.OpenApi.Models;
using MyApp.Core.Repositories.Implementations;
using MyApp.Core.Repositories.Interfaces;
using MyApp.MVC.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<NpgsqlConnection>((parameter) =>
{
    var ConnectionString = parameter.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(ConnectionString);
});

builder.Services.AddScoped<IUserProfileInterface, UserProfileRepository>();

// ✅ Configure Swagger (for API testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });
});

// ✅ Configure PostgreSQL Connection String
var connectionString = "Host=cipg01;Port=5432;Username=postgres;Password=123456;Database=Group_E_TaskTrack";

// ✅ Register Repository & Service for Dependency Injection
builder.Services.AddSingleton<IAdminInterface>(new AdminRepository(connectionString));

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
app.UseStaticFiles();

app.UseRouting();

// ✅ Apply CORS Policy (Fixes Fetch Errors)
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

// ✅ Route for API Controllers
app.MapControllers();

// ✅ Route for MVC Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapControllers();
app.Run();

