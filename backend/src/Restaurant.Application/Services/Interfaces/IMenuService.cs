using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface IMenuService
{
    Task<List<CategoryDto>> GetCategoriesAsync(long restaurantId);
    Task<List<ProductDto>> GetProductsAsync(long restaurantId, long? categoryId = null);
    Task<RecipeDto> GetRecipeByProductIdAsync(long productId);
}
