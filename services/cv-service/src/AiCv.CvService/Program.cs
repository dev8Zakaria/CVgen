using Microsoft.AspNetCore.Authentication.JwtBearer;
using AiCv.CvService.Data;
using AiCv.CvService.Modules.Cvs.Clients;
using AiCv.CvService.Modules.Cvs.Repositories;
using AiCv.CvService.Modules.Cvs.Services;
using AiCv.CvService.Modules.Cvs.Storage;
using AiCv.CvService.Modules.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string serviceName = "cv-service";
const string serviceDisplayName = "AI CV Generation Service";
const string corsPolicy = "ConfiguredCors";

var builder = WebApplication.CreateBuilder(args);
var jwtAuthority = builder.Configuration["Keycloak:Authority"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var profileBaseUrl = builder.Configuration["DownstreamServices:Profile"] ?? "http://localhost:5101";
var opportunityBaseUrl = builder.Configuration["DownstreamServices:Opportunity"] ?? "http://localhost:5102";
var aiBaseUrl = builder.Configuration["DownstreamServices:Ai"] ?? "http://localhost:8000";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = serviceDisplayName,
        Version = "v1"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<CvDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<CvRepository>();
builder.Services.AddScoped<CvGenerationService>();
builder.Services.AddScoped<CvGenerationJobService>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IEventPublisher>(serviceProvider =>
    builder.Configuration.GetValue("RabbitMq:Enabled", false)
        ? serviceProvider.GetRequiredService<RabbitMqEventPublisher>()
        : serviceProvider.GetRequiredService<NoOpEventPublisher>());
builder.Services.AddSingleton<ICvGenerationQueue>(serviceProvider =>
    builder.Configuration.GetValue("RabbitMq:Enabled", false)
        ? serviceProvider.GetRequiredService<RabbitMqCvGenerationQueue>()
        : serviceProvider.GetRequiredService<NoOpCvGenerationQueue>());
builder.Services.AddSingleton<RabbitMqEventPublisher>();
builder.Services.AddSingleton<NoOpEventPublisher>();
builder.Services.AddSingleton<RabbitMqCvGenerationQueue>();
builder.Services.AddSingleton<NoOpCvGenerationQueue>();
builder.Services.AddHostedService<CvGenerationWorker>();
builder.Services.AddHttpClient<ICvObjectStorage, CvObjectStorage>();
builder.Services.AddHttpClient<IProfileClient, ProfileClient>(client =>
{
    client.BaseAddress = new Uri(profileBaseUrl);
});
builder.Services.AddHttpClient<IOpportunityClient, OpportunityClient>(client =>
{
    client.BaseAddress = new Uri(opportunityBaseUrl);
});
builder.Services.AddHttpClient<IAiCvGenerationClient, AiCvGenerationClient>(client =>
{
    client.BaseAddress = new Uri(aiBaseUrl);
});

if (!string.IsNullOrWhiteSpace(jwtAuthority))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtAuthority;
            options.RequireHttpsMetadata = builder.Configuration.GetValue("Keycloak:RequireHttpsMetadata", false);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = !builder.Environment.IsDevelopment()
            };
        });
}

var app = builder.Build();

if (builder.Configuration.GetValue("Database:ApplyMigrations", false))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CvDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicy);

if (!string.IsNullOrWhiteSpace(jwtAuthority))
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = serviceName,
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealth")
.AllowAnonymous();

app.MapGet("/info", () => Results.Ok(new
{
    service = serviceName,
    displayName = serviceDisplayName,
    responsibility = "Owns CV generation orchestration, generated CV versions, templates, and final document metadata."
}))
.WithName("GetServiceInfo")
.AllowAnonymous();

app.Run();

public partial class Program;
