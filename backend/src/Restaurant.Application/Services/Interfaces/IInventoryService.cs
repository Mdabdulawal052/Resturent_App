using Restaurant.Application.DTOs;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Services.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryItemDto>> GetInventoryItemsAsync(long restaurantId);
    Task DeductInventoryForOrderAsync(Order order, long userId);
}
