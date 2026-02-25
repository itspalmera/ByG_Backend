using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ByG_Backend.src.Interfaces;
using ByG_Backend.src.Services;
using ByG_Backend.src.Data; // Asegúrate de que este namespace incluya tu DataSeeder
using ByG_Backend.src.Models;
using Resend;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using QuestPDF.Infrastructure;
using ByG_Backend.src.Options;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddOpenApi();

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
            // Usamos los tipos estándar que coinciden con TokenService.cs
        
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddOptions();

// Configurar Resend para envío de emails
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["Resend:ApiKey"]!;
});
builder.Services.AddHttpClient<IResend, ResendClient>();

// Configurar Entity Framework con SQLite
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar controladores
builder.Services.AddControllers();
builder.Services.AddScoped<ITokenServices, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS: Configuración flexible
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
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
        
        // 1. Crea la BD y aplica migraciones pendientes
        await context.Database.MigrateAsync(); 

        // 2. Seeder de Usuarios y Roles (Identity)
        await IdentitySeeder.SeedRolesAsync(services);
        await IdentitySeeder.SeedAdminUserAsync(services);

        // 3. Seeder de Datos de Negocio (Compras, Proveedores, Cotizaciones)
        //    Nota: Como este método no es async en el archivo que te di, se llama directo.
        DataSeeder.Seed(context);
        
        Console.WriteLine("--> Datos de prueba (Compras/Proveedores) cargados correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error durante el inicio y sembrado de datos: {ex.Message}");
    }
}


// ==========================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("DefaultCorsPolicy");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();