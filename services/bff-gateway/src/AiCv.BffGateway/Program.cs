using AiCv.BffGateway.Endpoints;
using AiCv.BffGateway.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string serviceName = "bff-gateway";
const string serviceDisplayName = "AI CV BFF Gateway";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "frontend-client";
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Keycloak:RequireHttpsMetadata", false);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = !builder.Environment.IsDevelopment()
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("require-auth", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = serviceDisplayName,
        Version = "v1"
    });
});

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
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
    responsibility = "Single frontend entry point that proxies and composes responses from downstream services.",
    downstreamServices = configuration.GetSection("DownstreamServices").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()
}))
.WithName("GetServiceInfo")
.AllowAnonymous();

app.MapAggregatedEndpoints();
app.MapHealthEndpoints();
app.MapProxyEndpoints();

app.Run();

public partial class Program;
