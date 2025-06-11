using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WorkPlusAPI.Data;
using OfficeOpenXml;
using WorkPlusAPI.Archive.Services;
using WorkPlusAPI.Archive.Data.Workplus;
using WorkPlusAPI.Archive.Services.Archive;
using WorkPlusAPI.WorkPlus.Service;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.Service.LR;
using WorkPlusAPI.WorkPlus.Service.HR;
using WorkPlusAPI.WorkPlus.Data.UserSettings;
using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WorkPlus API", Version = "v1" });
    
    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Get environment and log it
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"🌍 Running in Environment: {environment}");

// Debug configuration loading
Console.WriteLine($"🔧 Configuration sources loaded:");
foreach (var source in builder.Configuration.Sources)
{
    Console.WriteLine($"   - {source.GetType().Name}");
}

// Configure DbContext
var workingConnectionString = builder.Configuration.GetConnectionString("WorkPlusConnection") ?? 
    throw new InvalidOperationException("WorkPlusConnection not found");
var workingArchiveConnectionString = builder.Configuration.GetConnectionString("ArchiveConnection") ?? 
    throw new InvalidOperationException("ArchiveConnection not found");

// Debug connection strings
Console.WriteLine($"🔗 Using WorkPlus Connection: {workingConnectionString.Substring(0, Math.Min(50, workingConnectionString.Length))}...");
Console.WriteLine($"🔗 Using Archive Connection: {workingArchiveConnectionString.Substring(0, Math.Min(50, workingArchiveConnectionString.Length))}...");

// Configure DbContext using the connection strings
builder.Services.AddDbContext<LoginWorkPlusContext>(options =>
    options.UseMySql(workingConnectionString, ServerVersion.AutoDetect(workingConnectionString))
);

// Add WorkPlus DbContext
builder.Services.AddDbContext<WorkPlusContext>(options =>
    options.UseMySql(workingConnectionString, ServerVersion.AutoDetect(workingConnectionString))
);

// Add User Settings DbContext
builder.Services.AddDbContext<UserSettingsContext>(options =>
    options.UseMySql(workingConnectionString, ServerVersion.AutoDetect(workingConnectionString))
);

// Add LR DbContext
builder.Services.AddDbContext<LRDbContext>(options =>
    options.UseMySql(workingConnectionString, ServerVersion.AutoDetect(workingConnectionString))
);

// Add HR DbContext
builder.Services.AddDbContext<HRDbContext>(options =>
    options.UseMySql(workingConnectionString, ServerVersion.AutoDetect(workingConnectionString))
);

// Add Archive DbContext
builder.Services.AddDbContext<ArchiveContext>(options =>
    options.UseMySql(workingArchiveConnectionString, ServerVersion.AutoDetect(workingArchiveConnectionString))
);

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobWorkService, JobWorkService>();
builder.Services.AddScoped<WorkPlusAPI.WorkPlus.Service.LR.ILRService, WorkPlusAPI.WorkPlus.Service.LR.LRService>();
builder.Services.AddScoped<WorkPlusAPI.Archive.Services.Archive.ILRService, WorkPlusAPI.Archive.Services.Archive.LRService>();
builder.Services.AddScoped<IJobEntryService, JobEntryService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IWorkPlusReportsService, WorkPlusReportsService>();

// Register HR services
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

// Register User Settings service
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")))
        };
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        corsBuilder =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[]
            {
                "http://localhost:5173",
                "https://localhost:5173",
                "https://workplus.layerbiz.com",
                "http://workplus.layerbiz.com"
            };
            
            corsBuilder.WithOrigins(allowedOrigins)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials()
                   .SetPreflightMaxAge(TimeSpan.FromSeconds(2520)); // Cache preflight for 42 minutes
        });
    
    // Add a more permissive policy for debugging (remove in production)
    options.AddPolicy("DevelopmentCors", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
    });
});

// Add global exception handling middleware
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandlingPath = "/error";
});

// Add problem details for better error responses
builder.Services.AddProblemDetails();

// Set EPPlus license
// For EPPlus version 4.5.3.x:
ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Use CORS before routing and authentication
app.UseCors("DevelopmentCors"); // Temporarily use permissive CORS for debugging

// Add global exception handling
app.UseExceptionHandler();

// Custom middleware to ensure CORS headers are always present
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Log the exception
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred");
        
        // Ensure CORS headers are set even on error
        var corsPolicy = "AllowReactApp";
        var corsService = context.RequestServices.GetRequiredService<ICorsService>();
        var corsPolicyProvider = context.RequestServices.GetRequiredService<ICorsPolicyProvider>();
        var policy = await corsPolicyProvider.GetPolicyAsync(context, corsPolicy);
        
        if (policy != null)
        {
            var corsResult = corsService.EvaluatePolicy(context, policy);
            corsService.ApplyResult(corsResult, context.Response);
        }
        
        // Return a proper error response
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Internal server error",
            message = "An error occurred while processing your request"
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
