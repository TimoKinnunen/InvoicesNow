using InvoicesNow.Models;
using InvoicesNow.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the sellers backend using 
    /// SQL via Entity Framework Core 3.1.
    /// </summary>
    public class SqlSeller : InterfaceSeller
    {
        InvoicesNowDbContext db { get; }

        public SqlSeller(InvoicesNowDbContext invoicesNowContext)
        {
            db = invoicesNowContext;
        }

        public async Task<IEnumerable<Seller>> GetAllSellersAsync()
        {
            return await db.Sellers
                .ToListAsync();
        }

        public async Task<Seller> GetSellerAsync(Guid sellerId)
        {
            return await db.Sellers
                .FirstOrDefaultAsync(seller => seller.SellerId == sellerId);
        }

        public async Task<Seller> UpsertAsync(Seller seller)
        {
            if (seller.SellerId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(seller));
            }

            Seller existingSeller = await db.Sellers.FindAsync(seller.SellerId);
            if (existingSeller == null)
            {
                db.Sellers.Add(seller);
            }
            else
            {
                db.Entry(existingSeller).CurrentValues.SetValues(seller);
            }
            await db.SaveChangesAsync();
            return seller;
        }

        public async Task DeleteAsync(Guid sellerId)
        {
            Seller existingSeller = await db.Sellers.FindAsync(sellerId);
            if (existingSeller != null)
            {
                db.Sellers.Remove(existingSeller);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Seller> FindExistingSeller(Seller newSeller)
        {
            return await db.Sellers.FirstOrDefaultAsync(seller =>
                seller.SellerName.Equals(newSeller.SellerName) 
                && seller.SellerEmail.Equals(newSeller.SellerEmail) 
                && seller.SellerAddress.Equals(newSeller.SellerAddress)
                && seller.SellerPhonenumber.Equals(newSeller.SellerPhonenumber)
                && seller.SellerAccount.Equals(newSeller.SellerAccount)
                && seller.SellerSWIFTBIC.Equals(newSeller.SellerSWIFTBIC)
                && seller.SellerIBAN.Equals(newSeller.SellerIBAN));
        }
        
        //public async Task<int> GetRecordCountAsync()
        //{
        //    return await db.Sellers
        //       .CountAsync();
        //}
    }
}

