using System;

namespace InvoicesNow.Printing.ViewModels
{
    //needed on InvoicePage.xaml
    public class PrintInvoiceItemViewModel
    {
        public Guid PrintInvoiceItemViewModelId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}