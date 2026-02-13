using Microsoft.AspNetCore.Identity;
using ByG_Backend.src.Models;

namespace ByG_Backend.src.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { 
                "Admin", 
                "GestorCompras", 
                "AutorizadorCompras", 
                "User" 
                };

            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
        }

        public static async Task SeedAdminUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();

            // Cambia estos datos por los reales
            var adminEmail = "pame@gmail.com";
            var adminPassword = "Admin1234!"; // Ideal: desde config/env

            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                user = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Pamela",
                    LastName = "Vera",
                    Role = "Admin",
                    IsActive = true,
                    Registered = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                var created = await userManager.CreateAsync(user, adminPassword);
                if (!created.Succeeded)
                    throw new Exception(string.Join(" | ", created.Errors.Select(e => e.Description)));
            }

            // Asegura rol Admin (y opcionalmente quita User)
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }

            // Opcional: si NO quieres que Admin también tenga "User"
            if (await userManager.IsInRoleAsync(user, "User"))
            {
                await userManager.RemoveFromRoleAsync(user, "User");
            }
        }
    }
}
