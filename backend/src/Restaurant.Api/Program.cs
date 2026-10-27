using Restaurant.Application;
using Restaurant.Infrastructure;
using Restaurant.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add application and infrastructure layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add SignalR for real-time KDS and POS updates
builder.Services.AddSignalR();

// Add API controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngularClient");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<KdsHub>("/hubs/kds");

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "Healthy",
    Version = "1.0.0",
    TimestampUtc = DateTime.UtcNow,
    Service = "Bangladesh Restaurant POS Backend (.NET 10)"
}))
.WithName("HealthCheck");

// Auto-seed sample database records if empty
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Restaurant.Application.Services.SeedDataService>();
    await seeder.SeedAsync();
}

app.Run();
