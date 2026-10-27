using Microsoft.EntityFrameworkCore;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<RestaurantEntity> Restaurants { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Terminal> Terminals { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Modifier> Modifiers { get; }
    DbSet<ModifierOption> ModifierOptions { get; }
    DbSet<ProductModifier> ProductModifiers { get; }
    DbSet<Combo> Combos { get; }
    DbSet<DiningArea> DiningAreas { get; }
    DbSet<RestaurantTable> Tables { get; }
    DbSet<TableSession> TableSessions { get; }
    DbSet<TableBooking> Bookings { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderItemModifier> OrderItemModifiers { get; }
    DbSet<OrderDiscount> OrderDiscounts { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<NumberSequence> NumberSequences { get; }
    DbSet<Setting> Settings { get; }

    DbSet<KitchenStation> KitchenStations { get; }
    DbSet<KOT> KOTs { get; }
    DbSet<KOTItem> KOTItems { get; }

    DbSet<Unit> Units { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<StockBalance> StockBalances { get; }
    DbSet<StockLedger> StockLedger { get; }
    DbSet<Recipe> Recipes { get; }
    DbSet<RecipeLine> RecipeLines { get; }

    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptLine> GoodsReceiptLines { get; }
    DbSet<PurchaseReturn> PurchaseReturns { get; }

    DbSet<CashShift> CashShifts { get; }
    DbSet<CashMovement> CashMovements { get; }
    DbSet<ExpenseCategory> ExpenseCategories { get; }
    DbSet<Expense> Expenses { get; }

    DbSet<TaxCategory> TaxCategories { get; }
    DbSet<TaxInvoice> TaxInvoices { get; }

    DbSet<Customer> Customers { get; }
    DbSet<CustomerAddress> CustomerAddresses { get; }
    DbSet<LoyaltyAccount> LoyaltyAccounts { get; }
    DbSet<LoyaltyTransaction> LoyaltyTransactions { get; }

    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
