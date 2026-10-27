using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(long restaurantId);
}
