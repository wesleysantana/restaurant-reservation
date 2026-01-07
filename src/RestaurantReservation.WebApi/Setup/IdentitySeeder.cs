using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Identity.Context;

namespace RestaurantReservation.WebApi.Setup;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<IdentityDataContext>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        // garante que migrations do Identity rodem
        await context.Database.MigrateAsync();

        //var adminEmail = configuration["ADMIN_EMAIL"];
        //var adminPassword = configuration["ADMIN_PASSWORD"];
        //var adminRole = configuration["ADMIN_ROLE"] ?? "Admin";

        var adminEmail = configuration["ADMIN_EMAIL"] ?? configuration["AdminSeed:Email"];
        var adminPassword = configuration["ADMIN_PASSWORD"] ?? configuration["AdminSeed:Password"];
        var adminRole = configuration["ADMIN_ROLE"] ?? configuration["AdminSeed:Role"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        // cria role Admin se não existir
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // cria usuário admin se não existir
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, adminRole);
            }
        }
    }
}