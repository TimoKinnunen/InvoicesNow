using System;

namespace InvoicesNow.Models
{
    /// <summary>
    /// Represents an item.
    /// </summary>
    public class Item
    {
        public Item()
        {
            ItemId = Guid.NewGuid();
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Item(string name)
        {
            ItemId = Guid.NewGuid();
            Name = name;
            CreatedAtDateTime = UpdatedAtDateTime = DateTime.Now;
        }

        public Item(DTOItem importedDTOItem)
        {
            ItemId = importedDTOItem.ItemId;
            Name = importedDTOItem.Name;
            CreatedAtDateTime = importedDTOItem.CreatedAtDateTime;
            UpdatedAtDateTime = importedDTOItem.UpdatedAtDateTime;
            Tax = importedDTOItem.Tax;
            Price = importedDTOItem.Price;
        }

        public Guid ItemId { get; set; }
        public string Name { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}

