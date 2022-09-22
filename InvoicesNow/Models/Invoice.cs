using System;
using System.Collections.Generic;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents an invoice.
    /// </summary>
    public class Invoice
    {
        public Invoice()
        {
            InvoiceInfoToBuyer = "Please pay latest at due date";
            NetPaymentTermDays = 30; //net payment within 1, 5, 15, 30 days
            InvoiceItems = new List<InvoiceItem>();
        }

        public Invoice(DateTime invoiceDate, int invoiceNumber)
        {
            InvoiceId = Guid.NewGuid();
            InvoiceDate = invoiceDate;
            InvoiceNumber = invoiceNumber;
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;

            InvoiceInfoToBuyer = "Please pay latest at due date";
            NetPaymentTermDays = 30; //net payment within 1, 5, 15, 30 days

            InvoiceItems = new List<InvoiceItem>();
        }

        public Invoice(DTOInvoice importedDTOInvoice)
        {
            InvoiceId = importedDTOInvoice.InvoiceId;
            InvoiceNumber = importedDTOInvoice.InvoiceNumber;

            CreatedAtDateTime = importedDTOInvoice.CreatedAtDateTime;
            UpdatedAtDateTime = importedDTOInvoice.UpdatedAtDateTime;

            InvoiceDate = importedDTOInvoice.InvoiceDate;
            SellerName = importedDTOInvoice.SellerName;
            BuyerName = importedDTOInvoice.BuyerName;

            InvoiceInfoToBuyer = importedDTOInvoice.InvoiceInfoToBuyer;

            TotalIncludingTax = importedDTOInvoice.TotalIncludingTax;
            TotalExcludingTax = importedDTOInvoice.TotalExcludingTax;
            TotalTax = importedDTOInvoice.TotalTax;

            NetPaymentTermDays = importedDTOInvoice.NetPaymentTermDays;

            NetPaymentDueDate = importedDTOInvoice.NetPaymentDueDate;

            SellerEmail = importedDTOInvoice.SellerEmail;
            SellerAddress = importedDTOInvoice.SellerAddress;
            SellerPhonenumber = importedDTOInvoice.SellerPhonenumber;
            SellerAccount = importedDTOInvoice.SellerAccount;
            SellerSWIFTBIC = importedDTOInvoice.SellerSWIFTBIC;
            SellerIBAN = importedDTOInvoice.SellerIBAN;
            SellerId = importedDTOInvoice.SellerId;

            BuyerEmail = importedDTOInvoice.BuyerEmail;
            BuyerAddress = importedDTOInvoice.BuyerAddress;
            BuyerPhonenumber = importedDTOInvoice.BuyerPhonenumber;
            BuyerId = importedDTOInvoice.BuyerId;

            InvoiceItems = new List<InvoiceItem>();

            foreach (var DTOInvoiceItem in importedDTOInvoice.DTOInvoiceItems)
            {
                InvoiceItems.Add(new InvoiceItem()
                {
                    InvoiceItemId = DTOInvoiceItem.InvoiceItemId,
                    Name = DTOInvoiceItem.Name,
                    InvoiceId = DTOInvoiceItem.InvoiceId,
                    CreatedAtDateTime = DTOInvoiceItem.CreatedAtDateTime,
                    UpdatedAtDateTime = DTOInvoiceItem.UpdatedAtDateTime,
                    Quantity = DTOInvoiceItem.Quantity,
                    Tax = DTOInvoiceItem.Tax,
                    Price = DTOInvoiceItem.Price,
                });
            }
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

        public ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}

