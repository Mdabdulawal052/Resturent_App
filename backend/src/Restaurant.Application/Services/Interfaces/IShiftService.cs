using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface IShiftService
{
    Task<CashShiftDto> OpenShiftAsync(OpenShiftRequest req, long userId);
    Task<CashShiftDto> CloseShiftAsync(CloseShiftRequest req);
}
