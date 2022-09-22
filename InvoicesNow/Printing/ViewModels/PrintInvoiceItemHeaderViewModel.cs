namespace InvoicesNow.Printing.ViewModels
{
    //needed on InvoicePage.xaml
    public class PrintInvoiceItemHeaderViewModel
    {
        public string TranslateHeaderName { get; set; } = "Name";
        public string TranslateHeaderQuantity { get; set; } = "Quantity";
        public string TranslateHeaderTax { get; set; } = "Tax";
        public string TranslateHeaderPrice { get; set; } = "Price";
    }
}

