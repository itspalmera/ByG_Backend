using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.Services;
using ByG_Backend.src.Data;
using ByG_Backend.src.Models;
using Resend;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using QuestPDF.Infrastructure;
using ByG_Backend.src.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql; // Necesario para PostgreSQL

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Licencia QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// ==================================================================
// 2. CONFIGURACIÓN HÍBRIDA DE BASE DE DATOS (SQLite Local / Postgres Render)
// ==================================================================

if (builder.Environment.IsDevelopment())
{
    // --- MODO LOCAL (SQLite) ---
    Console.WriteLine("--> Usando Base de Datos Local: SQLite");
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    // --- MODO PRODUCCIÓN (Render/Supabase - PostgreSQL) ---
    Console.WriteLine("--> Usando Base de Datos Producción: PostgreSQL (Supabase)");
    
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    // Parseo inteligente para convertir la URL de Supabase a formato .NET
    if (!string.IsNullOrEmpty(connectionString))
    {
        connectionString = BuildConnectionString(connectionString);
    }
    else 
    {
        // Fallback por seguridad si no encuentra la variable
        throw new Exception("No se encontró la variable de entorno DATABASE_URL en Producción.");
    }

    builder.Services.AddDbContext<DataContext>(options =>
        options.UseNpgsql(connectionString));
}

// ==================================================================

// 3. Configuración de Identity y JWT
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SignInKey"]!)
            ),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

// 4. Servicios (Resend, OpenAPI, CORS)
builder.Services.AddOpenApi();
builder.Services.AddOptions();

builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["ResendAPIkey"];
});
builder.Services.AddHttpClient<IResend, ResendClient>();

builder.Services.AddControllers();
builder.Services.AddScoped<ITokenServices, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        // En Producción lee de variable de entorno, en local usa localhost:3000
        var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
        var allowedOrigins = !string.IsNullOrEmpty(allowedOriginsEnv)
            ? allowedOriginsEnv.Split(',')
            : builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
              ?? new[] { "http://localhost:3000" }; 

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.Configure<CompanyInfoOptions>(builder.Configuration.GetSection("CompanyInfo"));

var app = builder.Build();

// ==========================================
// ZONA DE INICIALIZACIÓN DE DATOS (SEEDING)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<DataContext>();
        
        if (app.Environment.IsDevelopment())
        {
            // En LOCAL (SQLite) usamos migraciones normales
            await context.Database.MigrateAsync();
        }
        else
        {
            // --- MODO PRODUCCIÓN (Postgres / Supabase) ---
            // Estrategia: "Crear tablas sí o sí"
            // Usamos RelationalDatabaseCreator para forzar la creación del esquema
            // ignorando si Supabase ya tiene tablas de sistema.
            var databaseCreator = context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            
            if (databaseCreator != null)
            {
                // 1. Asegurar que la BD existe (En Supabase siempre es true, pero por seguridad)
                if (!await databaseCreator.CanConnectAsync()) await databaseCreator.CreateAsync();

                // 2. Intentar crear las tablas del modelo
                try
                {
                    await databaseCreator.CreateTablesAsync();
                    Console.WriteLine("--> Tablas creadas exitosamente en Supabase.");
                }
                catch (Exception ex)
                {
                    // Si falla porque ya existen, solo avisamos. Si es otro error, lo veremos en los logs.
                    Console.WriteLine($"--> Nota: {ex.Message} (Probablemente las tablas ya existían o hubo un conflicto menor).");
                }
            }
        }

        // Semillas (Usuarios y Datos) - Funcionan igual en ambos
        await IdentitySeeder.SeedRolesAsync(services);
        await IdentitySeeder.SeedAdminUserAsync(services);
        DataSeeder.Seed(context);
        
        Console.WriteLine("--> Base de datos inicializada y sembrada correctamente.");
    }

    catch (Exception ex)
    {
        Console.WriteLine($"Error crítico DB: {ex.Message}");
    }
}
// ==========================================

app.UseCors("DefaultCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// --- FUNCION AUXILIAR PARA PARSEAR URL DE SUPABASE ---
static string BuildConnectionString(string databaseUrl)
{
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Disable // Para Session Pooler de Supabase puerto 5432
    };
    return builder.ToString();
}