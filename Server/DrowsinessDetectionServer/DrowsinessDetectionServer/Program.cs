using DrowsinessDetectionServer.Datas;
using DrowsinessDetectionServer.Services.ScopedServices;
using DrowsinessDetectionServer.Services.SingletonServices;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using RoleBaseAuthorizationLibrary;
using Serilog;
using Serilog.Core;
using System.Reflection;
using System.Text.Json.Serialization;

// Create builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add API endpoints
builder.Services.AddEndpointsApiExplorer();

// Use http client
builder.Services.AddHttpClient();

// Use memory cache
builder.Services.AddMemoryCache();

// Add controller
builder.Services.AddControllers().AddJsonOptions
    (options =>
    {
        // Serialize enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Disable camelCasing
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Use swagger
builder.Services.AddSwaggerGen
    (options =>
    {
        // Annotations
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Drowsiness Detection Server",
            Description = "First iteration of the APIs",
        });
        // Set the comments path for the Swagger JSON and UI.
        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        // Pick comments from classes, including controller summary comments
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        // Security scheme
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        // Security implement
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

// Add serilog as logging service
Logger logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
GlobalData.TryGetLogPath(builder.Configuration);

// Set connection string
string? connectionStringVar = builder.Configuration.GetSection("ConnectionStringVar").Get<string>();
if (string.IsNullOrWhiteSpace(connectionStringVar)) connectionStringVar = "DefaultConnection";
string? connectionString = builder.Configuration.GetConnectionString(connectionStringVar);
if (string.IsNullOrWhiteSpace(connectionString)) connectionString = "Server=KATO\\MERCURYLOCAL;Database=DrowsinessDetection;Trusted_Connection=True;User Id=sa;Password=kato@131211#;MultipleActiveResultSets=True;TrustServerCertificate=True;Integrated security=false";

// Add database
builder.Services.AddDbContext<ApplicationDbContext>
    (options =>
    {
        // Connect to SQL server
        options.UseSqlServer(connectionString);
    });

// Configure background service

// Configure singleton service
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<ILogService, LogService>();

// Configure scoped services
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();

// Build the app
WebApplication app = builder.Build();

// Initialize database
using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        // Migrate latest database changes
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        // Seed user
        GlobalData.SeedUser(dbContext);
    }
    catch
    {

    }
}

// Set up cache
CacheData.Cache = app.Services.GetRequiredService<IMemoryCache>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Support web sockets
app.UseWebSockets();

// Global cors policy
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Web socket middleware
app.UseMiddleware<WebSocketMiddleware>();

// Custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();

// Run the app
app.Run();
