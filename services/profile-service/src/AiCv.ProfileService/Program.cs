using Microsoft.AspNetCore.Authentication.JwtBearer;
using AiCv.ProfileService.Data;
using AiCv.ProfileService.Modules.Profiles.Repositories;
using AiCv.ProfileService.Modules.Profiles.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string serviceName = "profile-service";
const string serviceDisplayName = "AI CV Profile Service";
const string corsPolicy = "ConfiguredCors";

var builder = WebApplication.CreateBuilder(args);
var jwtAuthority = builder.Configuration["Keycloak:Authority"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

builder.Services.AddDbContext<ProfileDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<ProfileRepository>();
builder.Services.AddScoped<ProfileService>();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
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
    responsibility = "Owns user profile data: personal information, education, experience, skills, languages, and projects."
}))
.WithName("GetServiceInfo")
.AllowAnonymous();

app.Run();

public partial class Program;
