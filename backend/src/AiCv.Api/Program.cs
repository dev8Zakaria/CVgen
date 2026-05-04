using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AiCv.Api.Data;
using AiCv.Api.Modules.Profiles.Services;
using AiCv.Api.Modules.Opportunities.Repositories;
using AiCv.Api.Modules.Opportunities.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURATION DE LA BASE DE DONNÉES
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// 2. INJECTION DES DÉPENDANCES (SERVICES)
// ==========================================
builder.Services.AddScoped<IProfileService, ProfileService>();
// ... dans la section builder.Services :
builder.Services.AddScoped<IOpportunityService, OpportunityService>();

// ==========================================
// 3. CONFIGURATION DES CORS (POUR REACT)
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // On autorise l'URL du frontend définie dans le docker-compose
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==========================================
// 4. CONFIGURATION AUTHENTIFICATION (KEYCLOAK)
// ==========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.RequireHttpsMetadata = false; // False car on est en local HTTP avec Docker
        
       options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true, // On vérifie l'émetteur
            // On force l'API à accepter ces deux émetteurs, peu importe d'où vient la requête
            ValidIssuers = new[] 
            { 
                "http://localhost:8080/realms/ai-cv-generator",
                "http://keycloak:8080/realms/ai-cv-generator"
            }
        };
    });


builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();


// ==========================================
// 5. PIPELINE HTTP (MIDDLEWARES)
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// L'ordre des middlewares est strict !
app.UseCors("AllowFrontend");

app.UseAuthentication(); // 1. Vérifier QUI est l'utilisateur (JWT)
app.UseAuthorization();  // 2. Vérifier s'il a les DROITS

app.MapControllers();

// Votre route de vérification de santé (très utile pour Docker/Proxmox !)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "AiCv.Api" }));
// ==========================================
// 6. AUTO-MIGRATION DE LA BASE DE DONNÉES
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Cette ligne applique automatiquement toutes les migrations en attente au démarrage
        context.Database.Migrate();
        Console.WriteLine("✅ Base de données mise à jour avec succès.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur lors de la migration de la base de données : {ex.Message}");
    }
}

app.Run();