using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string serviceName = "bff-gateway";
const string serviceDisplayName = "AI CV BFF Gateway";
const string corsPolicy = "ConfiguredCors";

var builder = WebApplication.CreateBuilder(args);
var jwtAuthority = builder.Configuration["Keycloak:Authority"];

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
                ValidateIssuer = true
            };
        });
}

var app = builder.Build();

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

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = serviceName,
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealth")
.AllowAnonymous();

app.MapGet("/info", (IConfiguration configuration) => Results.Ok(new
{
    service = serviceName,
    displayName = serviceDisplayName,
    responsibility = "Single frontend entry point that composes API responses from downstream services.",
    downstreamServices = configuration.GetSection("DownstreamServices").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()
}))
.WithName("GetServiceInfo")
.AllowAnonymous();

app.Run();

public partial class Program;
