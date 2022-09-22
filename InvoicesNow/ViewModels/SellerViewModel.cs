using InvoicesNow.Models;
using System;

namespace InvoicesNow.ViewModels
{
    public class SellerViewModel
    {
        public SellerViewModel(Seller seller)
        {
            SellerViewModelId = seller.SellerId;
            SellerName = seller.SellerName;

            CreatedAtDateTime = seller.CreatedAtDateTime;
            UpdatedAtDateTime = seller.UpdatedAtDateTime;

            SellerEmail = seller.SellerEmail;
            SellerAddress = seller.SellerAddress;
            SellerPhonenumber = seller.SellerPhonenumber;
            SellerAccount = seller.SellerAccount;
            SellerSWIFTBIC = seller.SellerSWIFTBIC;
            SellerIBAN = seller.SellerIBAN;
            SellerId = seller.SellerId;
        }

        public Guid SellerViewModelId { get; set; }
        public string SellerName { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public string SellerEmail { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPhonenumber { get; set; }
        public string SellerAccount { get; set; }
        public string SellerSWIFTBIC { get; set; }
        public string SellerIBAN { get; set; }
        public Guid SellerId { get; set; }
    }
}
