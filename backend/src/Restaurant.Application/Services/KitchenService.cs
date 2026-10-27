using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class KitchenService : IKitchenService
{
    private readonly IAppDbContext _db;

    public KitchenService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<KOTDto>> GetActiveKOTsAsync(long branchId, long? stationId = null)
    {
        var query = _db.KOTs
            .Include(k => k.Order)
            .Include(k => k.Station)
            .Include(k => k.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Product)
            .Include(k => k.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Variant)
            .Include(k => k.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Modifiers).ThenInclude(m => m.ModifierOption)
            .Where(k => k.Station != null && k.Station.BranchId == branchId && k.Status != KitchenStatus.Served && k.Status != KitchenStatus.Cancelled);

        if (stationId.HasValue)
        {
            query = query.Where(k => k.StationId == stationId.Value);
        }

        var kots = await query.OrderBy(k => k.SentAtUtc).ToListAsync();

        return kots.Select(k => new KOTDto(
            k.Id,
            k.OrderId,
            k.Order?.OrderNo ?? "",
            k.Order?.TableId?.ToString(),
            k.Order?.OrderType ?? OrderType.DineIn,
            k.StationId,
            k.Station?.Name ?? "Kitchen",
            k.KOTNo,
            k.Status,
            k.SentAtUtc,
            k.Items.Select(i => new KOTItemDto(
                i.Id,
                i.OrderItemId,
                i.OrderItem?.Product?.Name ?? "",
                i.OrderItem?.Variant?.Name,
                i.Qty,
                i.Status,
                i.Note,
                i.OrderItem?.Modifiers.Select(m => m.ModifierOption?.Name ?? "").ToList() ?? new List<string>()
            )).ToList()
        )).ToList();
    }

    public async Task<KOTDto> UpdateKOTStatusAsync(long kotId, KitchenStatus status)
    {
        var kot = await _db.KOTs
            .Include(k => k.Items).ThenInclude(i => i.OrderItem)
            .FirstOrDefaultAsync(k => k.Id == kotId)
            ?? throw new KeyNotFoundException("KOT not found");

        kot.Status = status;
        if (status == KitchenStatus.Preparing && kot.StartedAtUtc == null)
        {
            kot.StartedAtUtc = DateTime.UtcNow;
        }
        else if (status == KitchenStatus.Ready || status == KitchenStatus.Served)
        {
            kot.CompletedAtUtc = DateTime.UtcNow;
        }

        foreach (var item in kot.Items)
        {
            item.Status = status;
            if (item.OrderItem != null)
            {
                item.OrderItem.KitchenStatus = status;
            }
        }

        await _db.SaveChangesAsync();
        var kots = await GetActiveKOTsAsync(1); // default active branch
        return kots.FirstOrDefault(k => k.Id == kotId) ?? throw new KeyNotFoundException();
    }
}
