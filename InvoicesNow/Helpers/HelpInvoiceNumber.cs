using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InvoicesNow.Helpers
{
    public static class HelpInvoiceNumber
    {
        public static async Task<int> GetNewDateInvoiceNumberAsync(DateTime dateOfInvoice)
        {
            string date = dateOfInvoice.ToString("yyMMdd", CultureInfo.InvariantCulture);

            IEnumerable<Invoice> allInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

            List<int> dateAllInvoiceNumbers = allInvoices.Where(o => o.InvoiceNumber.ToString().StartsWith(date)).Select(o => o.InvoiceNumber).ToList();

            int newInvoiceNumber = int.Parse($"{date}{1}", CultureInfo.InvariantCulture);

            if (dateAllInvoiceNumbers.Count > 0)
            {
                int maximumNumberInList = dateAllInvoiceNumbers.OrderByDescending(x => x).First(); // maximum
                string invoiceNumberTrail = maximumNumberInList.ToString().Substring(6); // "2101291" -> "1", "21012910" -> "10"
                int newTrailingNumber = int.Parse(invoiceNumberTrail) + 1; // adds always one
                newInvoiceNumber = int.Parse($"{date}{newTrailingNumber}", CultureInfo.InvariantCulture);
            }

            return newInvoiceNumber;
        }

        public static async Task<int> GetNewSerieInvoiceNumberAsync()
        {
            int maxValue = App.SerieStartsWith;

            IEnumerable<Invoice> allInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

            List<int> serieAllInvoiceNumbers = allInvoices.Select(o => o.InvoiceNumber).ToList();

            if (serieAllInvoiceNumbers.Count > 0)
            {
                maxValue = Math.Max(maxValue, serieAllInvoiceNumbers.Max());
            }

            return maxValue + 1;
        }
    }
}
