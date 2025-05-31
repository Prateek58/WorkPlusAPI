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

// Configure DbContext
builder.Services.AddDbContext<LoginWorkPlusContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("WorkPlusConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("WorkPlusConnection"))
    )
);

// Add WorkPlus DbContext
builder.Services.AddDbContext<WorkPlusContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("WorkPlusConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("WorkPlusConnection"))
    )
);

// Add LR DbContext
builder.Services.AddDbContext<LRDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("WorkPlusConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("WorkPlusConnection"))
    )
);

// Add HR DbContext
builder.Services.AddDbContext<HRDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("WorkPlusConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("WorkPlusConnection"))
    )
);

// Add Archive DbContext
builder.Services.AddDbContext<ArchiveContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ArchiveConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ArchiveConnection"))
    )
);

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobWorkService, JobWorkService>();
builder.Services.AddScoped<WorkPlusAPI.WorkPlus.Service.LR.ILRService, WorkPlusAPI.WorkPlus.Service.LR.LRService>();
builder.Services.AddScoped<IJobEntryService, JobEntryService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IWorkPlusReportsService, WorkPlusReportsService>();

// Register HR services
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

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
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// Set EPPlus license
// For EPPlus version 4.5.3.x:
ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before routing and authentication
app.UseCors("AllowReactApp");

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
