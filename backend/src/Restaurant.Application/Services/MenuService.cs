using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Application.Services;

public class MenuService : IMenuService
{
    private readonly IAppDbContext _db;

    public MenuService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(long restaurantId)
    {
        return await _db.Categories
            .Where(c => c.RestaurantId == restaurantId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(c.Id, c.Name, c.SortOrder, c.ParentId, c.IsActive))
            .ToListAsync();
    }

    public async Task<List<ProductDto>> GetProductsAsync(long restaurantId, long? categoryId = null)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.ProductModifiers)
                .ThenInclude(pm => pm.Modifier)
                    .ThenInclude(m => m!.Options)
            .Where(p => p.RestaurantId == restaurantId && p.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await query.ToListAsync();

        return products.Select(p => new ProductDto(
            p.Id,
            p.CategoryId,
            p.Category?.Name ?? "",
            p.SKU,
            p.Name,
            p.ShortName,
            p.Description,
            p.BasePrice,
            p.CostPrice,
            p.IsTaxInclusive,
            p.IsActive,
            p.Variants.Where(v => v.IsActive).Select(v => new ProductVariantDto(v.Id, v.Name, v.SKU, v.PriceDelta, v.IsActive)).ToList(),
            p.ProductModifiers.Where(pm => pm.Modifier != null && pm.Modifier.IsActive).Select(pm => new ModifierDto(
                pm.Modifier!.Id,
                pm.Modifier.Name,
                pm.Modifier.IsRequired,
                pm.Modifier.MinSelect,
                pm.Modifier.MaxSelect,
                pm.Modifier.Options.Where(o => o.IsActive).Select(o => new ModifierOptionDto(o.Id, o.Name, o.PriceDelta, o.IsActive)).ToList()
            )).ToList()
        )).ToList();
    }

    public async Task<RecipeDto> GetRecipeByProductIdAsync(long productId)
    {
        var product = await _db.Products.FindAsync(productId) ?? throw new KeyNotFoundException("Product not found");
        var recipe = await _db.Recipes
            .Include(r => r.Unit)
            .Include(r => r.Lines).ThenInclude(l => l.InventoryItem).ThenInclude(ii => ii!.BaseUnit)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        if (recipe == null)
        {
            return new RecipeDto(0, productId, product.Name, 1, 1, 1, "PCS", 0, product.BasePrice, 100, new List<RecipeLineDto>());
        }

        var balances = await _db.StockBalances.ToListAsync();

        var lines = recipe.Lines.Select(l =>
        {
            var unitCost = balances.FirstOrDefault(b => b.InventoryItemId == l.InventoryItemId)?.AverageCost ?? 0m;
            var lineCost = l.Quantity * unitCost * (1m + (l.WastePercent / 100m));
            return new RecipeLineDto(
                l.Id,
                l.InventoryItemId,
                l.InventoryItem?.Name ?? "",
                l.InventoryItem?.BaseUnit?.Code ?? "",
                l.Quantity,
                l.WastePercent,
                lineCost
            );
        }).ToList();

        var totalCost = lines.Sum(l => l.ComponentCost);
        var grossMargin = product.BasePrice > 0 ? Math.Round(((product.BasePrice - totalCost) / product.BasePrice) * 100m, 2) : 0m;

        return new RecipeDto(
            recipe.Id,
            productId,
            product.Name,
            recipe.VersionNo,
            recipe.YieldQty,
            recipe.UnitId,
            recipe.Unit?.Code ?? "PCS",
            totalCost,
            product.BasePrice,
            grossMargin,
            lines
        );
    }
}
