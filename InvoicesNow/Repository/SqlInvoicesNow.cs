using InvoicesNow.Repository.Interfaces;
using InvoicesNow.Repository.Sql;
using Microsoft.EntityFrameworkCore;

namespace InvoicesNow.Repository
{
    /// <summary>
    /// Contains methods for interacting with the app backend using 
    /// SQL via Entity Framework Core 3.1. 
    /// </summary>
    public class SqlInvoicesNow : RepositoryInvoicesNow
    {
        DbContextOptions<InvoicesNowDbContext> dbContextOptions { get; }

        public SqlInvoicesNow(DbContextOptionsBuilder<InvoicesNowDbContext>
            dbOptionsBuilder)
        {
            if (dbOptionsBuilder != null)
            {
                dbContextOptions = dbOptionsBuilder.Options;
                using (InvoicesNowDbContext db = new InvoicesNowDbContext(dbContextOptions))
                {
                    var success = db.Database.EnsureCreated();
                    if (success)
                    {
                    }
                }
            }
        }

        public InterfaceSeller Sellers => new SqlSeller(
            new InvoicesNowDbContext(dbContextOptions));

        public InterfaceBuyer Buyers => new SqlBuyer(
            new InvoicesNowDbContext(dbContextOptions));

        public InterfaceInvoice Invoices => new SqlInvoice(
            new InvoicesNowDbContext(dbContextOptions));

        public InterfaceItem Items => new SqlItem(
            new InvoicesNowDbContext(dbContextOptions));

        public InterfaceInvoiceItem InvoiceItems => new SqlInvoiceItem(
            new InvoicesNowDbContext(dbContextOptions));
    }
}
