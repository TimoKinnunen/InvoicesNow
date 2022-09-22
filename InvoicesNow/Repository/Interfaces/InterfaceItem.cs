using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the items backend.
    /// </summary>
    public interface InterfaceItem
    {
        /// <summary>
        /// Returns all items. 
        /// </summary>
        Task<IEnumerable<Item>> GetAllItemsAsync();

        /// <summary>
        /// Returns the item with the given Id. 
        /// </summary>
        Task<Item> GetItemAsync(Guid itemId);

        /// <summary>
        /// Adds a new item if the item does not exist, updates the 
        /// existing item otherwise.
        /// </summary>
        Task<Item> UpsertAsync(Item item);

        /// <summary>
        /// Deletes a item.
        /// </summary>
        Task DeleteAsync(Guid itemId);
    }
}
