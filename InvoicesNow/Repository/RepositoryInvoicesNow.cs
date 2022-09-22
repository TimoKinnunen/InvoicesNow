using InvoicesNow.Repository.Interfaces;

namespace InvoicesNow.Repository
{
    /// <summary>
    /// Defines methods for interacting with the app backend.
    /// </summary>
    public interface RepositoryInvoicesNow
    {
        /// <summary>
        /// Returns the sellers repository.
        /// </summary>
        InterfaceSeller Sellers { get; }

        /// <summary>
        /// Returns the buyers repository.
        /// </summary>
        InterfaceBuyer Buyers { get; }

        /// <summary>
        /// Returns the invoices repository.
        /// </summary>
        InterfaceInvoice Invoices { get; }

        /// <summary>
        /// Returns the items repository.
        /// </summary>
        InterfaceItem Items { get; }

        /// <summary>
        /// Returns the invoice items repository.
        /// </summary>
        InterfaceInvoiceItem InvoiceItems { get; }
    }
}
