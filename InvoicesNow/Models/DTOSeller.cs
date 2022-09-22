using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents a seller.
    /// Json friendly.
    /// </summary>
    public class DTOSeller
    {
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
