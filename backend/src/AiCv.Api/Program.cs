using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AiCv.Api.Data;
using AiCv.Api.Modules.Cvs.Repositories;
using AiCv.Api.Modules.Cvs.Services;
using AiCv.Api.Modules.Ai;
using AiCv.Api.Modules.Profiles.Services;
using AiCv.Api.Modules.Opportunities.Repositories;
using AiCv.Api.Modules.Opportunities.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AiCv.Api.Modules.Profiles.Repositories.ProfileRepository>();
builder.Services.AddScoped<AiCv.Api.Modules.Profiles.Services.ProfileService>();
builder.Services.AddScoped<CvRepository>();
builder.Services.AddScoped<CvService>();
builder.Services.AddHttpClient<IAiService, AiService>(client =>
{
    var baseUrl = builder.Configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<OpportunityRepository>();
builder.Services.AddScoped<OpportunityService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://192.168.0.210")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.MetadataAddress = "http://192.168.0.220:8080/realms/ai-cv-generator/.well-known/openid-configuration";
        options.RequireHttpsMetadata = false;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                "https://192.168.0.210/realms/ai-cv-generator"
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "AiCv.Api" }));

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Base de données mise à jour avec succès.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur lors de la migration de la base de données : {ex.Message}");
    }
}

app.Run();

public partial class Program;
