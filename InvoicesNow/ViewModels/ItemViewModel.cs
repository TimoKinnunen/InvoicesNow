using InvoicesNow.Models;
using System;

namespace InvoicesNow.ViewModels
{
    public class ItemViewModel
    {
        public ItemViewModel(Item item)
        {
            ItemViewModelId = item.ItemId;
            Name = item.Name;

            CreatedAtDateTime = item.CreatedAtDateTime;
            UpdatedAtDateTime = item.UpdatedAtDateTime;

            BigTax = item.Tax * 100;
            Tax = item.Tax;
            Price = item.Price;
        }

        public Guid ItemViewModelId { get; set; }
        public string Name { get; set; }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public decimal BigTax { get; set; }
        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}