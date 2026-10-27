using Restaurant.Domain.Common;

namespace Restaurant.Domain.Entities;

public class Category : BaseEntity
{
    public long RestaurantId { get; set; }
    public RestaurantEntity? Restaurant { get; set; }
    public long? ParentId { get; set; }
    public Category? Parent { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product : AuditableEntity
{
    public long RestaurantId { get; set; }
    public RestaurantEntity? Restaurant { get; set; }
    public long CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? SKU { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public long? TaxCategoryId { get; set; }
    public bool IsTaxInclusive { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductModifier> ProductModifiers { get; set; } = new List<ProductModifier>();
}

public class ProductVariant : BaseEntity
{
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. Small, Medium, Large
    public string? SKU { get; set; }
    public decimal PriceDelta { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Modifier : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. Spice Level, Extra Toppings
    public bool IsRequired { get; set; }
    public int MinSelect { get; set; } = 0;
    public int MaxSelect { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<ModifierOption> Options { get; set; } = new List<ModifierOption>();
}

public class ModifierOption : BaseEntity
{
    public long ModifierId { get; set; }
    public Modifier? Modifier { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. Extra Cheese, Mild, Spicy
    public decimal PriceDelta { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductModifier : BaseEntity
{
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    public long ModifierId { get; set; }
    public Modifier? Modifier { get; set; }
}

public class Combo : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
