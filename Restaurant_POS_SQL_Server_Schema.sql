/*
 Bangladesh Restaurant POS - SQL Server starter schema
 Target: SQL Server 2022/2025 compatible
 Currency: BDT
 Time storage: UTC
 NOTE: This is a foundation script. Complete production DDL should be generated/managed
 through EF Core migrations and should add the remaining domain tables from the design document.
*/

CREATE SCHEMA pos;
GO
CREATE SCHEMA inventory;
GO
CREATE SCHEMA purchasing;
GO
CREATE SCHEMA kitchen;
GO
CREATE SCHEMA crm;
GO
CREATE SCHEMA finance;
GO
CREATE SCHEMA audit;
GO

CREATE TABLE pos.Restaurants (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    LegalName NVARCHAR(250) NULL,
    BIN NVARCHAR(50) NULL,
    TIN NVARCHAR(50) NULL,
    Phone NVARCHAR(50) NULL,
    Email NVARCHAR(200) NULL,
    CurrencyCode CHAR(3) NOT NULL DEFAULT 'BDT',
    TimeZoneId NVARCHAR(100) NOT NULL DEFAULT 'Asia/Dhaka',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION,
    CONSTRAINT UX_Restaurants_Code UNIQUE(Code)
);
GO

CREATE TABLE pos.Branches (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    BIN NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION,
    CONSTRAINT FK_Branches_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT UX_Branches_Restaurant_Code UNIQUE(RestaurantId, Code)
);
GO

CREATE TABLE pos.Terminals (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    BranchId BIGINT NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    DeviceKeyHash NVARCHAR(256) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION,
    CONSTRAINT FK_Terminals_Branches FOREIGN KEY(BranchId) REFERENCES pos.Branches(Id),
    CONSTRAINT UX_Terminals_Branch_Code UNIQUE(BranchId, Code)
);
GO

CREATE TABLE pos.Categories (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    ParentId BIGINT NULL,
    Name NVARCHAR(150) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Categories_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY(ParentId) REFERENCES pos.Categories(Id)
);
GO

CREATE TABLE pos.Products (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    CategoryId BIGINT NOT NULL,
    SKU NVARCHAR(80) NULL,
    Name NVARCHAR(200) NOT NULL,
    ShortName NVARCHAR(80) NULL,
    Description NVARCHAR(1000) NULL,
    BasePrice DECIMAL(18,2) NOT NULL,
    CostPrice DECIMAL(18,2) NULL,
    TaxCategoryId BIGINT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVersion ROWVERSION,
    CONSTRAINT FK_Products_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT FK_Products_Categories FOREIGN KEY(CategoryId) REFERENCES pos.Categories(Id)
);
GO
CREATE INDEX IX_Products_Category ON pos.Products(CategoryId, IsActive);
GO

CREATE TABLE pos.Orders (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    BranchId BIGINT NOT NULL,
    TerminalId BIGINT NULL,
    OrderNo NVARCHAR(50) NOT NULL,
    OrderType TINYINT NOT NULL, -- 1 DineIn, 2 Takeaway, 3 Delivery, 4 QuickSale
    TableId BIGINT NULL,
    CustomerId BIGINT NULL,
    Status TINYINT NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ServiceChargeAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    RoundingAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrandTotal DECIMAL(18,2) NOT NULL,
    OrderedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    PaidAtUtc DATETIME2(3) NULL,
    CreatedByUserId BIGINT NOT NULL,
    RowVersion ROWVERSION,
    CONSTRAINT UX_Orders_Branch_OrderNo UNIQUE(BranchId, OrderNo),
    CONSTRAINT CK_Orders_NonNegativeTotals CHECK
        (Subtotal >= 0 AND DiscountAmount >= 0 AND ServiceChargeAmount >= 0
         AND TaxAmount >= 0 AND GrandTotal >= 0)
);
GO
CREATE INDEX IX_Orders_Branch_Date ON pos.Orders(BranchId, OrderedAtUtc);
CREATE INDEX IX_Orders_Status_Date ON pos.Orders(Status, OrderedAtUtc);
GO

CREATE TABLE pos.OrderItems (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    VariantId BIGINT NULL,
    Qty DECIMAL(18,6) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL,
    KitchenStatus TINYINT NOT NULL DEFAULT 0,
    Note NVARCHAR(500) NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY(OrderId) REFERENCES pos.Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY(ProductId) REFERENCES pos.Products(Id),
    CONSTRAINT CK_OrderItems_Qty CHECK(Qty > 0)
);
GO
CREATE INDEX IX_OrderItems_Order ON pos.OrderItems(OrderId);
CREATE INDEX IX_OrderItems_Product ON pos.OrderItems(ProductId, OrderId);
GO

CREATE TABLE pos.PaymentMethods (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    MethodType TINYINT NOT NULL, -- Cash/Card/MFS/Bank/Other
    IsCash BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_PaymentMethods_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT UX_PaymentMethods_Code UNIQUE(RestaurantId, Code)
);
GO

CREATE TABLE pos.Payments (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    PaymentMethodId BIGINT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ReferenceNo NVARCHAR(150) NULL,
    PaidAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    Status TINYINT NOT NULL DEFAULT 1,
    CreatedByUserId BIGINT NOT NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY(OrderId) REFERENCES pos.Orders(Id),
    CONSTRAINT FK_Payments_Methods FOREIGN KEY(PaymentMethodId) REFERENCES pos.PaymentMethods(Id),
    CONSTRAINT CK_Payments_Amount CHECK(Amount > 0)
);
GO
CREATE INDEX IX_Payments_Order ON pos.Payments(OrderId);
CREATE INDEX IX_Payments_Method_Date ON pos.Payments(PaymentMethodId, PaidAtUtc);
GO

CREATE TABLE inventory.Units (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    Code NVARCHAR(30) NOT NULL,
    Name NVARCHAR(80) NOT NULL,
    CONSTRAINT FK_Units_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT UX_Units_Code UNIQUE(RestaurantId, Code)
);
GO

CREATE TABLE inventory.Items (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    SKU NVARCHAR(80) NULL,
    Name NVARCHAR(200) NOT NULL,
    BaseUnitId BIGINT NOT NULL,
    ItemType TINYINT NOT NULL, -- Ingredient/Packaging/Finished/Other
    ReorderLevel DECIMAL(18,6) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Items_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT FK_Items_Units FOREIGN KEY(BaseUnitId) REFERENCES inventory.Units(Id)
);
GO

CREATE TABLE inventory.Warehouses (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    BranchId BIGINT NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Warehouses_Branches FOREIGN KEY(BranchId) REFERENCES pos.Branches(Id),
    CONSTRAINT UX_Warehouses_Branch_Code UNIQUE(BranchId, Code)
);
GO

CREATE TABLE inventory.StockLedger (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    WarehouseId BIGINT NOT NULL,
    InventoryItemId BIGINT NOT NULL,
    TxnType TINYINT NOT NULL,
    ReferenceType NVARCHAR(50) NOT NULL,
    ReferenceId BIGINT NULL,
    QtyIn DECIMAL(18,6) NOT NULL DEFAULT 0,
    QtyOut DECIMAL(18,6) NOT NULL DEFAULT 0,
    UnitCost DECIMAL(18,6) NOT NULL DEFAULT 0,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId BIGINT NOT NULL,
    CONSTRAINT FK_StockLedger_Warehouse FOREIGN KEY(WarehouseId) REFERENCES inventory.Warehouses(Id),
    CONSTRAINT FK_StockLedger_Item FOREIGN KEY(InventoryItemId) REFERENCES inventory.Items(Id),
    CONSTRAINT CK_StockLedger_Qty CHECK(QtyIn >= 0 AND QtyOut >= 0 AND NOT(QtyIn > 0 AND QtyOut > 0))
);
GO
CREATE INDEX IX_StockLedger_Item_Warehouse_Date
ON inventory.StockLedger(WarehouseId, InventoryItemId, CreatedAtUtc);
GO

CREATE TABLE purchasing.Suppliers (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Email NVARCHAR(200) NULL,
    Address NVARCHAR(500) NULL,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Suppliers_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id),
    CONSTRAINT UX_Suppliers_Code UNIQUE(RestaurantId, Code)
);
GO

CREATE TABLE crm.Customers (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    Code NVARCHAR(50) NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Email NVARCHAR(200) NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Customers_Restaurants FOREIGN KEY(RestaurantId) REFERENCES pos.Restaurants(Id)
);
GO
CREATE INDEX IX_Customers_Phone ON crm.Customers(RestaurantId, Phone);
GO

CREATE TABLE audit.AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId BIGINT NOT NULL,
    BranchId BIGINT NULL,
    UserId BIGINT NULL,
    Action NVARCHAR(100) NOT NULL,
    EntityName NVARCHAR(150) NOT NULL,
    EntityId BIGINT NULL,
    OldValuesJson NVARCHAR(MAX) NULL,
    NewValuesJson NVARCHAR(MAX) NULL,
    CreatedAtUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
CREATE INDEX IX_AuditLogs_Restaurant_Date ON audit.AuditLogs(RestaurantId, CreatedAtUtc);
CREATE INDEX IX_AuditLogs_Entity ON audit.AuditLogs(EntityName, EntityId);
GO
