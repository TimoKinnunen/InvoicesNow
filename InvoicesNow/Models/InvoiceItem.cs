using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents an invoice item.
    /// </summary>
    public class InvoiceItem
    {
        public InvoiceItem() { }

        public InvoiceItem(string name, Guid invoiceId)
        {
            InvoiceItemId = Guid.NewGuid();
            Name = name;

            InvoiceId = invoiceId; // belongs to this parent

            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Guid InvoiceItemId { get; set; }
        public string Name { get; set; }

        public Guid InvoiceId { get; set; } // FKInvoiceId

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public decimal Quantity { get; set; } = 1;
        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}