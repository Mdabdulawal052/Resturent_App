namespace Restaurant.Application.Common.Pricing;

public record OrderCalculationResult(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal NetAfterDiscount,
    decimal ServiceChargeAmount,
    decimal TaxableBase,
    decimal VatAmount,
    decimal RoundingAmount,
    decimal GrandTotal
);

public static class BangladeshTaxCalculator
{
    public static OrderCalculationResult Calculate(
        decimal subtotal,
        decimal discountAmount,
        decimal serviceChargeRatePercent,
        decimal vatRatePercent,
        bool isVatInclusive)
    {
        if (subtotal < 0) subtotal = 0;
        if (discountAmount < 0) discountAmount = 0;

        decimal netAfterDiscount = Math.Max(0, subtotal - discountAmount);
        decimal serviceChargeAmount = serviceChargeRatePercent > 0 
            ? Math.Round(netAfterDiscount * (serviceChargeRatePercent / 100m), 2, MidpointRounding.AwayFromZero)
            : 0m;

        decimal taxableBase = netAfterDiscount + serviceChargeAmount;
        decimal vatAmount;
        decimal rawGrandTotal;

        if (isVatInclusive)
        {
            // The item price already includes VAT. We extract the VAT component from the base.
            vatAmount = vatRatePercent > 0
                ? Math.Round(taxableBase - (taxableBase / (1m + (vatRatePercent / 100m))), 2, MidpointRounding.AwayFromZero)
                : 0m;
            rawGrandTotal = taxableBase;
        }
        else
        {
            // VAT is added on top of the taxable base.
            vatAmount = vatRatePercent > 0
                ? Math.Round(taxableBase * (vatRatePercent / 100m), 2, MidpointRounding.AwayFromZero)
                : 0m;
            rawGrandTotal = taxableBase + vatAmount;
        }

        decimal grandTotal = Math.Round(rawGrandTotal, 0, MidpointRounding.AwayFromZero);
        decimal roundingAmount = grandTotal - rawGrandTotal;

        return new OrderCalculationResult(
            Subtotal: subtotal,
            DiscountAmount: discountAmount,
            NetAfterDiscount: netAfterDiscount,
            ServiceChargeAmount: serviceChargeAmount,
            TaxableBase: taxableBase,
            VatAmount: vatAmount,
            RoundingAmount: roundingAmount,
            GrandTotal: grandTotal
        );
    }
}
