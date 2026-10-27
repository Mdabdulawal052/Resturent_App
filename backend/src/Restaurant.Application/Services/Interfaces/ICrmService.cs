using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface ICrmService
{
    Task<List<CustomerDto>> GetCustomersAsync(long restaurantId, string? search = null);
}
