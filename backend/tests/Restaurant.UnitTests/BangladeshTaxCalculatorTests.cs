using Restaurant.Application.Common.Pricing;
using Xunit;

namespace Restaurant.UnitTests;

public class BangladeshTaxCalculatorTests
{
    [Fact]
    public void Calculate_VatExclusive_ComputesCorrectVatAndGrandTotal()
    {
        // Arrange: Subtotal 1000 BDT, 100 BDT discount, 10% service charge, 5% VAT (exclusive)
        decimal subtotal = 1000m;
        decimal discount = 100m;
        decimal serviceChargeRate = 10m; // 10% of 900 = 90
        decimal vatRate = 5m;           // 5% of (900 + 90) = 49.50
        bool isVatInclusive = false;

        // Act
        var result = BangladeshTaxCalculator.Calculate(subtotal, discount, serviceChargeRate, vatRate, isVatInclusive);

        // Assert
        Assert.Equal(1000m, result.Subtotal);
        Assert.Equal(100m, result.DiscountAmount);
        Assert.Equal(900m, result.NetAfterDiscount);
        Assert.Equal(90m, result.ServiceChargeAmount);
        Assert.Equal(990m, result.TaxableBase);
        Assert.Equal(49.50m, result.VatAmount);
        Assert.Equal(1040m, result.GrandTotal); // 990 + 49.50 = 1039.50 rounded to 1040
        Assert.Equal(0.50m, result.RoundingAmount);
    }

    [Fact]
    public void Calculate_VatInclusive_ExtractsVatFromTaxableBase()
    {
        // Arrange: Subtotal 1050 BDT with 5% VAT inclusive, 0 discount, 0 service charge
        decimal subtotal = 1050m;
        decimal discount = 0m;
        decimal serviceChargeRate = 0m;
        decimal vatRate = 5m; // 1050 - (1050 / 1.05) = 1050 - 1000 = 50 BDT VAT
        bool isVatInclusive = true;

        // Act
        var result = BangladeshTaxCalculator.Calculate(subtotal, discount, serviceChargeRate, vatRate, isVatInclusive);

        // Assert
        Assert.Equal(1050m, result.Subtotal);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(1050m, result.TaxableBase);
        Assert.Equal(50m, result.VatAmount);
        Assert.Equal(1050m, result.GrandTotal);
        Assert.Equal(0m, result.RoundingAmount);
    }

    [Fact]
    public void Calculate_ZeroSubtotal_ReturnsZeroGrandTotal()
    {
        // Act
        var result = BangladeshTaxCalculator.Calculate(0m, 50m, 10m, 5m, false);

        // Assert
        Assert.Equal(0m, result.GrandTotal);
        Assert.Equal(0m, result.VatAmount);
    }
}
