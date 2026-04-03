using Microsoft.EntityFrameworkCore;
using api_auth_service.Models;

namespace api_auth_service.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId);
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId);
                entity.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId);
            });

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.HasIndex(e => e.Token).IsUnique();
            });

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "User", Description = "Standard user role", IsActive = true },
                new Role { Id = 2, Name = "Admin", Description = "Administrator role", IsActive = true }
            );

            // Seed default permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "read:profile", Description = "Can read own profile", IsActive = true },
                new Permission { Id = 2, Name = "write:profile", Description = "Can edit own profile", IsActive = true },
                new Permission { Id = 3, Name = "read:users", Description = "Can read user data", IsActive = true },
                new Permission { Id = 4, Name = "write:users", Description = "Can create/edit users", IsActive = true },
                new Permission { Id = 5, Name = "delete:users", Description = "Can delete users", IsActive = true },
                new Permission { Id = 6, Name = "read:roles", Description = "Can read roles", IsActive = true },
                new Permission { Id = 7, Name = "write:roles", Description = "Can create/edit roles", IsActive = true },
                new Permission { Id = 8, Name = "admin:access", Description = "Can access admin features", IsActive = true }
            );

            // Assign permissions to roles
            modelBuilder.Entity<RolePermission>().HasData(
                // User role permissions
                new RolePermission { Id = 1, RoleId = 1, PermissionId = 1 },
                new RolePermission { Id = 2, RoleId = 1, PermissionId = 2 },
                new RolePermission { Id = 3, RoleId = 1, PermissionId = 3 },
                // Admin role permissions (all)
                new RolePermission { Id = 4, RoleId = 2, PermissionId = 1 },
                new RolePermission { Id = 5, RoleId = 2, PermissionId = 2 },
                new RolePermission { Id = 6, RoleId = 2, PermissionId = 3 },
                new RolePermission { Id = 7, RoleId = 2, PermissionId = 4 },
                new RolePermission { Id = 8, RoleId = 2, PermissionId = 5 },
                new RolePermission { Id = 9, RoleId = 2, PermissionId = 6 },
                new RolePermission { Id = 10, RoleId = 2, PermissionId = 7 },
                new RolePermission { Id = 11, RoleId = 2, PermissionId = 8 }
            );
        }
    }
}
