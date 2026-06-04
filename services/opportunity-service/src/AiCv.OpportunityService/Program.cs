using Microsoft.AspNetCore.Authentication.JwtBearer;
using AiCv.OpportunityService.Data;
using AiCv.OpportunityService.Modules.Ai;
using AiCv.OpportunityService.Modules.Messaging;
using AiCv.OpportunityService.Modules.Opportunities.Repositories;
using OpportunityDomainService = AiCv.OpportunityService.Modules.Opportunities.Services.OpportunityService;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string serviceName = "opportunity-service";
const string serviceDisplayName = "AI CV Opportunity Service";
const string corsPolicy = "ConfiguredCors";

var builder = WebApplication.CreateBuilder(args);
var jwtAuthority = builder.Configuration["Keycloak:Authority"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var aiServiceBaseUrl = builder.Configuration["AiService:BaseUrl"] ?? "http://localhost:8000";

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

builder.Services.AddDbContext<OpportunityDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<OpportunityRepository>();
builder.Services.AddScoped<OpportunityDomainService>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IEventPublisher>(serviceProvider =>
    builder.Configuration.GetValue("RabbitMq:Enabled", false)
        ? serviceProvider.GetRequiredService<RabbitMqEventPublisher>()
        : serviceProvider.GetRequiredService<NoOpEventPublisher>());
builder.Services.AddSingleton<RabbitMqEventPublisher>();
builder.Services.AddSingleton<NoOpEventPublisher>();
builder.Services.AddHttpClient<IAiService, AiService>(client =>
{
    client.BaseAddress = new Uri(aiServiceBaseUrl);
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
    var dbContext = scope.ServiceProvider.GetRequiredService<OpportunityDbContext>();
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
    responsibility = "Owns opportunity ingestion and analysis inputs: job offers, requirements, keywords, and match context."
}))
.WithName("GetServiceInfo")
.AllowAnonymous();

app.Run();

public partial class Program;
