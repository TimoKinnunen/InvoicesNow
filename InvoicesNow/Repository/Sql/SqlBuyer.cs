using InvoicesNow.Models;
using InvoicesNow.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the buyers backend using 
    /// SQL via Entity Framework Core 3.1.
    /// </summary>
    public class SqlBuyer : InterfaceBuyer
    {
        InvoicesNowDbContext db { get; }

        public SqlBuyer(InvoicesNowDbContext invoicesNowContext)
        {
            db = invoicesNowContext;
        }

        public async Task<IEnumerable<Buyer>> GetAllBuyersAsync()
        {
            return await db.Buyers
                .ToListAsync();
        }

        public async Task<Buyer> GetBuyerAsync(Guid buyerId)
        {
            return await db.Buyers
                .FirstOrDefaultAsync(buyer => buyer.BuyerId == buyerId);
        }

        public async Task<Buyer> UpsertAsync(Buyer buyer)
        {
            if (buyer.BuyerId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(buyer));
            }

            Buyer existingBuyer = await db.Buyers.FindAsync(buyer.BuyerId);
            if (existingBuyer == null)
            {
                db.Buyers.Add(buyer);
            }
            else
            {
                db.Entry(existingBuyer).CurrentValues.SetValues(buyer);
            }
            await db.SaveChangesAsync();
            return buyer;
        }

        public async Task DeleteAsync(Guid buyerId)
        {
            Buyer existingBuyer = await db.Buyers.FindAsync(buyerId);
            if (existingBuyer != null)
            {
                db.Buyers.Remove(existingBuyer);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Buyer> FindExistingBuyer(Buyer newBuyer)
        {
            return await db.Buyers.FirstOrDefaultAsync(buyer =>
                buyer.BuyerName.Equals(newBuyer.BuyerName)
                && buyer.BuyerEmail.Equals(newBuyer.BuyerEmail)
                && buyer.BuyerAddress.Equals(newBuyer.BuyerAddress)
                && buyer.BuyerPhonenumber.Equals(newBuyer.BuyerPhonenumber));
        }
    }
}

