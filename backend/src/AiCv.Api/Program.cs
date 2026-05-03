using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AiCv.Api.Data;
using AiCv.Api.Modules.Profiles.Services;

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
            ValidAudience = builder.Configuration["Keycloak:Audience"],
            ValidateIssuer = true
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

app.Run();