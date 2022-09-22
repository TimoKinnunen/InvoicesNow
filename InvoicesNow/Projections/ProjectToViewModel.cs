using InvoicesNow.Models;
using InvoicesNow.ViewModels;
using System.Linq;

namespace InvoicesNow.Projections
{
    public static class ProjectToViewModel
    {
        public static InvoiceViewModel NewInvoiceViewModel(Invoice invoice)
        {
            return new InvoiceViewModel(invoice)
            {
            };
        }

        public static ItemViewModel NewItemViewModel(Item item)
        {
            return new ItemViewModel(item)
            {
            };
        }

        public static InvoiceItemViewModel NewInvoiceItemViewModel(InvoiceItem invoiceItem)
        {
            return new InvoiceItemViewModel(invoiceItem)
            {
            };
        }

        public static InvoiceListViewModel NewInvoiceListViewModel(Invoice invoice)
        {
            InvoiceListViewModel invoiceListViewModel = new InvoiceListViewModel(invoice)
            {
            };

            foreach (var invoiceItem in invoice.InvoiceItems.OrderBy(o => o.Name))
            {
                InvoiceItemViewModel invoiceItemViewModel = NewInvoiceItemViewModel(invoiceItem);
                invoiceListViewModel.InvoiceItemViewModels.Add(invoiceItemViewModel);
            }

            return invoiceListViewModel;
        }

        public static SellerViewModel NewSellerViewModel(Seller seller)
        {
            return new SellerViewModel(seller)
            {
            };
        }

        public static BuyerViewModel NewBuyerViewModel(Buyer buyer)
        {
            return new BuyerViewModel(buyer)
            {
            };
        }
    }
}

