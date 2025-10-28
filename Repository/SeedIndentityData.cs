using Microsoft.AspNetCore.Identity;
using Shopping_Tutorial.Models;
using System;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Repository
{
    public static class SeedIdentityData
    {
        public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
        {
            string[] roles = { "USER", "ADMIN" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new AppUserModel
                {
                    UserName = "admin",
                    Email = adminEmail
                };
                var result = await userManager.CreateAsync(admin, "Admin1234!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "ADMIN");
                }
            }
        }
    }
}
