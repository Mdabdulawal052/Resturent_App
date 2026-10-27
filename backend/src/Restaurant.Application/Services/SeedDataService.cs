using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class SeedDataService
{
    private readonly IAppDbContext _db;

    public SeedDataService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        await SeedBookingsIfEmptyAsync();

        if (await _db.Restaurants.AnyAsync()) return; // Already seeded

        // 1. Restaurant
        var rest = new RestaurantEntity
        {
            Code = "REST-001",
            Name = "Sultan's Grand Dine & Café",
            LegalName = "Sultans Food & Hospitality Ltd.",
            BIN = "002345678-0101", // Bangladesh NBR BIN
            TIN = "492837465192",
            Phone = "+8801711223344",
            Email = "contact@sultansdinebd.com",
            CurrencyCode = "BDT",
            TimeZoneId = "Asia/Dhaka",
            IsActive = true
        };
        _db.Restaurants.Add(rest);
        await _db.SaveChangesAsync();

        // 2. Branch
        var branch = new Branch
        {
            RestaurantId = rest.Id,
            Code = "DHN-01",
            Name = "Dhanmondi Branch (Satmasjid Road)",
            Phone = "+8801811223344",
            Address = "Plot 34, Satmasjid Road, Dhanmondi, Dhaka 1209",
            BIN = "002345678-0101",
            IsActive = true
        };
        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();

        // 3. Terminals
        _db.Terminals.AddRange(
            new Terminal { BranchId = branch.Id, Code = "TERM-01", Name = "Cash Counter 1", IsActive = true },
            new Terminal { BranchId = branch.Id, Code = "TERM-02", Name = "Takeaway Counter 2", IsActive = true }
        );

        // 4. Warehouse
        var warehouse = new Warehouse
        {
            BranchId = branch.Id,
            Code = "WH-DHN-MAIN",
            Name = "Dhanmondi Central Kitchen Store",
            IsActive = true
        };
        _db.Warehouses.Add(warehouse);

        // 5. Kitchen Stations
        var grillStation = new KitchenStation { BranchId = branch.Id, Name = "Grill & Fry Station", IsActive = true };
        var mainKitchen = new KitchenStation { BranchId = branch.Id, Name = "Main Biryani & Curry Kitchen", IsActive = true };
        var barStation = new KitchenStation { BranchId = branch.Id, Name = "Beverage & Dessert Bar", IsActive = true };
        _db.KitchenStations.AddRange(grillStation, mainKitchen, barStation);

        // 6. Payment Methods (Bangladesh ecosystem)
        _db.PaymentMethods.AddRange(
            new PaymentMethod { RestaurantId = rest.Id, Code = "CASH", Name = "Cash (BDT)", MethodType = PaymentMethodType.Cash, IsCash = true },
            new PaymentMethod { RestaurantId = rest.Id, Code = "BKASH", Name = "bKash Merchant", MethodType = PaymentMethodType.MFS, IsCash = false },
            new PaymentMethod { RestaurantId = rest.Id, Code = "NAGAD", Name = "Nagad", MethodType = PaymentMethodType.MFS, IsCash = false },
            new PaymentMethod { RestaurantId = rest.Id, Code = "ROCKET", Name = "DBBL Rocket", MethodType = PaymentMethodType.MFS, IsCash = false },
            new PaymentMethod { RestaurantId = rest.Id, Code = "VISA_MC", Name = "Debit / Credit Card (POS)", MethodType = PaymentMethodType.Card, IsCash = false }
        );

        // 7. Tax Categories
        _db.TaxCategories.AddRange(
            new TaxCategory { RestaurantId = rest.Id, Code = "VAT-5", Name = "Standard Restaurant VAT (5%)", Rate = 5.0m, IsInclusive = true },
            new TaxCategory { RestaurantId = rest.Id, Code = "VAT-7.5", Name = "Restaurant AC VAT (7.5%)", Rate = 7.5m, IsInclusive = false },
            new TaxCategory { RestaurantId = rest.Id, Code = "VAT-15", Name = "Standard Rate (15%)", Rate = 15.0m, IsInclusive = false }
        );

        // 8. Dining Areas & Tables
        var ground = new DiningArea { BranchId = branch.Id, Name = "Ground Floor Hall", SortOrder = 1, IsActive = true };
        var rooftop = new DiningArea { BranchId = branch.Id, Name = "Rooftop Garden Terrace", SortOrder = 2, IsActive = true };
        _db.DiningAreas.AddRange(ground, rooftop);
        await _db.SaveChangesAsync();

        for (int i = 1; i <= 6; i++)
        {
            _db.Tables.Add(new RestaurantTable { BranchId = branch.Id, DiningAreaId = ground.Id, TableNo = $"G-{i:00}", Capacity = 4, Status = TableStatus.Available });
        }
        for (int i = 1; i <= 6; i++)
        {
            _db.Tables.Add(new RestaurantTable { BranchId = branch.Id, DiningAreaId = rooftop.Id, TableNo = $"R-{i:00}", Capacity = 6, Status = TableStatus.Available });
        }

        // 9. Categories
        var catBiryani = new Category { RestaurantId = rest.Id, Name = "Kacchi & Biryani", SortOrder = 1, IsActive = true };
        var catBurger = new Category { RestaurantId = rest.Id, Name = "Burgers & Sandwiches", SortOrder = 2, IsActive = true };
        var catGrill = new Category { RestaurantId = rest.Id, Name = "Grill, BBQ & Kebabs", SortOrder = 3, IsActive = true };
        var catDrinks = new Category { RestaurantId = rest.Id, Name = "Beverages & Borhani", SortOrder = 4, IsActive = true };
        var catDesserts = new Category { RestaurantId = rest.Id, Name = "Traditional Desserts", SortOrder = 5, IsActive = true };
        _db.Categories.AddRange(catBiryani, catBurger, catGrill, catDrinks, catDesserts);
        await _db.SaveChangesAsync();

        // 10. Modifiers
        var modBurger = new Modifier { RestaurantId = rest.Id, Name = "Burger Add-ons", IsRequired = false, MinSelect = 0, MaxSelect = 3, IsActive = true };
        modBurger.Options.Add(new ModifierOption { Name = "Extra Melted Cheddar", PriceDelta = 50m, IsActive = true });
        modBurger.Options.Add(new ModifierOption { Name = "Crispy Beef Bacon", PriceDelta = 80m, IsActive = true });
        modBurger.Options.Add(new ModifierOption { Name = "Pickled Jalapeños", PriceDelta = 30m, IsActive = true });

        var modSpice = new Modifier { RestaurantId = rest.Id, Name = "Spice Level", IsRequired = true, MinSelect = 1, MaxSelect = 1, IsActive = true };
        modSpice.Options.Add(new ModifierOption { Name = "Mild", PriceDelta = 0m, IsActive = true });
        modSpice.Options.Add(new ModifierOption { Name = "Regular Spicy", PriceDelta = 0m, IsActive = true });
        modSpice.Options.Add(new ModifierOption { Name = "Naga Hot (Very Spicy)", PriceDelta = 20m, IsActive = true });

        _db.Modifiers.AddRange(modBurger, modSpice);
        await _db.SaveChangesAsync();

        // 11. Products
        var p1 = new Product
        {
            RestaurantId = rest.Id,
            CategoryId = catBiryani.Id,
            SKU = "KB-01",
            Name = "Sultan's Special Mutton Kacchi Biryani",
            ShortName = "Mutton Kacchi",
            Description = "Aromatic Chinigura/Basmati rice cooked in dum with tender baby mutton & spiced potato.",
            BasePrice = 450m,
            CostPrice = 220m,
            IsTaxInclusive = true,
            IsActive = true
        };
        p1.Variants.Add(new ProductVariant { Name = "Regular (1:1)", PriceDelta = 0m, SKU = "KB-01-R", IsActive = true });
        p1.Variants.Add(new ProductVariant { Name = "Jumbo (1:2)", PriceDelta = 350m, SKU = "KB-01-J", IsActive = true });

        var p2 = new Product
        {
            RestaurantId = rest.Id,
            CategoryId = catBurger.Id,
            SKU = "BB-02",
            Name = "Double Patty Gourmet Beef Burger",
            ShortName = "Beef Burger",
            Description = "100% prime beef patties with caramelized onions, cheddar cheese & signature sauce in brioche bun.",
            BasePrice = 360m,
            CostPrice = 165m,
            IsTaxInclusive = true,
            IsActive = true
        };
        p2.ProductModifiers.Add(new ProductModifier { ModifierId = modBurger.Id });
        p2.ProductModifiers.Add(new ProductModifier { ModifierId = modSpice.Id });

        var p3 = new Product
        {
            RestaurantId = rest.Id,
            CategoryId = catGrill.Id,
            SKU = "GC-03",
            Name = "Flame-Grilled Quarter Chicken",
            ShortName = "Grill Chicken",
            Description = "Smoky quarter chicken served with garlic sauce and naan bread.",
            BasePrice = 220m,
            CostPrice = 95m,
            IsTaxInclusive = true,
            IsActive = true
        };

        var p4 = new Product
        {
            RestaurantId = rest.Id,
            CategoryId = catDrinks.Id,
            SKU = "DR-04",
            Name = "Special Shahi Borhani (Glass)",
            ShortName = "Shahi Borhani",
            Description = "Traditional spiced yogurt digestive drink with mint, coriander, mustard and roasted spices.",
            BasePrice = 90m,
            CostPrice = 30m,
            IsTaxInclusive = true,
            IsActive = true
        };

        var p5 = new Product
        {
            RestaurantId = rest.Id,
            CategoryId = catDesserts.Id,
            SKU = "DS-05",
            Name = "Zafrani Matka Firni",
            ShortName = "Matka Firni",
            Description = "Rich ground rice pudding infused with saffron, pistachio and cardamom in clay pot.",
            BasePrice = 120m,
            CostPrice = 40m,
            IsTaxInclusive = true,
            IsActive = true
        };

        _db.Products.AddRange(p1, p2, p3, p4, p5);
        await _db.SaveChangesAsync();

        // 12. Units
        var uKg = new Unit { RestaurantId = rest.Id, Code = "KG", Name = "Kilogram", ConversionFactor = 1000m };
        var uGram = new Unit { RestaurantId = rest.Id, Code = "G", Name = "Gram", ConversionFactor = 1m };
        var uLiter = new Unit { RestaurantId = rest.Id, Code = "L", Name = "Liter", ConversionFactor = 1000m };
        var uPcs = new Unit { RestaurantId = rest.Id, Code = "PCS", Name = "Pieces", ConversionFactor = 1m };
        _db.Units.AddRange(uKg, uGram, uLiter, uPcs);
        await _db.SaveChangesAsync();

        // 13. Inventory Raw Materials
        var itemRice = new InventoryItem { RestaurantId = rest.Id, Name = "Premium Basmati Rice", BaseUnitId = uKg.Id, ItemType = ItemType.Ingredient, ReorderLevel = 50m };
        var itemMutton = new InventoryItem { RestaurantId = rest.Id, Name = "Fresh Baby Mutton", BaseUnitId = uKg.Id, ItemType = ItemType.Ingredient, ReorderLevel = 30m };
        var itemPatty = new InventoryItem { RestaurantId = rest.Id, Name = "Pre-formed Beef Patty 150g", BaseUnitId = uPcs.Id, ItemType = ItemType.Ingredient, ReorderLevel = 100m };
        var itemBun = new InventoryItem { RestaurantId = rest.Id, Name = "Brioche Burger Buns", BaseUnitId = uPcs.Id, ItemType = ItemType.Ingredient, ReorderLevel = 100m };
        var itemCheese = new InventoryItem { RestaurantId = rest.Id, Name = "Cheddar Cheese Slices", BaseUnitId = uPcs.Id, ItemType = ItemType.Ingredient, ReorderLevel = 50m };
        _db.InventoryItems.AddRange(itemRice, itemMutton, itemPatty, itemBun, itemCheese);
        await _db.SaveChangesAsync();

        // 14. Stock Balances
        _db.StockBalances.AddRange(
            new StockBalance { WarehouseId = warehouse.Id, InventoryItemId = itemRice.Id, Quantity = 250m, AverageCost = 140m },
            new StockBalance { WarehouseId = warehouse.Id, InventoryItemId = itemMutton.Id, Quantity = 120m, AverageCost = 950m },
            new StockBalance { WarehouseId = warehouse.Id, InventoryItemId = itemPatty.Id, Quantity = 300m, AverageCost = 75m },
            new StockBalance { WarehouseId = warehouse.Id, InventoryItemId = itemBun.Id, Quantity = 350m, AverageCost = 25m },
            new StockBalance { WarehouseId = warehouse.Id, InventoryItemId = itemCheese.Id, Quantity = 200m, AverageCost = 18m }
        );

        // 15. Recipes (BOM)
        var recipeBurger = new Recipe
        {
            ProductId = p2.Id,
            VersionNo = 1,
            YieldQty = 1m,
            UnitId = uPcs.Id,
            IsActive = true
        };
        recipeBurger.Lines.Add(new RecipeLine { InventoryItemId = itemBun.Id, Quantity = 1m, WastePercent = 2m });
        recipeBurger.Lines.Add(new RecipeLine { InventoryItemId = itemPatty.Id, Quantity = 2m, WastePercent = 0m });
        recipeBurger.Lines.Add(new RecipeLine { InventoryItemId = itemCheese.Id, Quantity = 1m, WastePercent = 0m });
        _db.Recipes.Add(recipeBurger);

        // 16. Customers
        var c1 = new Customer { RestaurantId = rest.Id, Name = "Tanvir Rahman", Phone = "01711998877", Email = "tanvir@gmail.com" };
        c1.Addresses.Add(new CustomerAddress { Label = "Home", AddressLine = "House 12, Road 4, Dhanmondi", District = "Dhaka", Area = "Dhanmondi" });
        c1.LoyaltyAccount = new LoyaltyAccount { PointsBalance = 150m, Tier = "Silver" };

        var c2 = new Customer { RestaurantId = rest.Id, Name = "Nusrat Jahan", Phone = "01819556677", Email = "nusrat@gmail.com" };
        c2.Addresses.Add(new CustomerAddress { Label = "Office", AddressLine = "Gulshan Tower, Level 8", District = "Dhaka", Area = "Gulshan-1" });
        c2.LoyaltyAccount = new LoyaltyAccount { PointsBalance = 320m, Tier = "Gold" };

        _db.Customers.AddRange(c1, c2);

        // 17. Expense Categories
        _db.ExpenseCategories.AddRange(
            new ExpenseCategory { RestaurantId = rest.Id, Name = "Daily Raw Produce & Grocery" },
            new ExpenseCategory { RestaurantId = rest.Id, Name = "Gas & Electricity Utility" },
            new ExpenseCategory { RestaurantId = rest.Id, Name = "Cleaning & Kitchen Sanitation" },
            new ExpenseCategory { RestaurantId = rest.Id, Name = "Staff Tea, Snacks & Misc" }
        );

        // 18. Default Users
        _db.Users.AddRange(
            new User { RestaurantId = rest.Id, UserName = "admin", DisplayName = "System Administrator", Phone = "01700000000", PinHash = "1234", PasswordHash = "AQAAAAIAAYagAAAAEI..." },
            new User { RestaurantId = rest.Id, UserName = "cashier1", DisplayName = "Kamrul Hasan (Cashier)", Phone = "01711111111", PinHash = "0000", PasswordHash = "AQAAAAIAAYagAAAAEI..." }
        );

        // 19. Sourcing Suppliers (Bangladesh Market)
        var sup1 = new Supplier { RestaurantId = rest.Id, Code = "SUP-001", Name = "Bengal Meat Agro Ltd.", Phone = "01711002233", Email = "supply@bengalmeat.com", Address = "Hazaribagh / Savar Plant, Dhaka", OpeningBalance = 0, IsActive = true };
        var sup2 = new Supplier { RestaurantId = rest.Id, Code = "SUP-002", Name = "Pran Dairy & Agro Supplies", Phone = "01811445566", Email = "sales@pranbd.com", Address = "Pran-RFL Centre, Middle Badda, Dhaka", OpeningBalance = 0, IsActive = true };
        var sup3 = new Supplier { RestaurantId = rest.Id, Code = "SUP-003", Name = "Karwan Bazar Fresh Produce Vendor", Phone = "01911778899", Email = "karwanbazar.fresh@gmail.com", Address = "Shed 4, Karwan Bazar Wholesale Market, Dhaka", OpeningBalance = 0, IsActive = true };
        var sup4 = new Supplier { RestaurantId = rest.Id, Code = "SUP-004", Name = "Moulvibazar Spices & Rice Syndicate", Phone = "01611223344", Email = "moulvibazar.spices@gmail.com", Address = "Moulvibazar, Old Dhaka", OpeningBalance = 0, IsActive = true };
        _db.Suppliers.AddRange(sup1, sup2, sup3, sup4);
        await _db.SaveChangesAsync();

        // 20. Sample Purchase Order
        var po1 = new PurchaseOrder
        {
            BranchId = branch.Id,
            SupplierId = sup1.Id,
            PONo = "PO-202610-0001",
            PODateUtc = DateTime.UtcNow.AddDays(-2),
            Status = PurchaseOrderStatus.Received,
            Total = 114000m
        };
        po1.Lines.Add(new PurchaseOrderLine { InventoryItemId = itemMutton.Id, Qty = 120m, UnitCost = 950m, TaxAmount = 0m });

        var po2 = new PurchaseOrder
        {
            BranchId = branch.Id,
            SupplierId = sup4.Id,
            PONo = "PO-202610-0002",
            PODateUtc = DateTime.UtcNow.AddDays(-1),
            Status = PurchaseOrderStatus.Approved,
            Total = 35000m
        };
        po2.Lines.Add(new PurchaseOrderLine { InventoryItemId = itemRice.Id, Qty = 250m, UnitCost = 140m, TaxAmount = 0m });

        _db.PurchaseOrders.AddRange(po1, po2);
        await _db.SaveChangesAsync();
    }

    public async Task SeedBookingsIfEmptyAsync()
    {
        try
        {
            if (await _db.Bookings.AnyAsync()) return;

            var branch = await _db.Branches.FirstOrDefaultAsync();
            if (branch == null) return;

            var tables = await _db.Tables.OrderBy(t => t.Id).Take(10).ToListAsync();
            if (tables.Count < 4) return;

            var t1 = tables[0];
            var t2 = tables[1];
            var t4 = tables.Count > 3 ? tables[3] : tables[0];
            var t6 = tables.Count > 5 ? tables[5] : tables[1];

            var today = DateTime.UtcNow.Date;

            var bookings = new List<TableBooking>
            {
                new TableBooking
                {
                    BranchId = branch.Id,
                    BookingNo = "BKG-202610-001",
                    CustomerName = "Tanvir Ahmed Chowdhury",
                    CustomerPhone = "01712345678",
                    CustomerEmail = "tanvir.ahmed@gmail.com",
                    BookingType = "Table",
                    TableId = t1.Id,
                    TableNo = t1.TableNo,
                    SeatNumber = null,
                    GuestCount = 4,
                    BookingDate = today,
                    TimeSlot = "Dinner (08:00 PM - 09:30 PM)",
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(21, 30, 0),
                    Status = "Confirmed",
                    DepositAmount = 1000m,
                    IsDepositPaid = true,
                    SpecialRequests = "Family anniversary dinner, quiet window table requested.",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-3)
                },
                new TableBooking
                {
                    BranchId = branch.Id,
                    BookingNo = "BKG-202610-002",
                    CustomerName = "Dr. Shamim Reza",
                    CustomerPhone = "01819876543",
                    CustomerEmail = "shamim.reza@bsmmu.edu.bd",
                    BookingType = "Chair",
                    TableId = t4.Id,
                    TableNo = t4.TableNo,
                    SeatNumber = 1,
                    GuestCount = 1,
                    BookingDate = today,
                    TimeSlot = "Dinner (08:00 PM - 09:30 PM)",
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(21, 30, 0),
                    Status = "Confirmed",
                    DepositAmount = 300m,
                    IsDepositPaid = true,
                    SpecialRequests = "Solo doctor dining, quiet corner seat",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
                },
                new TableBooking
                {
                    BranchId = branch.Id,
                    BookingNo = "BKG-202610-003",
                    CustomerName = "Farzana Yasmin",
                    CustomerPhone = "01911223388",
                    CustomerEmail = "farzana.y@outlook.com",
                    BookingType = "Chair",
                    TableId = t4.Id,
                    TableNo = t4.TableNo,
                    SeatNumber = 2,
                    GuestCount = 1,
                    BookingDate = today,
                    TimeSlot = "Dinner (08:00 PM - 09:30 PM)",
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(21, 30, 0),
                    Status = "Confirmed",
                    DepositAmount = 300m,
                    IsDepositPaid = true,
                    SpecialRequests = "Individual chair booking",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-1)
                },
                new TableBooking
                {
                    BranchId = branch.Id,
                    BookingNo = "BKG-202610-004",
                    CustomerName = "Engr. Mahmudul Hasan",
                    CustomerPhone = "01671122334",
                    CustomerEmail = "mahmudul@techfirm.bd",
                    BookingType = "Table",
                    TableId = t6.Id,
                    TableNo = t6.TableNo,
                    SeatNumber = null,
                    GuestCount = 6,
                    BookingDate = today.AddDays(1),
                    TimeSlot = "Lunch (01:00 PM - 02:30 PM)",
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Status = "Confirmed",
                    DepositAmount = 1500m,
                    IsDepositPaid = true,
                    SpecialRequests = "Corporate lunch meeting, VIP setup.",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-5)
                },
                new TableBooking
                {
                    BranchId = branch.Id,
                    BookingNo = "BKG-202610-005",
                    CustomerName = "Nusrat Jahan & Friend",
                    CustomerPhone = "01755667788",
                    CustomerEmail = "nusrat.j@gmail.com",
                    BookingType = "Table",
                    TableId = t2.Id,
                    TableNo = t2.TableNo,
                    SeatNumber = null,
                    GuestCount = 2,
                    BookingDate = today,
                    TimeSlot = "Lunch (01:00 PM - 02:30 PM)",
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(14, 30, 0),
                    Status = "Seated",
                    DepositAmount = 500m,
                    IsDepositPaid = true,
                    SpecialRequests = "Couple high table seating",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-4)
                }
            };

            _db.Bookings.AddRange(bookings);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Ignore if table not yet created during initial migration
        }
    }
}
