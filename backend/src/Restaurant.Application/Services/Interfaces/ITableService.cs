using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services.Interfaces;

public interface ITableService
{
    Task<List<DiningAreaDto>> GetDiningAreasAsync(long branchId);
    Task<TableDto> UpdateTableStatusAsync(long tableId, TableStatus status, long? orderId = null);
    Task<TableDto> UpdateTableLayoutAsync(UpdateTableLayoutRequest req);
    Task<bool> TransferTableAsync(TransferTableRequest req);
    Task<bool> MergeTablesAsync(MergeTablesRequest req);
    Task<DiningAreaDto> CreateDiningAreaAsync(long branchId, string name, int floorNumber);
    Task<TableDto> CreateTableAsync(long branchId, long diningAreaId, string tableNo, int capacity, string shape, double positionX = 100, double positionY = 100, double width = 140, double height = 140);
    Task<bool> DeleteTableAsync(long tableId);
}

