using api_cart_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_cart_service.Data
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Cart entity
            modelBuilder.Entity<Cart>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Cart>()
                .Property(c => c.Status)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalPrice)
                .HasColumnType("decimal(18, 2)");

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => ci.Id);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.ProductSku)
                .HasMaxLength(50);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.TotalPrice)
                .HasColumnType("decimal(18, 2)");

            // Configure one-to-many relationship
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes for faster queries
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.CustomerId);

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.CreatedAt);

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductId);
        }
    }
}
