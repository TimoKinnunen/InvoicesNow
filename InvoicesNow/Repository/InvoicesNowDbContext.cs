using InvoicesNow.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoicesNow.Repository
{
    /// <summary>
    /// Entity Framework Core DbContext for InvoicesNow.
    /// </summary>
    public class InvoicesNowDbContext : DbContext
    {
        /// <summary>
        /// Creates a new InvoicesNow DbContext.
        /// </summary>
        public InvoicesNowDbContext(DbContextOptions<InvoicesNowDbContext> options) : base(options)
        { }

        ///// <summary>
        ///// Gets the sellers DbSet.
        ///// </summary>
        public DbSet<Seller> Sellers { get; set; }
        ///// <summary>
        ///// Gets the buyers DbSet.
        ///// </summary>
        public DbSet<Buyer> Buyers { get; set; }
        ///// <summary>
        ///// Gets the invoices DbSet.
        ///// </summary>
        public DbSet<Invoice> Invoices { get; set; }
        ///// <summary>
        ///// Gets the order items DbSet.
        ///// </summary>
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        ///// <summary>
        ///// Gets the items DbSet.
        ///// </summary>
        public DbSet<Item> Items { get; set; }
    }
}