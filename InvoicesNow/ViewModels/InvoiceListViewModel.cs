using InvoicesNow.Models;
using System;
using System.Collections.Generic;

namespace InvoicesNow.ViewModels
{
    public class InvoiceListViewModel
    {
        public InvoiceListViewModel(Invoice invoice)
        {
            InvoiceListViewModelId = invoice.InvoiceId;
            InvoiceNumber = invoice.InvoiceNumber;
            InvoiceDate = invoice.InvoiceDate;

            SellerName = invoice.SellerName;
            BuyerName = invoice.BuyerName;
            BuyerEmail = invoice.BuyerEmail;

            TotalIncludingTax = invoice.TotalIncludingTax;
            TotalExcludingTax = invoice.TotalExcludingTax;
            TotalTax = invoice.TotalTax;

            InvoiceItemViewModels = new List<InvoiceItemViewModel>();
        }

        public Guid InvoiceListViewModelId { get; set; }

        public int InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string SellerName { get; set; }

        public string BuyerName { get; set; }

        public string BuyerEmail { get; set; }

        public decimal TotalIncludingTax { get; set; }

        public decimal TotalExcludingTax { get; set; }

        public decimal TotalTax { get; set; }

        public ICollection<InvoiceItemViewModel> InvoiceItemViewModels { get; set; }
    }
}
