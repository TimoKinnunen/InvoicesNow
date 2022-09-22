using System;
using System.Collections.Generic;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents an invoice.
    /// Json friendly.
    /// </summary>
    public class DTOInvoice
    {
        public DTOInvoice()
        {
            DTOInvoiceItems = new List<DTOInvoiceItem>();
        }

        public Guid InvoiceId { get; set; }
        public int InvoiceNumber { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public DateTime InvoiceDate { get; set; }
        public string SellerName { get; set; }
        public string BuyerName { get; set; }

        public string InvoiceInfoToBuyer { get; set; }

        public decimal TotalIncludingTax { get; set; }
        public decimal TotalExcludingTax { get; set; }
        public decimal TotalTax { get; set; }

        public int NetPaymentTermDays { get; set; }

        public DateTime NetPaymentDueDate { get; set; }

        public string SellerEmail { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPhonenumber { get; set; }
        public string SellerAccount { get; set; }
        public string SellerSWIFTBIC { get; set; }
        public string SellerIBAN { get; set; }
        public Guid SellerId { get; set; }

        public string BuyerEmail { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhonenumber { get; set; }
        public Guid BuyerId { get; set; }

        public ICollection<DTOInvoiceItem> DTOInvoiceItems { get; set; }
    }
}
