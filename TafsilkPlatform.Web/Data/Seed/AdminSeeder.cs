using System;
using System.Linq;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Security;

namespace TafsilkPlatform.Web.Data.Seed
{
 public static class AdminSeeder
 {
 public static void Seed(AppDbContext db)
 {
 // ensure roles
 var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
 if (adminRole == null)
 {
 adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow };
 db.Roles.Add(adminRole);
 }

 var userRole = db.Roles.FirstOrDefault(r => r.Name == "Customer");
 if (userRole == null)
 {
 userRole = new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role", CreatedAt = DateTime.UtcNow };
 db.Roles.Add(userRole);
 }

 db.SaveChanges();

 // ensure admin user exists
 var adminEmail = "admin@tafsilk.local";
 var adminUser = db.Users.FirstOrDefault(u => u.Email == adminEmail);
 if (adminUser == null)
 {
 adminUser = new User
 {
 Id = Guid.NewGuid(),
 Email = adminEmail,
 PasswordHash = PasswordHasher.Hash("ChangeMe!123"),
 RoleId = adminRole.Id,
 IsActive = true,
 CreatedAt = DateTime.UtcNow
 };
 db.Users.Add(adminUser);
 db.SaveChanges();
 }

 // ensure Admin record exists (link user to admin table)
 var adminRecord = db.Admins.FirstOrDefault(a => a.UserId == adminUser.Id);
 if (adminRecord == null)
 {
 adminRecord = new Admin
 {
 Id = Guid.NewGuid(),
 UserId = adminUser.Id,
 Permissions = "*",
 CreatedAt = DateTime.UtcNow
 };
 db.Admins.Add(adminRecord);
 db.SaveChanges();
 }
 }
 }
}
