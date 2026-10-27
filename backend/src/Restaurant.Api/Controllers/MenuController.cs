using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class MenuController : ApiControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService) => _menuService = menuService;

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] long restaurantId = 1)
    {
        return Ok(await _menuService.GetCategoriesAsync(restaurantId));
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<ProductDto>>> GetProducts([FromQuery] long restaurantId = 1, [FromQuery] long? categoryId = null)
    {
        return Ok(await _menuService.GetProductsAsync(restaurantId, categoryId));
    }

    [HttpGet("recipes/{productId}")]
    public async Task<ActionResult<RecipeDto>> GetRecipe(long productId)
    {
        return Ok(await _menuService.GetRecipeByProductIdAsync(productId));
    }
}
