using InvoicesNow.Models;
using InvoicesNow.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the invoices backend using 
    /// SQL via Entity Framework Core 3.1.
    /// </summary>
    public class SqlInvoice : InterfaceInvoice
    {
        InvoicesNowDbContext db { get; }

        public SqlInvoice(InvoicesNowDbContext oneToManyContext) => db = oneToManyContext;

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await db.Invoices
                .Include(o => o.InvoiceItems) //get with children
                .ToListAsync();
        }

        public async Task<Invoice> GetInvoiceAsync(Guid invoiceId)
        {
            return await db.Invoices
                .Include(o => o.InvoiceItems) //get with children
                .FirstOrDefaultAsync(invoice => invoice.InvoiceId == invoiceId);
        }

        public async Task<Invoice> InsertAsync(Invoice invoice)
        {
            invoice.UpdatedAtDateTime = DateTime.Now;

            db.Invoices.Add(invoice); // add invoice and invoice items

            await db.SaveChangesAsync();

            return await db.Invoices
                .Include(o => o.InvoiceItems) //get with children
                .FirstOrDefaultAsync(o => o.InvoiceId == invoice.InvoiceId);
        }

        public async Task<Invoice> SetNewInvoiceNumberAsync(Guid invoiceId, int newInvoiceNumber)
        {
            Invoice existingInvoice = await db.Invoices
              .FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);
            if (existingInvoice != null)
            {
                existingInvoice.InvoiceNumber = newInvoiceNumber;

                await db.SaveChangesAsync();
            }
            return await db.Invoices
                .Include(o => o.InvoiceItems) //get with children
                .FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);
        }

        //public async Task<Invoice> InsertInvoiceItemAsync(InvoiceItem invoiceItem)
        //{
        //    Invoice existingInvoice = await db.Invoices
        //      .Include(o => o.InvoiceItems) //get with children
        //      .FirstOrDefaultAsync(o => o.InvoiceId == invoiceItem.InvoiceId);
        //    if (existingInvoice != null)
        //    {
        //        db.InvoiceItems.Add(invoiceItem);

        //        existingInvoice.InvoiceItems.Add(invoiceItem);

        //        await db.SaveChangesAsync();
        //    }
        //    return await db.Invoices
        //        .Include(o => o.InvoiceItems) //get with children
        //        .FirstOrDefaultAsync(o => o.InvoiceId == invoiceItem.InvoiceId);
        //}

        //public async Task<Invoice> RemoveInvoiceItemAsync(Guid invoiceId, Guid invoiceItemId)
        //{
        //    Invoice existingInvoice = await db.Invoices
        //      .Include(o => o.InvoiceItems) //get with children
        //      .FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);
        //    if (existingInvoice != null)
        //    {
        //        InvoiceItem existingInvoiceItem = await db.InvoiceItems
        //          .FirstOrDefaultAsync(o => o.InvoiceItemId == invoiceItemId);

        //        if (existingInvoiceItem != null)
        //        {
        //            existingInvoice.InvoiceItems.Remove(existingInvoiceItem);

        //            db.InvoiceItems.Remove(existingInvoiceItem);

        //            await db.SaveChangesAsync();
        //        }
        //    }
        //    return await db.Invoices
        //        .Include(o => o.InvoiceItems) //get with children
        //        .FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);
        //}

        public async Task DeleteAsync(Guid invoiceId)
        {
            Invoice existingInvoice = await db.Invoices
              .Include(o => o.InvoiceItems) //get with children
              .FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);
            if (existingInvoice != null)
            {
                foreach (InvoiceItem invoiceItem in existingInvoice.InvoiceItems)
                {
                    db.InvoiceItems.Remove(invoiceItem);
                }

                db.Invoices.Remove(existingInvoice);

                existingInvoice.UpdatedAtDateTime = DateTime.Now;

                await db.SaveChangesAsync();
            }
        }
    }
}