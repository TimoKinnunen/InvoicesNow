using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the buyers backend.
    /// </summary>
    public interface InterfaceBuyer
    {
        /// <summary>
        /// Returns all buyers. 
        /// </summary>
        Task<IEnumerable<Buyer>> GetAllBuyersAsync();

        /// <summary>
        /// Returns the buyer with the given id. 
        /// </summary>
        Task<Buyer> GetBuyerAsync(Guid buyerId);

        /// <summary>
        /// Adds a new buyer if the buyer does not exist, updates the 
        /// existing buyer otherwise.
        /// </summary>
        Task<Buyer> UpsertAsync(Buyer buyer);

        /// <summary>
        /// Deletes a buyer.
        /// </summary>
        Task DeleteAsync(Guid buyerId);

        /// <summary>
        /// Returns the buyer with matching field values. 
        /// </summary>
        Task<Buyer> FindExistingBuyer(Buyer newBuyer);

        /// <summary>
        /// Returns the count of buyers.
        /// </summary>
        //Task<int> GetRecordCountAsync();
    }
}