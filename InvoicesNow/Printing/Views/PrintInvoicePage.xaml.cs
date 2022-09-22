using InvoicesNow.Helpers;
using InvoicesNow.Models;
using InvoicesNow.Printing.ViewModels;
using InvoicesNow.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Printing.Views
{
    public sealed partial class PrintInvoicePage : Page
    {
        MainPage MainPage { get; }

        Guid InvoiceId { get; set; }

        Invoice invoice { get; set; }

        InvoicePrintHelper InvoicePrintHelper { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        List<PrintInvoiceItemViewModel> PrintInvoiceItemViewModels;

        CultureInfo CurrentCulture { get; } = CultureInfo.CurrentCulture;

        string translatedInvoice { get; set; }

        public PrintInvoicePage()
        {
            InitializeComponent();

            Loaded += PrintInvoicePage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void PrintInvoicePage_Loaded(object sender, RoutedEventArgs e)
        {
            invoice = await App.Repository.Invoices.GetInvoiceAsync(InvoiceId).ConfigureAwait(false);

            if (invoice != null)
            {
                List<TranslationViewModel> TranslationViewModels = await HelpTranslationsFromLocalFolder.GetCurrentCultureTranslationsAsync().ConfigureAwait(false);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PrintInvoiceViewModel printInvoiceViewModel = NewPrintInvoiceViewModel(invoice);

                    PrintInvoiceStackPanel.DataContext = TranslatePrintInvoiceViewModel(printInvoiceViewModel, TranslationViewModels);

                    PrintInvoiceItemViewModels = new List<PrintInvoiceItemViewModel>();
                    foreach (InvoiceItem invoiceItem in invoice.InvoiceItems.OrderBy(v => v.Name).ThenBy(v => v.Quantity).ThenBy(v => v.Tax).ThenBy(v => v.Price))
                    {
                        PrintInvoiceItemViewModels.Add(NewPrintInvoiceItemViewModel(invoiceItem));
                    }

                    PrintInvoiceItemListView.ItemsSource = PrintInvoiceItemViewModels;

                    PrintInvoiceItemListView.DataContext = TranslatePrintInvoiceItemHeaderViewModel(TranslationViewModels);

                    GetExistingSellerLogotype(invoice.SellerId);

                    EmailAppBarButton.IsEnabled = string.IsNullOrEmpty(invoice.BuyerEmail) == true ? false : true;

                    TranslateToLanguage(TranslationViewModels);
                });
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MainPage.NotifyUser("Invoice is missing. Try again.", NotifyType.ErrorMessage);
                });
            }
        }

        private void TranslateToLanguage(List<TranslationViewModel> TranslationViewModels)
        {
            TranslationViewModel translationViewModel;

            #region e-mail translations
            translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice");
            if (translationViewModel != null)
            {
                translatedInvoice = translationViewModel.TranslatedText;
            }
            #endregion e-mail translations
        }

        private async void GetExistingSellerLogotype(Guid SellerId)
        {
            StorageFolder logotypesStorageFolder = await GetLogotypesStorageFolder();
            IReadOnlyList<StorageFile> fileList = await logotypesStorageFolder.GetFilesAsync();
            StorageFile existingLogotype = fileList.FirstOrDefault(o => o.DisplayName.ToUpper() == SellerId.ToString().ToUpper());
            if (existingLogotype != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromFile(existingLogotype);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(await stream.OpenReadAsync());
                    SellerLogotypeImage.Source = bitmapImage;
                    SellerLogotypeImage.Visibility = Visibility.Visible;
                });
            }
        }

        private static async Task<StorageFolder> GetLogotypesStorageFolder()
        {
            // Get the app's local folder.
            StorageFolder localStorageFolder = ApplicationData.Current.LocalFolder;

            // Create a new subfolder in the current folder.
            StorageFolder logotypesStorageFolder = await localStorageFolder.CreateFolderAsync("Logotypes", CreationCollisionOption.OpenIfExists);
            return logotypesStorageFolder;
        }
        private static PrintInvoiceItemHeaderViewModel TranslatePrintInvoiceItemHeaderViewModel(List<TranslationViewModel> translationViewModels)
        {
            PrintInvoiceItemHeaderViewModel printInvoiceItemHeaderViewModel = new PrintInvoiceItemHeaderViewModel();

            TranslationViewModel translationViewModel;

            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Name");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderName = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Quantity");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderQuantity = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Tax");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Price");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderPrice = translationViewModel.TranslatedText;
            }

            return printInvoiceItemHeaderViewModel;
        }

        private static PrintInvoiceViewModel TranslatePrintInvoiceViewModel(PrintInvoiceViewModel printInvoiceViewModel, List<TranslationViewModel> translationViewModels)
        {
            TranslationViewModel translationViewModel;

            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateInvoice = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice number");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateInvoiceNumber = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Please pay latest at due date");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.InvoiceInfoToBuyer = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice date");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateInvoiceDate = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Due date");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateDueDate = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Amount to pay");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateAmountToPay = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Payment to account");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslatePaymentToAccount = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "SWIFT/BIC");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSWIFTBIC = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "IBAN");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateIBAN = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Total including tax");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateTotalIncludingTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Total excluding tax");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateTotalExcludingTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Total tax");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateTotalTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's name");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerName = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's e-mail address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerEmailAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's phonenumber");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerPhonenumber = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's name");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerName = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's e-mail address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerEmailAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's phonenumber");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerPhonenumber = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Page number");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslatePageNumber = translationViewModel.TranslatedText;
            }

            return printInvoiceViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                InvoiceId = Guid.Parse(e.Parameter.ToString());
            }

            if (PrintManager.IsSupported())
            {
                // Tell the user how to print
                MainPage.NotifyUser("Tapp Print button to preview and print.", NotifyType.StatusMessage);
            }
            else
            {
                // Remove the print button
                PrintInvoiceAppBarButton.Visibility = Visibility.Collapsed;

                // Inform user that Printing is not supported
                MainPage.NotifyUser("Printing is not supported.", NotifyType.ErrorMessage);

                // Printing-related event handlers will never be called if printing
                // is not supported, but it's okay to register for them anyway.
            }

            // Initalize common helper class and register for printing
            //invoicePrintHelper = new InvoicePrintHelper(this, invoiceId);
            InvoicePrintHelper = new InvoicePrintHelper(this);
            InvoicePrintHelper.RegisterForPrinting();

            // Initialize print content for this scenario
            InvoicePrintHelper.GetDataForPrintContent(InvoiceId);
            // code here
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // code here
            if (InvoicePrintHelper != null)
            {
                InvoicePrintHelper.UnregisterForPrinting();
            }
            // code here
        }

        private static PrintInvoiceViewModel NewPrintInvoiceViewModel(Invoice invoice)
        {
            return new PrintInvoiceViewModel
            {
                PrintInvoiceViewModelId = invoice.InvoiceId,
                CreatedAtDateTime = invoice.CreatedAtDateTime,
                UpdatedAtDateTime = invoice.UpdatedAtDateTime,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,

                InvoiceInfoToBuyer = invoice.InvoiceInfoToBuyer,

                TotalIncludingTax = invoice.TotalIncludingTax,
                TotalExcludingTax = invoice.TotalExcludingTax,
                TotalTax = invoice.TotalTax,

                NetPaymentDueDate = invoice.NetPaymentDueDate,
                NetPaymentTermDays = invoice.NetPaymentTermDays,

                SellerName = invoice.SellerName,
                SellerEmail = invoice.SellerEmail,
                SellerAddress = invoice.SellerAddress,
                SellerPhonenumber = invoice.SellerPhonenumber,
                SellerAccount = invoice.SellerAccount,
                SellerSWIFTBIC = invoice.SellerSWIFTBIC,
                SellerIBAN = invoice.SellerIBAN,
                SellerId = invoice.SellerId,

                BuyerName = invoice.BuyerName,
                BuyerEmail = invoice.BuyerEmail,
                BuyerAddress = invoice.BuyerAddress,
                BuyerPhonenumber = invoice.BuyerPhonenumber,
                BuyerId = invoice.BuyerId,
            };
        }

        private static PrintInvoiceItemViewModel NewPrintInvoiceItemViewModel(InvoiceItem invoiceItem)
        {
            return new PrintInvoiceItemViewModel
            {
                PrintInvoiceItemViewModelId = invoiceItem.InvoiceItemId,
                Name = invoiceItem.Name,
                Quantity = invoiceItem.Quantity,
                Tax = invoiceItem.Tax,
                Price = invoiceItem.Price,
            };
        }

        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToHomePage();
            }
        }

        private void EditAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToInvoicePage("Edit invoice", InvoiceId);
            }
        }

        private void BackAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToInvoicesListPage(InvoiceId);
            }
        }

        private async void PrintInvoiceAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                await InvoicePrintHelper.ShowPrintUIAsync().ConfigureAwait(false);
            }
        }

        private async void EmailAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (invoice != null)
                {
                    if (!string.IsNullOrEmpty(invoice.BuyerEmail))
                    {
                        if (!string.IsNullOrEmpty(invoice.BuyerName))
                        {
                            var invoiceDate = invoice.InvoiceDate.ToString("d", CurrentCulture);
                            var emailMessage = new EmailMessage
                            {
                                Subject = $"{translatedInvoice} {invoice.InvoiceNumber} {invoiceDate}",
                                Body = $"{invoice.BuyerName}",
                            };

                            var emailRecipient = new EmailRecipient(invoice.BuyerEmail);
                            emailMessage.To.Add(emailRecipient);

                            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
                        }
                        else
                        {
                            MainPage.NotifyUser("Buyer's name is missing.", NotifyType.StatusMessage);
                        }
                    }
                    else
                    {
                        MainPage.NotifyUser("Buyer's e-mailaddress is missing.", NotifyType.StatusMessage);
                    }
                }
            }
        }
    }
}

