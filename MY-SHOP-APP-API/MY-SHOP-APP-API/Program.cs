using Microsoft.EntityFrameworkCore;
using MY_SHOP_APP_API.Data;
using MY_SHOP_APP_API.Business;
using MY_SHOP_APP_API.Logic;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register DbContext using connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services (business and logic layers)
builder.Services.AddScoped<IUserMasterRepository, UserMasterRepository>();
builder.Services.AddScoped<IUserMasterService, UserMasterService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/shop-app-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
// expose OpenAPI endpoints for testing (Swagger UI)
//app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        // Optional: serve swagger UI at app root -> set RoutePrefix = string.Empty
        // c.RoutePrefix = string.Empty;
    });
}

// Use exception handling middleware
app.UseMiddleware<MY_SHOP_APP_API.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
