using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents a buyer.
    /// </summary>
    public class Buyer
    {
        public Buyer()
        {
            BuyerId = Guid.NewGuid();
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Buyer(string buyerName)
        {
            BuyerId = Guid.NewGuid();
            BuyerName = buyerName;
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Buyer(DTOBuyer importedDTOBuyer)
        {
            BuyerId = importedDTOBuyer.BuyerId;
            BuyerName = importedDTOBuyer.BuyerName;

            CreatedAtDateTime = importedDTOBuyer.CreatedAtDateTime;
            UpdatedAtDateTime = importedDTOBuyer.UpdatedAtDateTime;

            BuyerEmail = importedDTOBuyer.BuyerEmail;
            BuyerAddress = importedDTOBuyer.BuyerAddress;
            BuyerPhonenumber = importedDTOBuyer.BuyerPhonenumber;
            BuyerId = importedDTOBuyer.BuyerId;
        }

        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public string BuyerEmail { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerPhonenumber { get; set; }
    }
}
