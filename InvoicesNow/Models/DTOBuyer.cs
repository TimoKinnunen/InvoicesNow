using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents a buyer.
    /// Json friendly.
    /// </summary>
    public class DTOBuyer
    {
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public string BuyerEmail { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhonenumber { get; set; }
    }
}
