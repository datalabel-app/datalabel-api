using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataLabeling.API.Extensions
{
    public class DatabaseSeeder
    {
        public static async System.Threading.Tasks.Task SeedAdminUserAsync(ApplicationDbContext context)
        {
            var adminEmail = "admin@datalabel.site";
            var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

            if (!adminExists)
            {
                var adminUser = new User
                {
                    FullName = "Administrator",
                    Email = adminEmail,
                    Password = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = UserRole.Admin,
                    Status = "Active",
                    IsChangePassword = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                Console.WriteLine($"Admin user seeded successfully: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"ℹAdmin user already exists: {adminEmail}");
            }
        }
    }
}
