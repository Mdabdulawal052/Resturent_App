using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Application.Services;
using Restaurant.Application.Services.Interfaces;
using System.Reflection;

namespace Restaurant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Modular Domain Services with Clean Architecture Interfaces
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IKitchenService, KitchenService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<ICrmService, CrmService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPurchasingService, PurchasingService>();
        services.AddScoped<IBookingService, BookingService>();

        // System Seeder
        services.AddScoped<SeedDataService>();

        return services;
    }
}
