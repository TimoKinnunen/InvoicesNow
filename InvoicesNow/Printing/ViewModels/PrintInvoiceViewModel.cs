using System;

namespace InvoicesNow.Printing.ViewModels
{
    //needed on InvoicePage.xaml
    public class PrintInvoiceViewModel
    {
        public Guid PrintInvoiceViewModelId { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        
        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public string InvoiceInfoToBuyer { get; set; }

        public decimal TotalIncludingTax { get; set; }
        public decimal TotalExcludingTax { get; set; }
        public decimal TotalTax { get; set; }

        public int NetPaymentTermDays { get; set; }

        public DateTime NetPaymentDueDate { get; set; }

        public string SellerName { get; set; }
        public string SellerEmail { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPhonenumber { get; set; }
        public string SellerAccount { get; set; }
        public string SellerSWIFTBIC { get; set; }
        public string SellerIBAN { get; set; }
        public Guid SellerId { get; set; }

        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhonenumber { get; set; }
        public Guid BuyerId { get; set; }

        public string TranslateInvoice { get; set; }
        public string TranslateInvoiceNumber { get; set; }
        public string TranslateInvoiceDate { get; set; }
        public string TranslateDueDate { get; set; }
        public string TranslateAmountToPay { get; set; }
        public string TranslatePaymentToAccount { get; set; }
        public string TranslateSWIFTBIC { get; set; }
        public string TranslateIBAN { get; set; }
        public string TranslateTotalIncludingTax { get; set; }
        public string TranslateTotalExcludingTax { get; set; }
        public string TranslateTotalTax { get; set; }
        public string TranslateBuyerName { get; set; }
        public string TranslateBuyerEmailAddress { get; set; }
        public string TranslateBuyerAddress { get; set; }
        public string TranslateBuyerPhonenumber { get; set; }
        public string TranslateSellerName { get; set; }
        public string TranslateSellerEmailAddress { get; set; }
        public string TranslateSellerAddress { get; set; }
        public string TranslateSellerPhonenumber { get; set; }
        public string TranslatePageNumber { get; set; }
    }
}