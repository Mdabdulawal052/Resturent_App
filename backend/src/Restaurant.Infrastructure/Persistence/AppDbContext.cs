using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Domain.Entities;

namespace Restaurant.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RestaurantEntity> Restaurants => Set<RestaurantEntity>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Modifier> Modifiers => Set<Modifier>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<ProductModifier> ProductModifiers => Set<ProductModifier>();
    public DbSet<Combo> Combos => Set<Combo>();
    public DbSet<DiningArea> DiningAreas => Set<DiningArea>();
    public DbSet<RestaurantTable> Tables => Set<RestaurantTable>();
    public DbSet<TableSession> TableSessions => Set<TableSession>();
    public DbSet<TableBooking> Bookings => Set<TableBooking>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemModifier> OrderItemModifiers => Set<OrderItemModifier>();
    public DbSet<OrderDiscount> OrderDiscounts => Set<OrderDiscount>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<KitchenStation> KitchenStations => Set<KitchenStation>();
    public DbSet<KOT> KOTs => Set<KOT>();
    public DbSet<KOTItem> KOTItems => Set<KOTItem>();

    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<StockLedger> StockLedger => Set<StockLedger>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeLine> RecipeLines => Set<RecipeLine>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();

    public DbSet<CashShift> CashShifts => Set<CashShift>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();
    public DbSet<TaxInvoice> TaxInvoices => Set<TaxInvoice>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema pos
        modelBuilder.Entity<RestaurantEntity>(b =>
        {
            b.ToTable("Restaurants", "pos");
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Branch>(b =>
        {
            b.ToTable("Branches", "pos");
            b.HasIndex(x => new { x.RestaurantId, x.Code }).IsUnique();
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Terminal>(b =>
        {
            b.ToTable("Terminals", "pos");
            b.HasIndex(x => new { x.BranchId, x.Code }).IsUnique();
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<DiningArea>().ToTable("DiningAreas", "pos");
        modelBuilder.Entity<RestaurantTable>().ToTable("Tables", "pos");
        modelBuilder.Entity<TableSession>().ToTable("TableSessions", "pos");

        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("Categories", "pos");
            b.HasOne(x => x.Parent)
             .WithMany(x => x.SubCategories)
             .HasForeignKey(x => x.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products", "pos");
            b.Property(x => x.BasePrice).HasPrecision(18, 2);
            b.Property(x => x.CostPrice).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
            b.HasIndex(x => new { x.CategoryId, x.IsActive });
        });

        modelBuilder.Entity<ProductVariant>(b =>
        {
            b.ToTable("ProductVariants", "pos");
            b.Property(x => x.PriceDelta).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Modifier>().ToTable("Modifiers", "pos");
        modelBuilder.Entity<ModifierOption>(b =>
        {
            b.ToTable("ModifierOptions", "pos");
            b.Property(x => x.PriceDelta).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProductModifier>().ToTable("ProductModifiers", "pos");
        modelBuilder.Entity<Combo>(b =>
        {
            b.ToTable("Combos", "pos");
            b.Property(x => x.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<TableBooking>(b =>
        {
            b.ToTable("TableBookings", "pos");
            b.Property(x => x.DepositAmount).HasPrecision(18, 2);
            b.HasIndex(x => new { x.BranchId, x.BookingDate });
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("Orders", "pos");
            b.HasIndex(x => new { x.BranchId, x.OrderNo }).IsUnique();
            b.HasIndex(x => new { x.BranchId, x.OrderedAtUtc });
            b.HasIndex(x => new { x.Status, x.OrderedAtUtc });
            b.Property(x => x.Subtotal).HasPrecision(18, 2);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            b.Property(x => x.ServiceChargeAmount).HasPrecision(18, 2);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2);
            b.Property(x => x.RoundingAmount).HasPrecision(18, 2);
            b.Property(x => x.GrandTotal).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItems", "pos");
            b.Property(x => x.Qty).HasPrecision(18, 6);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2);
            b.Property(x => x.NetAmount).HasPrecision(18, 2);
            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => new { x.ProductId, x.OrderId });
        });

        modelBuilder.Entity<OrderItemModifier>(b =>
        {
            b.ToTable("OrderItemModifiers", "pos");
            b.Property(x => x.Qty).HasPrecision(18, 6);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderDiscount>(b =>
        {
            b.ToTable("OrderDiscounts", "pos");
            b.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PaymentMethod>(b =>
        {
            b.ToTable("PaymentMethods", "pos");
            b.HasIndex(x => new { x.RestaurantId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments", "pos");
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => new { x.PaymentMethodId, x.PaidAtUtc });
        });

        modelBuilder.Entity<Refund>(b =>
        {
            b.ToTable("Refunds", "pos");
            b.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<NumberSequence>().ToTable("NumberSequences", "pos");
        modelBuilder.Entity<Setting>().ToTable("Settings", "pos");

        // Schema kitchen
        modelBuilder.Entity<KitchenStation>().ToTable("KitchenStations", "kitchen");
        modelBuilder.Entity<KOT>(b =>
        {
            b.ToTable("KOTs", "kitchen");
            b.HasIndex(x => new { x.StationId, x.Status, x.CreatedAtUtc });
            b.HasIndex(x => x.OrderId);
        });
        modelBuilder.Entity<KOTItem>(b =>
        {
            b.ToTable("KOTItems", "kitchen");
            b.Property(x => x.Qty).HasPrecision(18, 6);
        });

        // Schema inventory
        modelBuilder.Entity<Unit>(b =>
        {
            b.ToTable("Units", "inventory");
            b.Property(x => x.ConversionFactor).HasPrecision(18, 6);
            b.HasIndex(x => new { x.RestaurantId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<InventoryItem>(b =>
        {
            b.ToTable("Items", "inventory");
            b.Property(x => x.ReorderLevel).HasPrecision(18, 6);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Warehouse>(b =>
        {
            b.ToTable("Warehouses", "inventory");
            b.HasIndex(x => new { x.BranchId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<StockBalance>(b =>
        {
            b.ToTable("StockBalances", "inventory");
            b.Property(x => x.Quantity).HasPrecision(18, 6);
            b.Property(x => x.AverageCost).HasPrecision(18, 6);
            b.HasKey(x => new { x.WarehouseId, x.InventoryItemId });
        });

        modelBuilder.Entity<StockLedger>(b =>
        {
            b.ToTable("StockLedger", "inventory");
            b.Property(x => x.QtyIn).HasPrecision(18, 6);
            b.Property(x => x.QtyOut).HasPrecision(18, 6);
            b.Property(x => x.UnitCost).HasPrecision(18, 6);
            b.Property(x => x.BalanceQty).HasPrecision(18, 6);
            b.HasIndex(x => new { x.WarehouseId, x.InventoryItemId, x.CreatedAtUtc });
            b.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        });

        modelBuilder.Entity<Recipe>(b =>
        {
            b.ToTable("Recipes", "inventory");
            b.Property(x => x.YieldQty).HasPrecision(18, 6);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<RecipeLine>(b =>
        {
            b.ToTable("RecipeLines", "inventory");
            b.Property(x => x.Quantity).HasPrecision(18, 6);
            b.Property(x => x.WastePercent).HasPrecision(18, 2);
        });

        // Schema purchasing
        modelBuilder.Entity<Supplier>(b =>
        {
            b.ToTable("Suppliers", "purchasing");
            b.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
            b.HasIndex(x => new { x.RestaurantId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<PurchaseOrder>(b =>
        {
            b.ToTable("PurchaseOrders", "purchasing");
            b.Property(x => x.Total).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<PurchaseOrderLine>(b =>
        {
            b.ToTable("PurchaseOrderLines", "purchasing");
            b.Property(x => x.Qty).HasPrecision(18, 6);
            b.Property(x => x.UnitCost).HasPrecision(18, 6);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<GoodsReceipt>(b =>
        {
            b.ToTable("GoodsReceipts", "purchasing");
            b.Property(x => x.Total).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
            b.HasIndex(x => new { x.BranchId, x.GRNNo }).IsUnique();
        });

        modelBuilder.Entity<GoodsReceiptLine>(b =>
        {
            b.ToTable("GoodsReceiptLines", "purchasing");
            b.Property(x => x.Qty).HasPrecision(18, 6);
            b.Property(x => x.UnitCost).HasPrecision(18, 6);
        });

        modelBuilder.Entity<PurchaseReturn>(b =>
        {
            b.ToTable("PurchaseReturns", "purchasing");
            b.Property(x => x.Total).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        // Schema finance
        modelBuilder.Entity<CashShift>(b =>
        {
            b.ToTable("CashShifts", "finance");
            b.Property(x => x.OpeningCash).HasPrecision(18, 2);
            b.Property(x => x.ClosingCash).HasPrecision(18, 2);
            b.Property(x => x.ExpectedCash).HasPrecision(18, 2);
            b.Property(x => x.Variance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CashMovement>(b =>
        {
            b.ToTable("CashMovements", "finance");
            b.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ExpenseCategory>().ToTable("ExpenseCategories", "finance");
        modelBuilder.Entity<Expense>(b =>
        {
            b.ToTable("Expenses", "finance");
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        // Schema tax
        modelBuilder.Entity<TaxCategory>(b =>
        {
            b.ToTable("TaxCategories", "tax");
            b.Property(x => x.Rate).HasPrecision(18, 2);
            b.HasIndex(x => new { x.RestaurantId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<TaxInvoice>(b =>
        {
            b.ToTable("TaxInvoices", "tax");
            b.Property(x => x.TaxableAmount).HasPrecision(18, 2);
            b.Property(x => x.VATAmount).HasPrecision(18, 2);
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.HasIndex(x => x.InvoiceNo).IsUnique();
        });

        // Schema crm
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers", "crm");
            b.Property(x => x.RowVersion).IsRowVersion();
            b.HasIndex(x => new { x.RestaurantId, x.Phone });
        });

        modelBuilder.Entity<CustomerAddress>().ToTable("CustomerAddresses", "crm");

        modelBuilder.Entity<LoyaltyAccount>(b =>
        {
            b.ToTable("LoyaltyAccounts", "crm");
            b.Property(x => x.PointsBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<LoyaltyTransaction>(b =>
        {
            b.ToTable("LoyaltyTransactions", "crm");
            b.Property(x => x.Points).HasPrecision(18, 2);
        });

        // Schema identity
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users", "identity");
            b.HasIndex(x => new { x.RestaurantId, x.UserName }).IsUnique();
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Role>().ToTable("Roles", "identity");
        modelBuilder.Entity<Permission>().ToTable("Permissions", "identity");
        modelBuilder.Entity<UserRole>().ToTable("UserRoles", "identity");
        modelBuilder.Entity<RolePermission>().ToTable("RolePermissions", "identity");

        // Schema audit
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs", "audit");
            b.HasIndex(x => new { x.RestaurantId, x.CreatedAtUtc });
            b.HasIndex(x => new { x.EntityName, x.EntityId });
        });

        // Global disable cascade deletes on specific relationships to avoid cycles in SQL Server
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
