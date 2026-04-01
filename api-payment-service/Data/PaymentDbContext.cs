using api_payment_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_payment_service.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentRefund> PaymentRefunds { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== PAYMENT ENTITY ====================
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Payment>()
                .Property(p => p.TransactionId)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.CardLastFour)
                .HasMaxLength(4);

            modelBuilder.Entity<Payment>()
                .Property(p => p.CardBrand)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentGateway)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.GatewayTransactionId)
                .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Payment>()
                .Property(p => p.FailureReason)
                .HasMaxLength(500);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingEmail)
                .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingPhone)
                .HasMaxLength(20);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingAddress)
                .HasMaxLength(200);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingCity)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingState)
                .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingZipCode)
                .HasMaxLength(20);

            modelBuilder.Entity<Payment>()
                .Property(p => p.BillingCountry)
                .HasMaxLength(2);

            // ==================== PAYMENT REFUND ENTITY ====================
            modelBuilder.Entity<PaymentRefund>()
                .HasKey(pr => pr.Id);

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.RefundId)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.Amount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.Reason)
                .HasMaxLength(100);

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.Status)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.GatewayRefundId)
                .HasMaxLength(100);

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.Notes)
                .HasMaxLength(500);

            modelBuilder.Entity<PaymentRefund>()
                .Property(pr => pr.ProcessedBy)
                .HasMaxLength(100);

            // ==================== PAYMENT METHOD ENTITY ====================
            modelBuilder.Entity<PaymentMethod>()
                .HasKey(pm => pm.Id);

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.MethodType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.CardNumber)
                .HasMaxLength(50)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.CardholderName)
                .HasMaxLength(100)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.ExpiryMonth)
                .HasMaxLength(2)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.ExpiryYear)
                .HasMaxLength(4)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.CardBrand)
                .HasMaxLength(50)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.PayPalEmail)
                .HasMaxLength(100)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.BankAccountNumber)
                .HasMaxLength(50)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.BankRoutingNumber)
                .HasMaxLength(20)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.BankName)
                .HasMaxLength(100)
                .IsRequired(false); // ✅ Nullable

            modelBuilder.Entity<PaymentMethod>()
                .Property(pm => pm.GatewayPaymentMethodId)
                .HasMaxLength(100)
                .IsRequired(false); // ✅ Nullable

            // ==================== RELATIONSHIPS ====================
            // One-to-many: Payment -> PaymentRefund
            modelBuilder.Entity<Payment>()
                .HasMany(p => p.Refunds)
                .WithOne(pr => pr.Payment)
                .HasForeignKey(pr => pr.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==================== INDEXES ====================
            // Payment indexes
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.OrderId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.CreatedAt);

            // PaymentRefund indexes
            modelBuilder.Entity<PaymentRefund>()
                .HasIndex(pr => pr.RefundId)
                .IsUnique();

            modelBuilder.Entity<PaymentRefund>()
                .HasIndex(pr => pr.PaymentId);

            modelBuilder.Entity<PaymentRefund>()
                .HasIndex(pr => pr.Status);

            // PaymentMethod indexes
            modelBuilder.Entity<PaymentMethod>()
                .HasIndex(pm => pm.CustomerId);

            modelBuilder.Entity<PaymentMethod>()
                .HasIndex(pm => pm.IsDefault);

            modelBuilder.Entity<PaymentMethod>()
                .HasIndex(pm => pm.IsActive);
        }
    }
}
