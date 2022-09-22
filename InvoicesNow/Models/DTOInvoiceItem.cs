using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents an invoice item.
    /// Json friendly.
    /// </summary>
    public class DTOInvoiceItem
    {
        public Guid InvoiceItemId { get; set; }
        public string Name { get; set; }

        public Guid InvoiceId { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public decimal Quantity { get; set; }
        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}
