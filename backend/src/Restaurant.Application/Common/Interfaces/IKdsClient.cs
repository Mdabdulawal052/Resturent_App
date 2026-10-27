using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Common.Interfaces;

public interface IKdsClient
{
    Task ReceiveNewKOT(KOTDto kot);
    Task ReceiveKOTStatusUpdate(long kotId, KitchenStatus status);
    Task ReceiveOrderStatusUpdate(long orderId, OrderStatus status);
    Task ReceiveTableStatusUpdate(long tableId, TableStatus status);
}
