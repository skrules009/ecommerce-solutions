using api_inventory_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_inventory_service.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Inventory entity
            modelBuilder.Entity<Inventory>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<Inventory>()
                .Property(i => i.ProductSku)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Inventory>()
                .Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Inventory>()
                .Property(i => i.Warehouse)
                .HasMaxLength(100);

            modelBuilder.Entity<Inventory>()
                .Property(i => i.Location)
                .HasMaxLength(100);

            // Configure InventoryTransaction entity
            modelBuilder.Entity<InventoryTransaction>()
                .HasKey(it => it.Id);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.TransactionType)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.Reference)
                .HasMaxLength(100);

            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.CreatedBy)
                .HasMaxLength(100);

            // Configure one-to-many relationship
            modelBuilder.Entity<Inventory>()
                .HasMany(i => i.Transactions)
                .WithOne(it => it.Inventory)
                .HasForeignKey(it => it.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes for faster queries
            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.ProductId)
                .IsUnique();

            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.ProductSku);

            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.Warehouse);

            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.Quantity);

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(it => it.InventoryId);

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(it => it.CreatedAt);

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(it => it.TransactionType);
        }
    }
}
