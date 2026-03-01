using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices
{
    class CalculateTotalsAtInvoicePositionCommand(InvoicePositionDetailsFormViewModel vm) : BaseCommand
    {
        private readonly InvoicePositionDetailsFormViewModel _vm = vm;

        public override void Execute(object parameter)
        {
            // Reset outputs
            _vm.InvoicePositionNetAmount = 0m;
            _vm.InvoicePositionGrossAmount = 0m;
            _vm.InvoicePositionNetAmountAfterDiscount = null; // <- leave blank if no discount

            var quantity = _vm.InvoicePositionQuantity;
            var unitPrice = _vm.InvoicePositionUnitPrice;
            var vatRate = _vm.InvoicePositionVatRate;
            var discountNet = _vm.InvoicePositionDiscountNetAmount;

            // Basic validation
            if (quantity <= 0 || unitPrice <= 0 || vatRate < 0)
                return;

            // 1) Net
            var net = (quantity * unitPrice);
            net = Math.Round(net, 2);

            // 2) VAT + Gross
            var vatAmount = net * (vatRate / 100m);
            vatAmount = Math.Round(vatAmount, 2);

            var gross = net + vatAmount;
            gross = Math.Round(gross, 2);

            _vm.InvoicePositionNetAmount = net;
            _vm.InvoicePositionGrossAmount = gross;

            // 3) Discount optional -> Net after discount ONLY if discount > 0
            if (discountNet > 0m)
            {
                var netAfterDiscount = net - discountNet;
                if (netAfterDiscount < 0m) netAfterDiscount = 0m;

                _vm.InvoicePositionNetAmountAfterDiscount = Math.Round(netAfterDiscount, 2);
            }
            // else remains null => field empty
        }
    }
}
