using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Application.Services;

public class CrmService : ICrmService
{
    private readonly IAppDbContext _db;

    public CrmService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerDto>> GetCustomersAsync(long restaurantId, string? search = null)
    {
        var query = _db.Customers
            .Include(c => c.Addresses)
            .Include(c => c.LoyaltyAccount)
            .Where(c => c.RestaurantId == restaurantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) || c.Phone.Contains(search));
        }

        var customers = await query.ToListAsync();

        return customers.Select(c => new CustomerDto(
            c.Id,
            c.Code,
            c.Name,
            c.Phone,
            c.Email,
            c.LoyaltyAccount?.PointsBalance ?? 0,
            c.LoyaltyAccount?.Tier ?? "Bronze",
            c.Addresses.Select(a => new CustomerAddressDto(a.Id, a.Label, a.AddressLine, a.District, a.Upazila, a.Area)).ToList()
        )).ToList();
    }
}
