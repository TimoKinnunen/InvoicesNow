using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents a seller.
    /// </summary>
    public class Seller
    {
        public Seller()
        {
            SellerId = Guid.NewGuid();
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Seller(string sellerName)
        {
            SellerId = Guid.NewGuid();
            SellerName = sellerName;
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Seller(DTOSeller importedDTOSeller)
        {
            SellerId = importedDTOSeller.SellerId;
            SellerName = importedDTOSeller.SellerName;

            CreatedAtDateTime = importedDTOSeller.CreatedAtDateTime;
            UpdatedAtDateTime = importedDTOSeller.UpdatedAtDateTime;

            SellerEmail = importedDTOSeller.SellerEmail;
            SellerAddress = importedDTOSeller.SellerAddress;
            SellerPhonenumber = importedDTOSeller.SellerPhonenumber;
            SellerAccount = importedDTOSeller.SellerAccount;
            SellerSWIFTBIC = importedDTOSeller.SellerSWIFTBIC;
            SellerIBAN = importedDTOSeller.SellerIBAN;
        }

        public Guid SellerId { get; set; }
        public string SellerName { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public string SellerEmail { get; set; }
        public string SellerAddress { get; set; }
        public string SellerPhonenumber { get; set; }
        public string SellerAccount { get; set; }
        public string SellerSWIFTBIC { get; set; }
        public string SellerIBAN { get; set; }
    }
}

