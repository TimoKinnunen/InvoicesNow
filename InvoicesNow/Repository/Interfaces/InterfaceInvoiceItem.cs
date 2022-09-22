using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the invoice items backend.
    /// </summary>
    public interface InterfaceInvoiceItem
    {
        /// <summary>
        /// Returns all invoice items. 
        /// </summary>
        Task<IEnumerable<InvoiceItem>> GetAllInvoiceItemsAsync();

        /// <summary>
        /// Returns the invoice item with the given id.
        /// </summary>
        Task<InvoiceItem> GetInvoiceItemAsync(Guid invoiceItemId);
    }
}