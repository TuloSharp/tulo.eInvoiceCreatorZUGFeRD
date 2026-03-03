using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices
{
    public class CalculateTotalsAtInvoicePositionCommand(InvoicePositionDetailsFormViewModel invoicePositionDetailsFormViewModel) : BaseCommand
    {
        private readonly InvoicePositionDetailsFormViewModel _invoicePositionDetailsFormViewModel = invoicePositionDetailsFormViewModel;

        public override void Execute(object parameter)
        {
            // =========================================================================
            // PURPOSE / PRICING MODEL (IMPORTANT)
            // =========================================================================
            // This command calculates invoice position totals using a NET pricing model:
            //
            // InvoicePositionUnitPrice is a NET unit price (excluding VAT).
            // 1.- Net line amount  = Quantity * NetUnitPrice
            // 2.- VAT amount       = Net line amount * VAT rate
            // 3.- Gross line amount= Net line amount + VAT amount
            // 4.- Discount (optional) is a NET discount amount (excluding VAT).
            //
            // This matches typical invoice structures (e.g., ZUGFeRD / Factur-X),
            // where line totals are net and the invoice header shows net + VAT = gross.
            //
            // Rounding:
            // - We round monetary values to 2 decimals using normal commercial rounding.
            //   (Consider MidpointRounding.AwayFromZero if your accounting rules require it.)
            //
            // Discount:
            // - InvoicePositionDiscountNetAmount is a NET discount amount (excluding VAT).
            // - This command only outputs "Net after discount" (optional field).
            //   It does NOT recalculate a discounted gross value unless you add it explicitly.
            // =========================================================================

            // Reset outputs
            _invoicePositionDetailsFormViewModel.InvoicePositionNetAmount = 0m;
            _invoicePositionDetailsFormViewModel.InvoicePositionGrossAmount = 0m;
            _invoicePositionDetailsFormViewModel.InvoicePositionNetAmountAfterDiscount = null; // <- leave blank if no discount

            var quantity = _invoicePositionDetailsFormViewModel.InvoicePositionQuantity;
            var unitPrice = _invoicePositionDetailsFormViewModel.InvoicePositionUnitPrice; // NET unit price (excl. VAT)
            var vatRate = _invoicePositionDetailsFormViewModel.InvoicePositionVatRate;  // e.g. 19 for 19%
            var discountNet = _invoicePositionDetailsFormViewModel.InvoicePositionDiscountNetAmount;

            // Basic validation
            if (quantity <= 0 || unitPrice <= 0 || vatRate < 0)
                return;

            // 1) Net
            var net = quantity * unitPrice;
            net = Math.Round(net, 2);

            // 2) VAT + Gross
            var vatAmount = net * (vatRate / 100m);
            vatAmount = Math.Round(vatAmount, 2);

            // 3) Gross line amount (net + VAT)
            var gross = net + vatAmount;
            gross = Math.Round(gross, 2);

            _invoicePositionDetailsFormViewModel.InvoicePositionNetAmount = net;
            _invoicePositionDetailsFormViewModel.InvoicePositionGrossAmount = gross;

            // 4) Discount optional -> Net after discount ONLY if discount > 0
            if (discountNet > 0m)
            {
                var netAfterDiscount = net - discountNet;
                if (netAfterDiscount < 0m) netAfterDiscount = 0m;

                _invoicePositionDetailsFormViewModel.InvoicePositionNetAmountAfterDiscount = Math.Round(netAfterDiscount, 2);
            }
            // else remains null => field empty
        }
    }
}
