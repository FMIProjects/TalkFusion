using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalkFusion.Data;
using TalkFusion.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Add application user and roles services
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
options.SignIn.RequireConfirmedAccount = true)
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>(
);

var app = builder.Build();

//Initialize call from seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "ModeratedGroups",
    pattern: "Groups/ModeratedGroups",
    defaults: new { controller = "Groups", action = "ModeratedGroups" }
);

app.MapControllerRoute(
    name: "UnjoinedGroups",
    pattern: "Groups/UnjoinedGroups",
    defaults: new { controller = "Groups", action = "UnjoinedGroups" }
);

app.MapControllerRoute(
    name: "GroupIndex",
    pattern: "Groups/Index",
    defaults: new { controller = "Groups", action = "Index" }
);

app.MapControllerRoute(
    name: "GroupShow",
    pattern: "Groups/Show/{id}/{channelId?}",
    defaults: new { controller = "Groups", action = "Show" }
);

app.MapControllerRoute(
    name: "PromoteMember",
    pattern: "Users/Promote/{userId}/{groupId}",
    defaults: new { controller = "Users", action = "Promote" }
);

app.MapControllerRoute(
    name: "DemoteMember",
    pattern: "Users/Demote/{userId}/{groupId}",
    defaults: new { controller = "Users", action = "Demote" }
);

app.MapControllerRoute(
    name: "KickMember",
    pattern: "Users/Kick/{userId}/{groupId}",
    defaults: new { controller = "Users", action = "Kick" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
