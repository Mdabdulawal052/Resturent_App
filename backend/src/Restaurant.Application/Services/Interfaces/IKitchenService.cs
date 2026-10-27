using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services.Interfaces;

public interface IKitchenService
{
    Task<List<KOTDto>> GetActiveKOTsAsync(long branchId, long? stationId = null);
    Task<KOTDto> UpdateKOTStatusAsync(long kotId, KitchenStatus status);
}
