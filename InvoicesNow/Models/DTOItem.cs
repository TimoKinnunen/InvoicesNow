using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents an item.
    /// Json friendly.
    /// </summary>
    public class DTOItem
    {
        public Guid ItemId { get; set; }
        public string Name { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}
