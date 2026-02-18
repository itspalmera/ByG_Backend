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
using ByG_Backend.src.Repository;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

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
            RoleClaimType = "role",
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SignInKey"]!)
            ),
            NameClaimType = JwtRegisteredClaimNames.NameId
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


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try 
    {
        var context = services.GetRequiredService<DataContext>();
        
        // ESTA LÍNEA ES CLAVE: Crea la BD y las tablas si no existen
        await context.Database.MigrateAsync(); 

        await IdentitySeeder.SeedRolesAsync(services);
        await IdentitySeeder.SeedAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error durante el inicio: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
