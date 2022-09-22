using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the sellers backend.
    /// </summary>
    public interface InterfaceSeller
    {
        /// <summary>
        /// Returns all sellers. 
        /// </summary>
        Task<IEnumerable<Seller>> GetAllSellersAsync();

        /// <summary>
        /// Returns the seller with the given id. 
        /// </summary>
        Task<Seller> GetSellerAsync(Guid sellerId);

        /// <summary>
        /// Adds a new seller if the seller does not exist, updates the 
        /// existing seller otherwise.
        /// </summary>
        Task<Seller> UpsertAsync(Seller seller);

        /// <summary>
        /// Deletes a seller.
        /// </summary>
        Task DeleteAsync(Guid sellerId);

        /// <summary>
        /// Returns the seller with matching field values. 
        /// </summary>
        Task<Seller> FindExistingSeller(Seller newSeller);

        /// <summary>
        /// Returns the count of sellerss.
        /// </summary>
        //Task<int> GetRecordCountAsync();
    }
}