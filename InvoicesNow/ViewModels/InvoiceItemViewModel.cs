using InvoicesNow.Models;
using System;

namespace InvoicesNow.ViewModels
{
    //public class InvoiceItemViewModel : INotifyPropertyChanged
    public class InvoiceItemViewModel
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        //private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        public InvoiceItemViewModel(InvoiceItem invoiceItem)
        {
            InvoiceItemViewModelId = invoiceItem.InvoiceItemId;
            Name = invoiceItem.Name;

            InvoiceId = invoiceItem.InvoiceId; // FKInvoiceId belongs to this parent

            CreatedAtDateTime = invoiceItem.CreatedAtDateTime;
            UpdatedAtDateTime = invoiceItem.UpdatedAtDateTime;

            //DoubleInvoiceItemQuantityNumberBox = Convert.ToDouble(invoiceItem.Quantity);

            Quantity = invoiceItem.Quantity;
            Tax = invoiceItem.Tax;
            Price = invoiceItem.Price;
        }

        public Guid InvoiceItemViewModelId { get; set; }
        public string Name { get; set; }

        public Guid InvoiceId { get; set; } // FKInvoiceId belongs to this parent

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        //public double DoubleInvoiceItemQuantityNumberBox { get; set; } // NumberBox help
        //private double doubleInvoiceItemQuantityNumberBox;
        //public double DoubleInvoiceItemQuantityNumberBox
        //{
        //    get { return doubleInvoiceItemQuantityNumberBox; }
        //    set
        //    {
        //        if (value != doubleInvoiceItemQuantityNumberBox)
        //        {
        //            doubleInvoiceItemQuantityNumberBox = value;
        //            NotifyPropertyChanged();
        //        }
        //    }
        //}

        public decimal Quantity { get; set; }
        //private decimal quantity;
        //public decimal Quantity
        //{
        //    get { return quantity; }
        //    set
        //    {
        //        if (value != quantity)
        //        {
        //            quantity = value;
        //            NotifyPropertyChanged();
        //        }
        //    }
        //}
        public decimal Tax { get; set; }
        public decimal Price { get; set; }
    }
}