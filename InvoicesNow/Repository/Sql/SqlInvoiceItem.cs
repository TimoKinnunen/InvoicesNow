using InvoicesNow.Models;
using InvoicesNow.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the invoice items backend using 
    /// SQL via Entity Framework Core 3.1.
    /// </summary>
    public class SqlInvoiceItem : InterfaceInvoiceItem
    {
        InvoicesNowDbContext db { get; }

        public SqlInvoiceItem(InvoicesNowDbContext oneToManyContext) => db = oneToManyContext;

        public async Task<IEnumerable<InvoiceItem>> GetAllInvoiceItemsAsync()
        {
            return await db.InvoiceItems
                .ToListAsync();
        }

        public async Task<InvoiceItem> GetInvoiceItemAsync(Guid invoiceItemId)
        {
            return await db.InvoiceItems
                .FirstOrDefaultAsync(item => item.InvoiceItemId == invoiceItemId);
        }
    }
}