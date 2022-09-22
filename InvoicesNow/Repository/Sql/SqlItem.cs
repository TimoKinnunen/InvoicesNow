using InvoicesNow.Models;
using InvoicesNow.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicesNow.Repository.Sql
{
    /// <summary>
    /// Contains methods for interacting with the items backend using 
    /// SQL via Entity Framework Core 3.1.
    /// </summary>
    public class SqlItem : InterfaceItem
    {
        InvoicesNowDbContext db { get; }

        public SqlItem(InvoicesNowDbContext oneToManyContext) => db = oneToManyContext;

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await db.Items
                .ToListAsync();
        }

        public async Task<Item> GetItemAsync(Guid itemId)
        {
            return await db.Items
                .FirstOrDefaultAsync(item => item.ItemId == itemId);
        }

        public async Task<Item> UpsertAsync(Item item)
        {
            if (item.ItemId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(item));
            }

            Item existingItem = await db.Items
                .FirstOrDefaultAsync(o => o.ItemId == item.ItemId);
            if (existingItem == null)
            {
                item.UpdatedAtDateTime = DateTime.Now;

                db.Items.Add(item);
            }
            else
            {
                existingItem.UpdatedAtDateTime = DateTime.Now;

                db.Entry(existingItem).CurrentValues.SetValues(item);
            }
            await db.SaveChangesAsync();
            return item;
        }

        public async Task DeleteAsync(Guid itemId)
        {
            Item existingItem = await db.Items
                .FirstOrDefaultAsync(o => o.ItemId == itemId);
            if (existingItem != null)
            {
                db.Items.Remove(existingItem);
                await db.SaveChangesAsync();
            }
        }
    }
}