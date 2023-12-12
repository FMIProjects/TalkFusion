using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Models;

namespace TalkFusion.Data
{
    
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Channel> Channels { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<UserGroup> UserGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder
modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // definire primary key compus
            modelBuilder.Entity<UserGroup>()
            .HasKey(ab => new {
                ab.Id,
                ab.UserId,
                ab.GroupId
            });

            // definire relatii cu modelele Bookmark si Article (FK)
            modelBuilder.Entity<UserGroup>()
            .HasOne(ab => ab.Group)
            .WithMany(ab => ab.UserGroups)
            .HasForeignKey(ab => ab.GroupId);
            modelBuilder.Entity<UserGroup>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.UserGroups)
            .HasForeignKey(ab => ab.UserId);
        }


    }
}