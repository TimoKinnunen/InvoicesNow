using InvoicesNow.Helpers;
using InvoicesNow.Models;
using InvoicesNow.Printing.ViewModels;
using InvoicesNow.Projections;
using InvoicesNow.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class InvoicesListPage : Page
    {
        Guid InvoiceId { get; set; }

        MainPage MainPage { get; }

        ObservableCollection<InvoiceListViewModel> InvoiceListViewModels { get; set; } = new ObservableCollection<InvoiceListViewModel>();

        IEnumerable<Invoice> AllInvoices { get; set; }

        HeaderInvoiceListViewModel HeaderInvoiceListViewModel { get; set; } = new HeaderInvoiceListViewModel();

        CultureInfo CurrentCulture { get; } = CultureInfo.CurrentCulture;

        bool SearchWasActive { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        string translatedInvoice { get; set; }

        public InvoicesListPage()
        {
            InitializeComponent();

            Loaded += InvoiceListPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void InvoiceListPage_Loaded(object sender, RoutedEventArgs e)
        {
            AllInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

            FillInvoiceListView();

            List<TranslationViewModel> TranslationViewModels = await HelpTranslationsFromLocalFolder.GetCurrentCultureTranslationsAsync().ConfigureAwait(false);
            TranslateToLanguage(TranslationViewModels);

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

        private void FillInvoiceListView()
        {
            ExportDataAppBarButton.IsEnabled = AllInvoices.Count() > 0 ? true : false;

            InvoiceListView.DataContext = new HeaderInvoiceListViewModel();

            InvoiceListViewModels.Clear();
            foreach (Invoice invoice in AllInvoices
                        .OrderByDescending(v => v.InvoiceNumber)
                        .ThenBy(v => v.InvoiceDate)
                        .ThenBy(v => v.SellerName)
                        .ThenBy(v => v.BuyerName)
                        .ThenBy(v => v.TotalIncludingTax))
            {
                InvoiceListViewModel invoiceListViewModelToAdd = ProjectToViewModel.NewInvoiceListViewModel(invoice);
                InvoiceListViewModels.Add(invoiceListViewModelToAdd);
            }

            InvoiceListViewModel invoiceListViewSelectedItem = InvoiceListViewModels.FirstOrDefault(o => o.InvoiceListViewModelId == InvoiceId);
            if (invoiceListViewSelectedItem != null)
            {
                InvoiceListView.SelectedItem = invoiceListViewSelectedItem;
            }
            else
            {
                var first = InvoiceListViewModels.FirstOrDefault();
                if (first != null)
                {
                    InvoiceListView.SelectedItem = first;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                InvoiceId = Guid.Parse(e.Parameter.ToString());
            }
            // code here
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // code here
            App.LatestVisitedInvoiceId = InvoiceId;
            // code here
        }

        private void SearchInvoiceTextChanged(object sender, TextChangedEventArgs e)
        {
            string invoiceSearchText = SearchInvoiceTextBox.Text.Trim();
            if (string.IsNullOrEmpty(invoiceSearchText))
            {
                MainPage.NotifyUser("Search string is empty.", NotifyType.StatusMessage);

                if (SearchWasActive)
                {
                    FillInvoiceListView();
                    SearchWasActive = false;
                }

                return;
            }

            string[] parameters = invoiceSearchText.Split(new char[] { ' ' },
                       StringSplitOptions.RemoveEmptyEntries);
            var matches = AllInvoices.Where(invoice => parameters
                 .Any(parameter =>
                     invoice.InvoiceNumber.ToString(CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (invoice.InvoiceDate != null && invoice.InvoiceDate.ToString("d", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (invoice.SellerName != null && invoice.SellerName.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (invoice.BuyerName != null && invoice.BuyerName.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (invoice.TotalIncludingTax.ToString("c", CurrentCulture) != null && invoice.TotalIncludingTax.ToString("c", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase))))
                 .OrderByDescending(invoice => parameters.Count(parameter =>
                     invoice.InvoiceNumber.ToString(CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (invoice.InvoiceDate != null && invoice.InvoiceDate.ToString("d", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (invoice.SellerName != null && invoice.SellerName.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (invoice.BuyerName != null && invoice.BuyerName.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (invoice.TotalIncludingTax.ToString("c", CurrentCulture) != null ? invoice.TotalIncludingTax.ToString("c", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase) : false)))
                 .ToList();

            if (matches.Count == 0)
            {
                MainPage.NotifyUser("No matches found. Try again.", NotifyType.StatusMessage);

                return;
            }

            SearchWasActive = true;

            InvoiceListView.ItemsSource = null;
            InvoiceListView.DataContext = new HeaderInvoiceListViewModel();
            InvoiceListViewModels.Clear();
            foreach (Invoice invoice in matches
                        .OrderBy(v => v.InvoiceNumber)
                        .ThenBy(v => v.InvoiceDate)
                        .ThenBy(v => v.SellerName)
                        .ThenBy(v => v.BuyerName)
                        .ThenBy(v => v.TotalIncludingTax))

            {
                InvoiceListViewModels.Add(ProjectToViewModel.NewInvoiceListViewModel(invoice));
            }
            InvoiceListView.ItemsSource = InvoiceListViewModels;

            InvoiceListViewModel invoiceListViewSelectedItem = InvoiceListViewModels.FirstOrDefault(o => o.InvoiceListViewModelId == InvoiceId);
            if (invoiceListViewSelectedItem != null)
            {
                InvoiceListView.SelectedItem = invoiceListViewSelectedItem;
            }
        }

        private void InvoiceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoiceListView.SelectedItem == null)
            {
                EmailAppBarButton.IsEnabled = false;
                EditAppBarButton.IsEnabled = false;
                DeleteAppBarButton.IsEnabled = false;
                CopyAppBarButton.IsEnabled = false;
                PrintAppBarButton.IsEnabled = false;
            }
            else
            {
                InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                if (invoiceListViewModel != null)
                {
                    EmailAppBarButton.IsEnabled = string.IsNullOrEmpty(invoiceListViewModel.BuyerEmail) == true ? false : true;
                }
                EditAppBarButton.IsEnabled = true;
                DeleteAppBarButton.IsEnabled = true;
                CopyAppBarButton.IsEnabled = true;
                PrintAppBarButton.IsEnabled = true;
            }
        }

        private async void DeleteAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (InvoiceListView.SelectedItem != null)
                {
                    //ContentDialog deleteRecordDialog = new ContentDialog
                    //{
                    //    Title = "Delete invoice permanently?",
                    //    Content = "If you delete this invoice, you won't be able to recover it. Do you want to delete it?",
                    //    PrimaryButtonText = "Delete",
                    //    CloseButtonText = "Cancel"
                    //};

                    //ContentDialogResult result = await deleteRecordDialog.ShowAsync();

                    //// Delete the record if the user clicked the primary button.
                    //// Otherwise, do nothing.
                    //if (result == ContentDialogResult.Primary)
                    //{
                    // Delete the record.
                    InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;

                    InvoiceId = invoiceListViewModel.InvoiceListViewModelId; // this will be deleted

                    await App.Repository.Invoices.DeleteAsync(InvoiceId).ConfigureAwait(false);

                    MainPage.NotifyUser($"Invoice was deleted.", NotifyType.StatusMessage);

                    AllInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

                    FillInvoiceListView();
                    //}
                }
            }
        }

        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToHomePage();
            }
        }

        private async void EmailAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (InvoiceListView.SelectedItem != null)
                {
                    InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                    if (invoiceListViewModel != null)
                    {
                        if (!string.IsNullOrEmpty(invoiceListViewModel.BuyerEmail))
                        {
                            if (!string.IsNullOrEmpty(invoiceListViewModel.BuyerName))
                            {
                                var invoiceDate = invoiceListViewModel.InvoiceDate.ToString("d", CurrentCulture);
                                var emailMessage = new EmailMessage
                                {
                                    Subject = $"{translatedInvoice} {invoiceListViewModel.InvoiceNumber} {invoiceDate}",
                                    Body = $"{invoiceListViewModel.BuyerName}",
                                };

                                var emailRecipient = new EmailRecipient(invoiceListViewModel.BuyerEmail);
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

        private void EditAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (InvoiceListView.SelectedItem != null)
                {
                    InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                    InvoiceId = invoiceListViewModel.InvoiceListViewModelId;
                    MainPage.GoToInvoicePage("Edit invoice", InvoiceId);
                }
            }
        }

        private void InvoiceListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (InvoiceListView.SelectedItem != null)
            {
                InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                InvoiceId = invoiceListViewModel.InvoiceListViewModelId;
                MainPage.GoToInvoicePage("Edit invoice", InvoiceId);
            }
        }

        private void NewAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToInvoicePage("New invoice", Guid.Empty);
            }
        }

        private async void CopyAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (InvoiceListView.SelectedItem != null)
                {
                    InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;

                    InvoiceId = invoiceListViewModel.InvoiceListViewModelId;

                    Invoice existingInvoice = await App.Repository.Invoices.GetInvoiceAsync(InvoiceId).ConfigureAwait(false);
                    if (existingInvoice != null)
                    {
                        var invoiceDate = existingInvoice.InvoiceDate;
                        var invoiceNumber = App.UseSerieAsInvoiceNumber == true ? await HelpInvoiceNumber.GetNewSerieInvoiceNumberAsync().ConfigureAwait(false) : await HelpInvoiceNumber.GetNewDateInvoiceNumberAsync(invoiceDate).ConfigureAwait(false);
                        Invoice newInvoice = new Invoice(invoiceDate, invoiceNumber)
                        {
                            InvoiceNumber = invoiceNumber,

                            InvoiceDate = invoiceDate,

                            InvoiceInfoToBuyer = existingInvoice.InvoiceInfoToBuyer,

                            TotalIncludingTax = existingInvoice.TotalIncludingTax,
                            TotalExcludingTax = existingInvoice.TotalExcludingTax,
                            TotalTax = existingInvoice.TotalTax,

                            NetPaymentTermDays = existingInvoice.NetPaymentTermDays,
                            NetPaymentDueDate = existingInvoice.NetPaymentDueDate,

                            SellerName = existingInvoice.SellerName,
                            SellerEmail = existingInvoice.SellerEmail,
                            SellerAddress = existingInvoice.SellerAddress,
                            SellerPhonenumber = existingInvoice.SellerPhonenumber,
                            SellerAccount = existingInvoice.SellerAccount,
                            SellerSWIFTBIC = existingInvoice.SellerSWIFTBIC,
                            SellerIBAN = existingInvoice.SellerIBAN,
                            SellerId = existingInvoice.SellerId,

                            BuyerName = existingInvoice.BuyerName,
                            BuyerEmail = existingInvoice.BuyerEmail,
                            BuyerAddress = existingInvoice.BuyerAddress,
                            BuyerPhonenumber = existingInvoice.BuyerPhonenumber,
                            BuyerId = existingInvoice.BuyerId,
                        };

                        foreach (var invoiceItem in existingInvoice.InvoiceItems)
                        {
                            newInvoice.InvoiceItems.Add(new InvoiceItem(invoiceItem.Name, newInvoice.InvoiceId)
                            {
                                Quantity = invoiceItem.Quantity,
                                Tax = invoiceItem.Tax,
                                Price = invoiceItem.Price,
                            });
                        }

                        Invoice savedInvoice = await App.Repository.Invoices.InsertAsync(newInvoice).ConfigureAwait(false);
                        if (savedInvoice != null)
                        {
                            InvoiceId = savedInvoice.InvoiceId;
                            MainPage.NotifyUser("Invoice was copied.", NotifyType.StatusMessage);
                        }
                        else
                        {
                            MainPage.NotifyUser("Invoice was not copied. Something went wrong. Try again.", NotifyType.StatusMessage);
                        }

                        AllInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

                        FillInvoiceListView();
                    }
                }
            }
        }

        #region sort table header
        bool sortAscendingByInvoiceNumber;
        private void TableHeaderInvoiceNumberTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = InvoiceListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByInvoiceNumber)
                {
                    case true:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number ↑";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                    default:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number ↓";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                }
                InvoiceListView.DataContext = HeaderInvoiceListViewModel;
                InvoiceListView.ItemsSource = sortAscendingByInvoiceNumber == true ? InvoiceListViewModels.OrderByDescending(v => v.InvoiceNumber) : InvoiceListViewModels.OrderBy(v => v.InvoiceNumber);
                sortAscendingByInvoiceNumber = !sortAscendingByInvoiceNumber;
            }
            if (selectedItem != null)
            {
                InvoiceListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByInvoiceDate;
        private void TableHeaderInvoiceDateTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = InvoiceListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByInvoiceDate)
                {
                    case true:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date ↓";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                    default:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date ↑";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                }
                InvoiceListView.DataContext = HeaderInvoiceListViewModel;
                InvoiceListView.ItemsSource = sortAscendingByInvoiceDate == true ? InvoiceListViewModels.OrderBy(v => v.InvoiceDate) : InvoiceListViewModels.OrderByDescending(v => v.InvoiceDate);
                sortAscendingByInvoiceDate = !sortAscendingByInvoiceDate;
            }
            if (selectedItem != null)
            {
                InvoiceListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingBySellerName;
        private void TableHeaderSellerNameTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = InvoiceListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingBySellerName)
                {
                    case true:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name ↓";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                    default:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name ↑";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                }
                InvoiceListView.DataContext = HeaderInvoiceListViewModel;
                InvoiceListView.ItemsSource = sortAscendingBySellerName == true ? InvoiceListViewModels.OrderBy(v => v.SellerName) : InvoiceListViewModels.OrderByDescending(v => v.SellerName);
                sortAscendingBySellerName = !sortAscendingBySellerName;
            }
            if (selectedItem != null)
            {
                InvoiceListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByBuyerName;
        private void TableHeaderBuyerNameTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = InvoiceListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByBuyerName)
                {
                    case true:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name ↓";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                    default:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name ↑";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax";
                        break;
                }
                InvoiceListView.DataContext = HeaderInvoiceListViewModel;
                InvoiceListView.ItemsSource = sortAscendingByBuyerName == true ? InvoiceListViewModels.OrderBy(v => v.BuyerName) : InvoiceListViewModels.OrderByDescending(v => v.BuyerName);
                sortAscendingByBuyerName = !sortAscendingByBuyerName;
            }
            if (selectedItem != null)
            {
                InvoiceListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByTotalIncludingTax;
        private void TableHeaderTotalIncludingTaxTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = InvoiceListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByTotalIncludingTax)
                {
                    case true:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax ↓";
                        break;
                    default:
                        HeaderInvoiceListViewModel.HeaderInvoiceNumber = "Invoice number";
                        HeaderInvoiceListViewModel.HeaderInvoiceDate = "Invoice date";
                        HeaderInvoiceListViewModel.HeaderSellerName = "Seller's name";
                        HeaderInvoiceListViewModel.HeaderBuyerName = "Buyer's name";
                        HeaderInvoiceListViewModel.HeaderTotalIncludingTax = "Total including tax ↑";
                        break;
                }
                InvoiceListView.DataContext = HeaderInvoiceListViewModel;
                InvoiceListView.ItemsSource = sortAscendingByTotalIncludingTax == true ? InvoiceListViewModels.OrderBy(v => v.TotalIncludingTax) : InvoiceListViewModels.OrderByDescending(v => v.TotalIncludingTax);
                sortAscendingByTotalIncludingTax = !sortAscendingByTotalIncludingTax;
            }
            if (selectedItem != null)
            {
                InvoiceListView.SelectedItem = selectedItem;
            }
        }
        #endregion sort table header

        private void CancelSearchInvoiceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchInvoiceTextBox.Text))
            {
                MainPage.NotifyUser("Nothing to cancel.", NotifyType.StatusMessage);
            }
            SearchInvoiceTextBox.Text = string.Empty;
        }

        private void PrintAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (InvoiceListView.SelectedItem != null)
                {
                    InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                    InvoiceId = invoiceListViewModel.InvoiceListViewModelId;
                    MainPage.GoToPrintInvoicePage(InvoiceId);
                }
            }
        }

        private async void ExportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                string suggestedFileName = $"{HelpFileName.AddDateTimeNowToFileName("InvoicesNow_Invoices")}.json";
                StorageFile storageFile = await HelpFileSavePicker.GetStorageFileForJsonAsync(suggestedFileName).ConfigureAwait(false);
                if (storageFile != null)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ExportDataAppBarButton.IsEnabled = false;
                        ImportDataAppBarButton.IsEnabled = false;
                        ExportDataProgressRing.Visibility = Visibility.Visible;
                        ExportDataProgressRing.IsEnabled = true;
                        ExportDataProgressRing.IsActive = true;

                        EmailAppBarButton.IsEnabled = false;
                        EditAppBarButton.IsEnabled = false;
                        DeleteAppBarButton.IsEnabled = false;
                        NewAppBarButton.IsEnabled = false;
                        CopyAppBarButton.IsEnabled = false;
                        PrintAppBarButton.IsEnabled = false;
                    });

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainPage.NotifyUser("Export data. Please wait.", NotifyType.StatusMessage);
                    });

                    IEnumerable<Invoice> allInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

                    List<DTOInvoice> allDTOInvoices = new List<DTOInvoice>();
                    foreach (var invoice in allInvoices)
                    {
                        DTOInvoice DTOInvoice = new DTOInvoice()
                        {
                            InvoiceId = invoice.InvoiceId,
                            InvoiceNumber = invoice.InvoiceNumber,

                            CreatedAtDateTime = invoice.CreatedAtDateTime,
                            UpdatedAtDateTime = invoice.UpdatedAtDateTime,

                            InvoiceDate = invoice.InvoiceDate,
                            SellerName = invoice.SellerName,
                            BuyerName = invoice.BuyerName,

                            InvoiceInfoToBuyer = invoice.InvoiceInfoToBuyer,

                            TotalIncludingTax = invoice.TotalIncludingTax,
                            TotalExcludingTax = invoice.TotalExcludingTax,
                            TotalTax = invoice.TotalTax,

                            NetPaymentTermDays = invoice.NetPaymentTermDays,

                            NetPaymentDueDate = invoice.NetPaymentDueDate,

                            SellerEmail = invoice.SellerEmail,
                            SellerAddress = invoice.SellerAddress,
                            SellerPhonenumber = invoice.SellerPhonenumber,
                            SellerAccount = invoice.SellerAccount,
                            SellerSWIFTBIC = invoice.SellerSWIFTBIC,
                            SellerIBAN = invoice.SellerIBAN,
                            SellerId = invoice.SellerId,

                            BuyerEmail = invoice.BuyerEmail,
                            BuyerAddress = invoice.BuyerAddress,
                            BuyerPhonenumber = invoice.BuyerPhonenumber,
                            BuyerId = invoice.BuyerId,
                        };

                        foreach (var invoiceItem in invoice.InvoiceItems)
                        {
                            DTOInvoice.DTOInvoiceItems.Add(new DTOInvoiceItem()
                            {
                                InvoiceItemId = invoiceItem.InvoiceItemId,
                                Name = invoiceItem.Name,
                                InvoiceId = invoiceItem.InvoiceId,
                                CreatedAtDateTime = invoiceItem.CreatedAtDateTime,
                                UpdatedAtDateTime = invoiceItem.UpdatedAtDateTime,
                                Quantity = invoiceItem.Quantity,
                                Tax = invoiceItem.Tax,
                                Price = invoiceItem.Price,
                            });
                        }

                        allDTOInvoices.Add(DTOInvoice);
                    }

                    string jsonData = await Task.Run(() => JsonConvert.SerializeObject(allDTOInvoices)).ConfigureAwait(false);

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(storageFile);
                        // write to file
                        await FileIO.WriteTextAsync(storageFile, jsonData);
                        // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                        // Completing updates may require Windows to ask for user input.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(storageFile);
                        if (status == FileUpdateStatus.Complete)
                        {
                            MainPage.NotifyUser($"File '{storageFile.Name}' was saved.", NotifyType.StatusMessage);
                        }
                        else
                        {
                            MainPage.NotifyUser($"File '{storageFile.Name}' couldn't be saved.", NotifyType.ErrorMessage);
                        }

                        ExportDataAppBarButton.IsEnabled = true;
                        ImportDataAppBarButton.IsEnabled = true;
                        ExportDataProgressRing.Visibility = Visibility.Collapsed;
                        ExportDataProgressRing.IsEnabled = false;
                        ExportDataProgressRing.IsActive = false;

                        NewAppBarButton.IsEnabled = true;

                        if (InvoiceListView.SelectedItem != null)
                        {
                            InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                            if (invoiceListViewModel != null)
                            {
                                EmailAppBarButton.IsEnabled = string.IsNullOrEmpty(invoiceListViewModel.BuyerEmail) == true ? false : true;
                            }
                            EditAppBarButton.IsEnabled = true;
                            DeleteAppBarButton.IsEnabled = true;
                            CopyAppBarButton.IsEnabled = true;
                            PrintAppBarButton.IsEnabled = true;
                        }
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainPage.NotifyUser("Operation canceled.", NotifyType.StatusMessage);
                    });
                }
            }
        }

        private async void ImportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                StorageFile storageFile = await HelpFileOpenPicker.PickJsonFileAsync().ConfigureAwait(false);
                if (storageFile != null)
                {
                    int importedDTOInvoiceCount = 0;
                    int existingInvoiceCount = 0;

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ExportDataAppBarButton.IsEnabled = false;
                        ImportDataAppBarButton.IsEnabled = false;
                        ImportDataProgressRing.Visibility = Visibility.Visible;
                        ImportDataProgressRing.IsEnabled = true;
                        ImportDataProgressRing.IsActive = true;

                        EmailAppBarButton.IsEnabled = false;
                        EditAppBarButton.IsEnabled = false;
                        DeleteAppBarButton.IsEnabled = false;
                        NewAppBarButton.IsEnabled = false;
                        CopyAppBarButton.IsEnabled = false;
                    });

                    try
                    {
                        string fileContent = await FileIO.ReadTextAsync(storageFile);
                        List<DTOInvoice> importedDTOInvoices = JsonConvert.DeserializeObject<List<DTOInvoice>>(fileContent);
                        foreach (DTOInvoice importedDTOInvoice in importedDTOInvoices)
                        {
                            Invoice newImportedInvoice = new Invoice(importedDTOInvoice);

                            if (!newImportedInvoice.InvoiceId.Equals(Guid.Empty))
                            {
                                Invoice existingInvoice = await App.Repository.Invoices.GetInvoiceAsync(newImportedInvoice.InvoiceId).ConfigureAwait(false);
                                if (existingInvoice != null)
                                {
                                    if (importedDTOInvoice.UpdatedAtDateTime > existingInvoice.UpdatedAtDateTime)
                                    {
                                        await App.Repository.Invoices.DeleteAsync(InvoiceId).ConfigureAwait(false);

                                        Invoice savedInvoice = await App.Repository.Invoices.InsertAsync(newImportedInvoice).ConfigureAwait(false);
                                        if (savedInvoice != null)
                                        {
                                            importedDTOInvoiceCount++;
                                        }
                                        else
                                        {
                                            MainPage.NotifyUser("Imported invoice was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                                        }
                                    }
                                    else
                                    {
                                        existingInvoiceCount++;
                                    }
                                }
                                else
                                {
                                    Invoice savedInvoice = await App.Repository.Invoices.InsertAsync(newImportedInvoice).ConfigureAwait(false);
                                    if (savedInvoice != null)
                                    {
                                        importedDTOInvoiceCount++;
                                    }
                                    else
                                    {
                                        MainPage.NotifyUser("Imported invoice was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                                    }
                                }
                            }
                        }

                        if (importedDTOInvoiceCount > 0)
                        {
                            AllInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(true);

                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                FillInvoiceListView();

                                MainPage.NotifyUser($"Imported {importedDTOInvoiceCount}({importedDTOInvoiceCount + existingInvoiceCount}) {(importedDTOInvoiceCount == 1 ? "invoice" : "invoices")}.", NotifyType.StatusMessage);
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (existingInvoiceCount == 0)
                                {
                                    MainPage.NotifyUser($"File didn't contain invoices. Try again.", NotifyType.ErrorMessage);
                                }
                                else
                                {
                                    MainPage.NotifyUser($"Didn't import {existingInvoiceCount}({importedDTOInvoiceCount + existingInvoiceCount}) {(existingInvoiceCount == 1 ? "duplicate invoice" : "duplicate invoices")}.", NotifyType.StatusMessage);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            MainPage.NotifyUser($"Operation failed. {ex.Message}", NotifyType.ErrorMessage);
                        });
                    }
                    finally
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ExportDataAppBarButton.IsEnabled = true;
                            ImportDataAppBarButton.IsEnabled = true;
                            ImportDataProgressRing.Visibility = Visibility.Collapsed;
                            ImportDataProgressRing.IsEnabled = false;
                            ImportDataProgressRing.IsActive = false;

                            NewAppBarButton.IsEnabled = true;

                            if (InvoiceListView.SelectedItem != null)
                            {
                                InvoiceListViewModel invoiceListViewModel = InvoiceListView.SelectedItem as InvoiceListViewModel;
                                if (invoiceListViewModel != null)
                                {
                                    EmailAppBarButton.IsEnabled = string.IsNullOrEmpty(invoiceListViewModel.BuyerEmail) == true ? false : true;
                                }
                                EditAppBarButton.IsEnabled = true;
                                DeleteAppBarButton.IsEnabled = true;
                                CopyAppBarButton.IsEnabled = true;
                            }
                        });
                    }
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainPage.NotifyUser("Operation canceled.", NotifyType.StatusMessage);
                    });
                }
            }
        }
    }
}
