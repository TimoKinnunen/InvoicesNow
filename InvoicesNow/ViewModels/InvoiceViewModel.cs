using InvoicesNow.Models;
using InvoicesNow.Projections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InvoicesNow.ViewModels
{
    public class InvoiceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public InvoiceViewModel()
        {
            InvoiceItemViewModels = new List<InvoiceItemViewModel>();
        }

        public InvoiceViewModel(Invoice invoice)
        {
            InvoiceViewModelId = invoice.InvoiceId;
            InvoiceViewModelId = invoice.InvoiceId;
            InvoiceNumber = invoice.InvoiceNumber;
            InvoiceDate = invoice.InvoiceDate;

            CreatedAtDateTime = invoice.CreatedAtDateTime;
            UpdatedAtDateTime = invoice.UpdatedAtDateTime;

            InvoiceInfoToBuyer = invoice.InvoiceInfoToBuyer;

            TotalIncludingTax = invoice.TotalIncludingTax;
            TotalExcludingTax = invoice.TotalExcludingTax;
            TotalTax = invoice.TotalTax;

            NetPaymentTermDays = invoice.NetPaymentTermDays;

            SellerName = invoice.SellerName;
            SellerEmail = invoice.SellerEmail;
            SellerAddress = invoice.SellerAddress;
            SellerPhonenumber = invoice.SellerPhonenumber;

            SellerAccount = invoice.SellerAccount;
            SellerSWIFTBIC = invoice.SellerSWIFTBIC;
            SellerIBAN = invoice.SellerIBAN;
            SellerId = invoice.SellerId;

            BuyerName = invoice.BuyerName;
            BuyerEmail = invoice.BuyerEmail;
            BuyerAddress = invoice.BuyerAddress;
            BuyerPhonenumber = invoice.BuyerPhonenumber;
            BuyerId = invoice.BuyerId;

            InvoiceItemViewModels = new List<InvoiceItemViewModel>();

            foreach (var invoiceItem in invoice.InvoiceItems)
            {
                InvoiceItemViewModels.Add(ProjectToViewModel.NewInvoiceItemViewModel(invoiceItem));
            }
        }

        public Guid InvoiceViewModelId { get; set; }
        //public int InvoiceNumber { get; set; }
        int invoiceNumber;
        public int InvoiceNumber
        {
            get { return invoiceNumber; }
            set
            {
                if (value != invoiceNumber)
                {
                    invoiceNumber = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DateTime CreatedAtDateTime { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public DateTime InvoiceDate { get; set; }
        //public string InvoiceInfoToBuyer { get; set; }
        string invoiceInfoToBuyer;
        public string InvoiceInfoToBuyer
        {
            get { return invoiceInfoToBuyer; }
            set
            {
                if (value != invoiceInfoToBuyer)
                {
                    invoiceInfoToBuyer = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //public decimal TotalIncludingTax { get; set; }
        private decimal totalIncludingTax;
        public decimal TotalIncludingTax
        {
            get { return totalIncludingTax; }
            set
            {
                if (value != totalIncludingTax)
                {
                    totalIncludingTax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //public decimal TotalExcludingTax { get; set; }
        private decimal totalExcludingTax;
        public decimal TotalExcludingTax
        {
            get { return totalExcludingTax; }
            set
            {
                if (value != totalExcludingTax)
                {
                    totalExcludingTax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //public decimal TotalTax { get; set; }
        private decimal totalTax;
        public decimal TotalTax
        {
            get { return totalTax; }
            set
            {
                if (value != totalTax)
                {
                    totalTax = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int NetPaymentTermDays { get; set; }

        public DateTime NetPaymentDueDate { get; set; }

        //public string SellerName { get; set; }
        string sellerName;
        public string SellerName
        {
            get { return sellerName; }
            set
            {
                if (value != sellerName)
                {
                    sellerName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string SellerEmail { get; set; }
        string sellerEmail;
        public string SellerEmail
        {
            get { return sellerEmail; }
            set
            {
                if (value != sellerEmail)
                {
                    sellerEmail = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string SellerAddress { get; set; }
        string sellerAddress;
        public string SellerAddress
        {
            get { return sellerAddress; }
            set
            {
                if (value != sellerAddress)
                {
                    sellerAddress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string SellerPhonenumber { get; set; }
        string sellerPhonenumber;
        public string SellerPhonenumber
        {
            get { return sellerPhonenumber; }
            set
            {
                if (value != sellerPhonenumber)
                {
                    sellerPhonenumber = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //public string SellerAccount { get; set; }
        string sellerAccount;
        public string SellerAccount
        {
            get { return sellerAccount; }
            set
            {
                if (value != sellerAccount)
                {
                    sellerAccount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string SellerSWIFTBIC { get; set; }
        string sellerSWIFTBIC;
        public string SellerSWIFTBIC
        {
            get { return sellerSWIFTBIC; }
            set
            {
                if (value != sellerSWIFTBIC)
                {
                    sellerSWIFTBIC = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string SellerIBAN { get; set; }
        string sellerIBAN;
        public string SellerIBAN
        {
            get { return sellerIBAN; }
            set
            {
                if (value != sellerIBAN)
                {
                    sellerIBAN = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Guid SellerId { get; set; }

        //public string BuyerName { get; set; }
        string buyerName;
        public string BuyerName
        {
            get { return buyerName; }
            set
            {
                if (value != buyerName)
                {
                    buyerName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string BuyerEmail { get; set; }
        string buyerEmail;
        public string BuyerEmail
        {
            get { return buyerEmail; }
            set
            {
                if (value != buyerEmail)
                {
                    buyerEmail = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string BuyerAddress { get; set; }
        string buyerAddress;
        public string BuyerAddress
        {
            get { return buyerAddress; }
            set
            {
                if (value != buyerAddress)
                {
                    buyerAddress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        //public string BuyerPhonenumber { get; set; }
        string buyerPhonenumber;
        public string BuyerPhonenumber
        {
            get { return buyerPhonenumber; }
            set
            {
                if (value != buyerPhonenumber)
                {
                    buyerPhonenumber = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Guid BuyerId { get; set; }

        public ICollection<InvoiceItemViewModel> InvoiceItemViewModels { get; set; }
    }
}
