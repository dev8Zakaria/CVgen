using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AiCv.BffGateway.Endpoints;
using AiCv.BffGateway.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 1. Auth Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = "account";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("require-auth", policy => policy.RequireAuthenticatedUser());
});

// 2. HttpClient pour nos agrégations
builder.Services.AddHttpClient();

// 3. YARP (Proxy automatique)
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy",
        policy => policy.WithOrigins("http://localhost:5173") 
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

// Ajout du Middleware de gestion d'erreurs (Point 14 du Todo)
app.UseMiddleware<ExceptionMiddleware>(); 

app.UseAuthentication();
app.UseAuthorization();

// Enregistrement de nos routes spécifiques d'agrégation (Points 8 à 12)
app.MapAggregatedEndpoints();
app.MapHealthEndpoints();

// Lancement de YARP pour le reste
app.MapReverseProxy();

app.Run();