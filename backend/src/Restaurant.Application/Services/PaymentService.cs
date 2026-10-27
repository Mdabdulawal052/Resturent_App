using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IAppDbContext _db;

    public PaymentService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(long restaurantId)
    {
        return await _db.PaymentMethods
            .Where(pm => pm.RestaurantId == restaurantId && pm.IsActive)
            .Select(pm => new PaymentMethodDto(pm.Id, pm.Code, pm.Name, pm.MethodType, pm.IsCash, pm.IsActive))
            .ToListAsync();
    }
}
