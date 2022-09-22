using InvoicesNow.Models;
using System;

namespace InvoicesNow.ViewModels
{
    public class BuyerViewModel
    {
        public BuyerViewModel(Buyer buyer)
        {
            BuyerViewModelId = buyer.BuyerId;
            BuyerName = buyer.BuyerName;

            CreatedAtDateTime = buyer.CreatedAtDateTime;
            UpdatedAtDateTime = buyer.UpdatedAtDateTime;

            BuyerEmail = buyer.BuyerEmail;
            BuyerAddress = buyer.BuyerAddress;
            BuyerPhonenumber = buyer.BuyerPhonenumber;
        }

        public Guid BuyerViewModelId { get; set; }
        public string BuyerName { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public string BuyerEmail { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhonenumber { get; set; }
        public Guid BuyerId { get; set; }
    }
}