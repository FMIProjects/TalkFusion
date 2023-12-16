
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Data;
using TalkFusion.Models;

namespace ArticlesApp.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider
        serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {

                if (context.Roles.Any())
                {
                    return;
                }

                context.Roles.AddRange(

                new IdentityRole
                {
                    Id = "16a0e322-cb98-4b42-9068-f3edfbd55890",
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                },

                new IdentityRole
                {
                    Id = "16a0e322-cb98-4b42-9068-f3edfbd55892",
                    Name = "User",
                    NormalizedName = "User".ToUpper()
                }


                );
                //hasher for passwords
                var hasher = new PasswordHasher<ApplicationUser>();

                //create a user for every role
                context.Users.AddRange(
                new ApplicationUser

                {

                    Id = "233dfd41-892a-4cdb-8ffc-17258d6c5770",
                    // primary key
                    UserName = "admin@test.com",
                    NickName = "Admin",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },

                new ApplicationUser
                {

                    Id = "233dfd41-892a-4cdb-8ffc-17258d6c5772",
                    // primary key
                    UserName = "user@test.com",
                    NickName = "User",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                }
                );

                // user-role association
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {

                    RoleId = "16a0e322-cb98-4b42-9068-f3edfbd55890",

                    UserId = "233dfd41-892a-4cdb-8ffc-17258d6c5770"
                },

                new IdentityUserRole<string>

                {

                    RoleId = "16a0e322-cb98-4b42-9068-f3edfbd55892",


                    UserId = "233dfd41-892a-4cdb-8ffc-17258d6c5772"
                }
                );
                context.SaveChanges();
            }
        }
    }
}
