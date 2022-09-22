using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Interfaces
{
    /// <summary>
    /// Defines methods for interacting with the invoices backend.
    /// </summary>
    public interface InterfaceInvoice
    {
        /// <summary>
        /// Returns all invoices with children. 
        /// </summary>
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();

        /// <summary>
        /// Returns the invoice with children with the given id.
        /// </summary>
        Task<Invoice> GetInvoiceAsync(Guid invoiceId);

        /// <summary>
        /// Adds a new invoice with children.
        /// </summary>
        Task<Invoice> InsertAsync(Invoice invoice);

        /// <summary>
        /// Sets new invoice number.
        /// </summary>
        Task<Invoice> SetNewInvoiceNumberAsync(Guid invoiceId, int newInvoiceNumber);

        /// <summary>
        /// Adds a new invoice item as invoice child.
        /// </summary>
        //Task<Invoice> InsertInvoiceItemAsync(InvoiceItem invoiceItem);

        /// <summary>
        /// Removes an invoice item as invoice child from invoice.
        /// </summary>
        //Task<Invoice> RemoveInvoiceItemAsync(Guid invoiceId, Guid invoiceItemId);

        /// <summary>
        /// Deletes an invoice.
        /// </summary>
        Task DeleteAsync(Guid invoiceId);
    }
}