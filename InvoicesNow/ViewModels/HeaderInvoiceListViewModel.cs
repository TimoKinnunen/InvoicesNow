namespace InvoicesNow.ViewModels
{
    public class HeaderInvoiceListViewModel
    {
        public string HeaderInvoiceNumber { get; set; } = "Invoice number ↑";
        public string HeaderInvoiceDate { get; set; } = "Invoice date";
        public string HeaderSellerName { get; set; } = "Seller's name";
        public string HeaderBuyerName { get; set; } = "Buyer's name";
        public string HeaderTotalIncludingTax { get; set; } = "Total including tax";
    }
}