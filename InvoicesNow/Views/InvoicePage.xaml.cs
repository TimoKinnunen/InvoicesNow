using InvoicesNow.Helpers;
using InvoicesNow.Models;
using InvoicesNow.Printing.ViewModels;
using InvoicesNow.Projections;
using InvoicesNow.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class InvoicePage : Page
    {

        MainPage MainPage { get; }

        Guid InvoiceId { get; set; }

        Guid ItemViewModelId { get; set; }

        Guid InvoiceItemViewModelId { get; set; }

        Guid ItemId { get; set; }

        InvoiceViewModel InvoiceViewModel { get; set; } = new InvoiceViewModel();

        CultureInfo CurrentCulture { get; } = CultureInfo.CurrentCulture;

        ObservableCollection<SellerViewModel> SellerViewModels { get; set; } = new ObservableCollection<SellerViewModel>();

        IEnumerable<Seller> AllSellers { get; set; }

        ObservableCollection<BuyerViewModel> BuyerViewModels { get; set; } = new ObservableCollection<BuyerViewModel>();

        IEnumerable<Buyer> AllBuyers { get; set; }

        IEnumerable<Item> AllItems { get; set; }

        ObservableCollection<ItemViewModel> ItemViewModels { get; set; } = new ObservableCollection<ItemViewModel>();

        ApplicationDataContainer LocalSettings { get; } = ApplicationData.Current.LocalSettings;

        ItemViewModel NewItemViewModel { get; set; } = new ItemViewModel(new Item()); // new item

        bool BuyerSearchWasActive { get; set; }

        bool SellerSearchWasActive { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        bool SearchWasActive { get; set; }

        /// <summary>
        /// Old values for seller and buyer come from old invoice 
        /// and should not be overwritten initially by comboboxes.
        /// For new invoice it does not matter.
        /// </summary>
        bool LetComboBoxOverwriteSellerAndBuyer { get; set; }

        public InvoicePage()
        {
            InitializeComponent();

            NewItemGrid.DataContext = NewItemViewModel;

            NetPaymentTermDaysComboBox.ItemsSource = new List<int> { 1, 5, 15, 30 };

            Loaded += InvoicePage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void InvoicePage_Loaded(object sender, RoutedEventArgs e)
        {
            var existingInvoice = await App.Repository.Invoices.GetInvoiceAsync(InvoiceId).ConfigureAwait(false);
            if (existingInvoice != null)
            {
                LetComboBoxOverwriteSellerAndBuyer = false;

                InvoiceViewModel = ProjectToViewModel.NewInvoiceViewModel(existingInvoice);
            }
            else
            {
                LetComboBoxOverwriteSellerAndBuyer = true;

                var invoiceDate = DateTime.Now;
                var invoiceNumber = App.UseSerieAsInvoiceNumber == true ? await HelpInvoiceNumber.GetNewSerieInvoiceNumberAsync().ConfigureAwait(false) : await HelpInvoiceNumber.GetNewDateInvoiceNumberAsync(invoiceDate).ConfigureAwait(false);
                Invoice invoice = new Invoice(invoiceDate, invoiceNumber);

                InvoiceId = invoice.InvoiceId;

                InvoiceViewModel = new InvoiceViewModel(invoice);
            }

            FillInvoiceItemListView();

            AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);
            FillBuyerGrid();

            AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);
            FillSellerGrid();

            AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);
            FillItemListView();

            if (InvoiceViewModel != null)
            {
                SaveAppBarButton.IsEnabled = true;

                InvoiceDateCalendarDatePicker.Date = InvoiceViewModel.InvoiceDate;

                NetPaymentTermDaysComboBox.SelectedItem = InvoiceViewModel.NetPaymentTermDays;

                ReadInvoicePageSettings();

                List<TranslationViewModel> TranslationViewModels = await HelpTranslationsFromLocalFolder.GetCurrentCultureTranslationsAsync().ConfigureAwait(false);
                TranslateToLanguage(TranslationViewModels);

                // ComboBox initialization is done for now and let user take control from here
                LetComboBoxOverwriteSellerAndBuyer = true;

                GetExistingSellerLogotype(InvoiceViewModel.SellerId);
            }
        }

        private async void TranslateToLanguage(List<TranslationViewModel> TranslationViewModels)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TranslationViewModel translationViewModel;

                #region HiddenHeaderGrid
                //HiddenHeaderGrid
                //TranslateInvoiceTextBlock.Text "Invoice"
                //TranslateInvoiceNumberTextBlock.Text "Invoice number"
                //TranslateInvoiceDateTextBlock.Text "Invoice date"
                //InvoiceInfoToBuyerTextBlock.Text "Please pay latest at due date"
                //TranslateDueDateTextBlock.Text "Due date"
                //TranslateAmountToPayTextBlock.Text "Amount to pay"
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice");
                if (translationViewModel != null)
                {
                    TranslateInvoiceTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice number");
                if (translationViewModel != null)
                {
                    TranslateInvoiceNumberTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice date");
                if (translationViewModel != null)
                {
                    TranslateInvoiceDateTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Please pay latest at due date");
                if (translationViewModel != null)
                {
                    InvoiceInfoToBuyerTextBlock.Text = translationViewModel.TranslatedText;
                }

                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Due date");
                if (translationViewModel != null)
                {
                    TranslateDueDateTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Amount to pay");
                if (translationViewModel != null)
                {
                    TranslateAmountToPayTextBlock.Text = translationViewModel.TranslatedText;
                }
                #endregion HiddenHeaderGrid

                #region HiddenSellerAccountGrid
                //HiddenSellerAccountGrid
                //TranslatePaymentToAccountTextBlock.Text "Payment to account"
                //TranslateSWIFTBICTextBlock.Text "SWIFT/BIC"
                //TranslateIBANTextBlock.Text "IBAN"
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Payment to account");
                if (translationViewModel != null)
                {
                    TranslatePaymentToAccountTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "SWIFT/BIC");
                if (translationViewModel != null)
                {
                    TranslateSWIFTBICTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "IBAN");
                if (translationViewModel != null)
                {
                    TranslateIBANTextBlock.Text = translationViewModel.TranslatedText;
                }
                #endregion HiddenSellerAccountGrid

                #region HiddenHeaderMoneyGrid
                //HiddenHeaderMoneyGrid
                //TranslateTotalIncludingTaxTextBlock.Text "Total including tax"
                //TranslateTotalExcludingTaxTextBlock.Text "Total excluding tax"
                //TranslateTotalTaxTextBlock.Text "Total tax"
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Total including tax");
                if (translationViewModel != null)
                {
                    TranslateTotalIncludingTaxTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Total excluding tax");
                if (translationViewModel != null)
                {
                    TranslateTotalExcludingTaxTextBlock.Text = translationViewModel.TranslatedText;
                }
                translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Total tax");
                if (translationViewModel != null)
                {
                    TranslateTotalTaxTextBlock.Text = translationViewModel.TranslatedText;
                }
                #endregion HiddenHeaderMoneyGrid
            });
        }

        private void FillInvoiceItemListView()
        {
            if (InvoiceViewModel != null)
            {
                CalculateMoney();

                InvoiceItemListView.DataContext = new NakedHeaderInvoiceItemViewModel();

                InvoiceItemListView.ItemsSource = InvoiceViewModel.InvoiceItemViewModels.OrderBy(o => o.Name).ThenBy(o => o.Quantity).ThenBy(o => o.Tax).ThenBy(o => o.Price);

                InvoiceItemViewModel existingInvoiceItemViewModel = InvoiceViewModel.InvoiceItemViewModels.FirstOrDefault(o => o.InvoiceItemViewModelId == InvoiceItemViewModelId);
                if (existingInvoiceItemViewModel != null)
                {
                    InvoiceItemListView.SelectedItem = existingInvoiceItemViewModel;
                }
            }
        }

        private void CalculateMoney()
        {
            decimal totalExcludingTax = 0m;
            decimal totalTax = 0m;

            try
            {
                foreach (var invoiceItemViewModel in InvoiceViewModel.InvoiceItemViewModels)
                {
                    totalExcludingTax = totalExcludingTax + invoiceItemViewModel.Price * invoiceItemViewModel.Quantity;
                    totalTax = totalTax + invoiceItemViewModel.Price * invoiceItemViewModel.Quantity * invoiceItemViewModel.Tax;
                }

                InvoiceViewModel.TotalIncludingTax = decimal.Round(totalExcludingTax + totalTax, 2);
                InvoiceViewModel.TotalExcludingTax = decimal.Round(totalExcludingTax, 2);
                InvoiceViewModel.TotalTax = decimal.Round(totalTax, 2);
            }
            catch (OverflowException oex)
            {
                MainPage.NotifyUser($"OverflowException. {oex.Message}", NotifyType.ErrorMessage);
            }
        }

        private void FillItemListView()
        {
            ItemListView.DataContext = new NakedHeaderItemViewModel();

            ItemViewModels.Clear();
            foreach (var item in AllItems.OrderBy(o => o.Name).ThenBy(o => o.Tax).ThenBy(o => o.Price))
            {
                ItemViewModel newItemViewModel = ProjectToViewModel.NewItemViewModel(item);
                ItemViewModels.Add(newItemViewModel);
            }
            //ItemListView.ItemsSource = ItemViewModels;

            ItemViewModel itemViewModel = ItemViewModels.FirstOrDefault(o => o.ItemViewModelId == ItemViewModelId);
            if (itemViewModel != null)
            {
                ItemListView.SelectedItem = itemViewModel;
            }
        }

        private void FillBuyerGrid()
        {
            foreach (Buyer buyer in AllBuyers.OrderBy(c => c.BuyerName))
            {
                BuyerViewModels.Add(ProjectToViewModel.NewBuyerViewModel(buyer));
            }

            if (BuyerViewModels.Count == 0)
            {
                BuyerViewModels.Add(ProjectToViewModel.NewBuyerViewModel(new Buyer())); // nameless buyer
            }

            BuyerViewModel buyerViewModel = BuyerViewModels.FirstOrDefault();

            ChooseBuyerComboBox.SelectedItem = buyerViewModel;
        }

        private void FillSellerGrid()
        {
            foreach (Seller seller in AllSellers.OrderBy(s => s.SellerName))
            {
                SellerViewModels.Add(ProjectToViewModel.NewSellerViewModel(seller));
            }

            if (SellerViewModels.Count == 0)
            {
                SellerViewModels.Add(ProjectToViewModel.NewSellerViewModel(new Seller())); // nameless seller
            }

            SellerViewModel sellerViewModel = SellerViewModels.FirstOrDefault();

            GetExistingSellerLogotype(sellerViewModel.SellerViewModelId);

            ChooseSellerComboBox.SelectedItem = sellerViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                string parameter = e.Parameter.ToString();
                string[] parameters = parameter.Split(':');

                PageTitleTextBlock.Text = parameters[0]; // 'New invoice' or 'Edit invoice'
                InvoiceId = Guid.Parse(parameters[1]);
            }
            // code here
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // code here
            LocalSettings.Values["InvoicePageShowInvoiceHeaderGrid"] = InvoiceHeaderGrid.Visibility == Visibility.Visible ? 0 : 1;
            LocalSettings.Values["InvoicePageShowBuyerGrid"] = BuyerGrid.Visibility == Visibility.Visible ? 0 : 1;
            LocalSettings.Values["InvoicePageShowSellerGrid"] = SellerGrid.Visibility == Visibility.Visible ? 0 : 1;

            LocalSettings.Values["InvoicePageShowInvoiceItemListView"] = InvoiceItemListView.Visibility == Visibility.Visible ? 0 : 1;
            LocalSettings.Values["InvoicePageShowItemListView"] = ItemListView.Visibility == Visibility.Visible ? 0 : 1;
            LocalSettings.Values["InvoicePageShowNewItemStackpanel"] = NewItemStackpanel.Visibility == Visibility.Visible ? 0 : 1;

            App.LatestVisitedInvoiceId = InvoiceId;
            // code here
        }

        private void ReadInvoicePageSettings()
        {
            object showInvoiceHeaderGrid = LocalSettings.Values["InvoicePageShowInvoiceHeaderGrid"];
            if (showInvoiceHeaderGrid != null)
            {
                InvoiceHeaderGrid.Visibility = showInvoiceHeaderGrid.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                SetToolTipInvoiceHeaderGrid();
            }
            object showBuyerGrid = LocalSettings.Values["InvoicePageShowBuyerGrid"];
            if (showBuyerGrid != null)
            {
                BuyerGrid.Visibility = showBuyerGrid.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                SetToolTipBuyerGrid();
            }
            object showSellerGrid = LocalSettings.Values["InvoicePageShowSellerGrid"];
            if (showSellerGrid != null)
            {
                SellerGrid.Visibility = showSellerGrid.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                SetToolTipSellerGrid();
            }

            object showInvoiceItemListView = LocalSettings.Values["InvoicePageShowInvoiceItemListView"];
            if (showInvoiceItemListView != null)
            {
                InvoiceItemListView.Visibility = showInvoiceItemListView.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                SetToolTipInvoiceItemListView();
            }
            object showItemListView = LocalSettings.Values["InvoicePageShowItemListView"];
            if (showItemListView != null)
            {
                ItemListView.Visibility = SearchItemStackPanel.Visibility = showItemListView.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                SetToolTipItemListView();
            }
            object showNewItemStackpanel = LocalSettings.Values["InvoicePageShowNewItemStackpanel"];
            if (showNewItemStackpanel != null)
            {
                NewItemStackpanel.Visibility = showNewItemStackpanel.ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                SetToolTipNewItemStackpanel();
            }
        }

        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToHomePage();
            }
        }

        private async void SaveAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (InvoiceViewModel != null)
                {
                    InvoiceId = InvoiceViewModel.InvoiceViewModelId;

                    var existingInvoice = await App.Repository.Invoices.GetInvoiceAsync(InvoiceId).ConfigureAwait(false);
                    if (existingInvoice != null)
                    {
                        await App.Repository.Invoices.DeleteAsync(InvoiceId).ConfigureAwait(false);

                        //MainPage.NotifyUser($"Invoice was deleted.", NotifyType.StatusMessage);
                    }

                    Invoice newInvoice = new Invoice()
                    {
                        InvoiceId = InvoiceViewModel.InvoiceViewModelId,
                        InvoiceNumber = InvoiceViewModel.InvoiceNumber,

                        CreatedAtDateTime = InvoiceViewModel.CreatedAtDateTime,
                        UpdatedAtDateTime = DateTime.Now, // InvoiceViewModel.UpdatedAtDateTime,

                        InvoiceDate = InvoiceViewModel.InvoiceDate,

                        InvoiceInfoToBuyer = InvoiceViewModel.InvoiceInfoToBuyer,

                        TotalIncludingTax = InvoiceViewModel.TotalIncludingTax,
                        TotalExcludingTax = InvoiceViewModel.TotalExcludingTax,
                        TotalTax = InvoiceViewModel.TotalTax,

                        NetPaymentTermDays = InvoiceViewModel.NetPaymentTermDays,
                        NetPaymentDueDate = InvoiceViewModel.NetPaymentDueDate,

                        SellerName = InvoiceViewModel.SellerName,
                        SellerEmail = InvoiceViewModel.SellerEmail,
                        SellerAddress = InvoiceViewModel.SellerAddress,
                        SellerPhonenumber = InvoiceViewModel.SellerPhonenumber,
                        SellerAccount = InvoiceViewModel.SellerAccount,
                        SellerSWIFTBIC = InvoiceViewModel.SellerSWIFTBIC,
                        SellerIBAN = InvoiceViewModel.SellerIBAN,
                        SellerId = InvoiceViewModel.SellerId,

                        BuyerName = InvoiceViewModel.BuyerName,
                        BuyerEmail = InvoiceViewModel.BuyerEmail,
                        BuyerAddress = InvoiceViewModel.BuyerAddress,
                        BuyerPhonenumber = InvoiceViewModel.BuyerPhonenumber,
                        BuyerId = InvoiceViewModel.BuyerId,

                    };

                    foreach (var invoiceItemViewModel in InvoiceViewModel.InvoiceItemViewModels)
                    {
                        newInvoice.InvoiceItems.Add(new InvoiceItem(invoiceItemViewModel.Name, invoiceItemViewModel.InvoiceId)
                        {
                            CreatedAtDateTime = invoiceItemViewModel.CreatedAtDateTime,
                            UpdatedAtDateTime = DateTime.Now, // invoiceItemViewModel.UpdatedAtDateTime,

                            Quantity = invoiceItemViewModel.Quantity,
                            Tax = invoiceItemViewModel.Tax,
                            Price = invoiceItemViewModel.Price,
                        });
                    }

                    #region save seller
                    if (!string.IsNullOrEmpty(InvoiceViewModel.SellerName))
                    {
                        Seller newSeller = new Seller(InvoiceViewModel.SellerName)
                        {
                            SellerEmail = InvoiceViewModel.SellerEmail,
                            SellerAddress = InvoiceViewModel.SellerAddress,
                            SellerPhonenumber = InvoiceViewModel.SellerPhonenumber,
                            SellerAccount = InvoiceViewModel.SellerAccount,
                            SellerSWIFTBIC = InvoiceViewModel.SellerSWIFTBIC,
                            SellerIBAN = InvoiceViewModel.SellerIBAN,
                        };

                        Seller existingSeller = await App.Repository.Sellers.FindExistingSeller(newSeller).ConfigureAwait(false);
                        if (existingSeller == null)
                        {
                            Seller savedSeller = await App.Repository.Sellers.UpsertAsync(newSeller).ConfigureAwait(false);
                            if (savedSeller != null)
                            {
                                newInvoice.SellerId = savedSeller.SellerId;
                                MainPage.NotifyUser("Seller was saved.", NotifyType.StatusMessage);
                            }
                        }
                    }
                    #endregion save seller

                    #region save buyer
                    if (!string.IsNullOrEmpty(InvoiceViewModel.BuyerName))
                    {
                        Buyer newBuyer = new Buyer(InvoiceViewModel.BuyerName)
                        {
                            BuyerEmail = InvoiceViewModel.BuyerEmail,
                            BuyerAddress = InvoiceViewModel.BuyerAddress,
                            BuyerPhonenumber = InvoiceViewModel.BuyerPhonenumber,
                        };

                        Buyer existingBuyer = await App.Repository.Buyers.FindExistingBuyer(newBuyer).ConfigureAwait(false);
                        if (existingBuyer == null)
                        {
                            Buyer savedBuyer = await App.Repository.Buyers.UpsertAsync(newBuyer).ConfigureAwait(false);
                            if (savedBuyer != null)
                            {
                                newInvoice.BuyerId = savedBuyer.BuyerId;
                                MainPage.NotifyUser("Buyer was saved.", NotifyType.StatusMessage);
                            }
                        }
                    }
                    #endregion save buyer

                    Invoice savedInvoice = await App.Repository.Invoices.InsertAsync(newInvoice).ConfigureAwait(false);
                    if (savedInvoice != null)
                    {
                        MainPage.NotifyUser("Invoice was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Invoice was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }

                    MainPage.GoToInvoicesListPage(savedInvoice.InvoiceId);
                }
            }
        }

        private void BackAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToInvoicesListPage(InvoiceId);
            }
        }

        private void SellerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SellerNameSubTitleTextBlock.Text = SellerNameTextBox.Text;
        }

        private void BuyerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BuyerNameSubTitleTextBlock.Text = BuyerNameTextBox.Text;
        }

        private void ChooseSellerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChooseSellerComboBox.SelectedItem != null)
            {
                if (LetComboBoxOverwriteSellerAndBuyer)
                {
                    SellerViewModel sellerViewModel = ChooseSellerComboBox.SelectedItem as SellerViewModel;
                    if (!string.IsNullOrEmpty(sellerViewModel.SellerName))
                    {
                        InvoiceViewModel.SellerName = sellerViewModel.SellerName;
                        InvoiceViewModel.SellerEmail = sellerViewModel.SellerEmail;
                        InvoiceViewModel.SellerAddress = sellerViewModel.SellerAddress;
                        InvoiceViewModel.SellerPhonenumber = sellerViewModel.SellerPhonenumber;
                        InvoiceViewModel.SellerAccount = sellerViewModel.SellerAccount;
                        InvoiceViewModel.SellerSWIFTBIC = sellerViewModel.SellerSWIFTBIC;
                        InvoiceViewModel.SellerIBAN = sellerViewModel.SellerIBAN;
                        InvoiceViewModel.SellerId = sellerViewModel.SellerId;
                    }
                    GetExistingSellerLogotype(sellerViewModel.SellerViewModelId);
                }
            }
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

        private void CancelSellerDataButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InvoiceViewModel.SellerName = string.Empty;
            InvoiceViewModel.SellerEmail = string.Empty;
            InvoiceViewModel.SellerAddress = string.Empty;
            InvoiceViewModel.SellerPhonenumber = string.Empty;
            InvoiceViewModel.SellerAccount = string.Empty;
            InvoiceViewModel.SellerSWIFTBIC = string.Empty;
            InvoiceViewModel.SellerIBAN = string.Empty;

            ChooseSellerComboBox.SelectedItem = null;
        }

        private void ChooseBuyerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChooseBuyerComboBox.SelectedItem != null)
            {
                if (LetComboBoxOverwriteSellerAndBuyer)
                {
                    BuyerViewModel buyerViewModel = ChooseBuyerComboBox.SelectedItem as BuyerViewModel;
                    if (!string.IsNullOrEmpty(buyerViewModel.BuyerName))
                    {
                        InvoiceViewModel.BuyerName = buyerViewModel.BuyerName;
                        InvoiceViewModel.BuyerEmail = buyerViewModel.BuyerEmail;
                        InvoiceViewModel.BuyerAddress = buyerViewModel.BuyerAddress;
                        InvoiceViewModel.BuyerPhonenumber = buyerViewModel.BuyerPhonenumber;
                        InvoiceViewModel.BuyerId = buyerViewModel.BuyerId;
                    }
                }
            }
        }

        private void CancelBuyerDataButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InvoiceViewModel.BuyerName = string.Empty;
            InvoiceViewModel.BuyerEmail = string.Empty;
            InvoiceViewModel.BuyerAddress = string.Empty;
            InvoiceViewModel.BuyerPhonenumber = string.Empty;

            ChooseBuyerComboBox.SelectedItem = null;
        }

        private void NetPaymentTermDaysComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NetPaymentTermDaysComboBox.SelectedItem != null)
            {
                InvoiceStackPanel.DataContext = null;

                InvoiceViewModel.NetPaymentTermDays = (int)NetPaymentTermDaysComboBox.SelectedItem;

                InvoiceViewModel.NetPaymentDueDate = InvoiceViewModel.InvoiceDate.AddDays(InvoiceViewModel.NetPaymentTermDays);

                InvoiceStackPanel.DataContext = InvoiceViewModel;
            }
        }

        private async void InvoiceDateCalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (NetPaymentTermDaysComboBox.SelectedItem != null)
            {
                InvoiceStackPanel.DataContext = null;

                InvoiceViewModel.InvoiceDate = DateTime.Parse(args.NewDate.Value.ToString(CurrentCulture), CurrentCulture);

                if (InvoiceViewModel.InvoiceDate > DateTime.Now)
                {
                    MainPage.NotifyUser("You chose a date later than today's date.", NotifyType.StatusMessage);
                }

                InvoiceViewModel.InvoiceNumber = App.UseSerieAsInvoiceNumber == true ? await HelpInvoiceNumber.GetNewSerieInvoiceNumberAsync().ConfigureAwait(false) : await HelpInvoiceNumber.GetNewDateInvoiceNumberAsync(InvoiceViewModel.InvoiceDate).ConfigureAwait(false);

                InvoiceViewModel.NetPaymentDueDate = InvoiceViewModel.InvoiceDate.AddDays((int)NetPaymentTermDaysComboBox.SelectedItem);

                InvoiceStackPanel.DataContext = InvoiceViewModel;
            }
        }

        private void ShowPrintHeaderPaneAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                ShowPrintHeaderPaneAppBarButton.Visibility = Visibility.Collapsed;
                HiddenPrintHeaderStackPanel.Visibility = Visibility.Visible;
                HidePrintHeaderPaneAppBarButton.Visibility = Visibility.Visible;
            }
        }

        private void HidePrintHeaderPaneAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                HidePrintHeaderPaneAppBarButton.Visibility = Visibility.Collapsed;
                HiddenPrintHeaderStackPanel.Visibility = Visibility.Collapsed;
                ShowPrintHeaderPaneAppBarButton.Visibility = Visibility.Visible;
            }
        }

        private void InvoiceHeaderBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border frame = sender as Border;
            if (frame != null)
            {
                InvoiceHeaderGrid.Visibility = InvoiceHeaderGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                SetToolTipInvoiceHeaderGrid();
            }
        }

        private void BuyerBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border frame = sender as Border;
            if (frame != null)
            {
                BuyerGrid.Visibility = BuyerGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                SetToolTipBuyerGrid();
            }
        }

        private void SellerBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border frame = sender as Border;
            if (frame != null)
            {
                SellerGrid.Visibility = SellerGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                SetToolTipSellerGrid();
            }
        }

        private void SetToolTipInvoiceHeaderGrid()
        {
            switch (InvoiceHeaderGrid.Visibility)
            {
                case Visibility.Visible:
                    ToolTipService.SetToolTip(InvoiceHeaderBorder, "Hide invoice header");
                    InvoiceHeaderBorderTextBlock.Text = "Hide invoice header";
                    break;
                case Visibility.Collapsed:
                    ToolTipService.SetToolTip(InvoiceHeaderBorder, "Show invoice header");
                    InvoiceHeaderBorderTextBlock.Text = "Show invoice header";
                    break;
                default:
                    ToolTipService.SetToolTip(InvoiceHeaderBorder, "Hide invoice header");
                    InvoiceHeaderBorderTextBlock.Text = "Hide invoice header";
                    break;
            }
        }

        private void SetToolTipBuyerGrid()
        {
            switch (BuyerGrid.Visibility)
            {
                case Visibility.Visible:
                    ToolTipService.SetToolTip(BuyerBorder, "Hide buyer");
                    BuyerBorderTextBlock.Text = "Hide buyer";
                    break;
                case Visibility.Collapsed:
                    ToolTipService.SetToolTip(BuyerBorder, "Show buyer");
                    BuyerBorderTextBlock.Text = "Show buyer";
                    break;
                default:
                    ToolTipService.SetToolTip(BuyerBorder, "Hide buyer");
                    BuyerBorderTextBlock.Text = "Hide buyer";
                    break;
            }
        }

        private void SetToolTipSellerGrid()
        {
            switch (SellerGrid.Visibility)
            {
                case Visibility.Visible:
                    ToolTipService.SetToolTip(SellerBorder, "Hide seller");
                    SellerBorderTextBlock.Text = "Hide seller";
                    break;
                case Visibility.Collapsed:
                    ToolTipService.SetToolTip(SellerBorder, "Show seller");
                    SellerBorderTextBlock.Text = "Show seller";
                    break;
                default:
                    ToolTipService.SetToolTip(SellerBorder, "Hide seller");
                    SellerBorderTextBlock.Text = "Hide seller";
                    break;
            }
        }

        private void InvoiceItemsBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border frame = sender as Border;
            if (frame != null)
            {
                InvoiceItemListView.Visibility = InvoiceItemListView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                SetToolTipInvoiceItemListView();
            }
        }

        private void SetToolTipInvoiceItemListView()
        {
            switch (InvoiceItemListView.Visibility)
            {
                case Visibility.Visible:
                    ToolTipService.SetToolTip(InvoiceItemsBorder, "Hide invoice items");
                    InvoiceItemsBorderTextBlock.Text = "Hide invoice items";
                    break;
                case Visibility.Collapsed:
                    ToolTipService.SetToolTip(InvoiceItemsBorder, "Show invoice items");
                    InvoiceItemsBorderTextBlock.Text = "Show invoice items";
                    break;
                default:
                    ToolTipService.SetToolTip(InvoiceItemsBorder, "Hide invoice items");
                    InvoiceItemsBorderTextBlock.Text = "Hide invoice items";
                    break;
            }
        }

        private void ItemsBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border frame = sender as Border;
            if (frame != null)
            {
                ItemListView.Visibility = SearchItemStackPanel.Visibility = ItemListView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                SetToolTipItemListView();
            }
        }

        private void SearchItemTextChanged(object sender, TextChangedEventArgs e)
        {
            string itemSearchText = SearchItemTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(itemSearchText))
            {
                MainPage.NotifyUser("Search string is empty.", NotifyType.StatusMessage);

                if (SearchWasActive)
                {
                    FillItemListView();
                    SearchWasActive = false;
                }

                return;
            }

            string[] parameters = itemSearchText.Split(new char[] { ' ' },
                       StringSplitOptions.RemoveEmptyEntries);
            var matches = AllItems.Where(item => parameters
               .Any(parameter =>
                     item.Name.Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (item.Tax.ToString("p2", CurrentCulture) != null && item.Tax.ToString("p2", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (item.Price.ToString("c", CurrentCulture) != null && item.Price.ToString("c", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase))))
                 .OrderByDescending(item => parameters.Count(parameter =>
                     item.Name.Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (item.Tax.ToString("p2", CurrentCulture) != null && item.Tax.ToString("p2", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (item.Price.ToString("c", CurrentCulture) != null ? item.Price.ToString("c", CurrentCulture).Contains(parameter, StringComparison.OrdinalIgnoreCase) : false)))
                 .ToList();

            if (matches.Count == 0)
            {
                MainPage.NotifyUser("No matches found. Try again.", NotifyType.StatusMessage);
                return;
            }

            SearchWasActive = true;

            ItemListView.ItemsSource = null;
            ItemListView.DataContext = new HeaderItemViewModel();
            ItemViewModels.Clear();
            foreach (Item item in matches
                                    .OrderBy(v => v.Name)
                                    .ThenBy(v => v.Tax)
                                    .ThenBy(v => v.Price))
            {
                ItemViewModels.Add(ProjectToViewModel.NewItemViewModel(item));
            }
            ItemListView.ItemsSource = ItemViewModels;

            ItemViewModel itemListViewSelectedItem = ItemViewModels.FirstOrDefault(o => o.ItemViewModelId == ItemId);
            if (itemListViewSelectedItem != null)
            {
                ItemListView.SelectedItem = itemListViewSelectedItem;
            }
        }

        private async void CancelSearchItemButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchItemTextBox.Text))
            {
                MainPage.NotifyUser("Nothing to cancel.", NotifyType.StatusMessage);
            }

            SearchItemTextBox.Text = string.Empty;

            if (SearchWasActive)
            {
                AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);
                FillItemListView();
                SearchWasActive = false;
            }
        }


        private void SetToolTipItemListView()
        {
            switch (ItemListView.Visibility)
            {
                case Visibility.Visible:
                    ToolTipService.SetToolTip(ItemsBorder, "Hide items");
                    ItemsBorderTextBlock.Text = "Hide items";
                    break;
                case Visibility.Collapsed:
                    ToolTipService.SetToolTip(ItemsBorder, "Show items");
                    ItemsBorderTextBlock.Text = "Show items";
                    break;
                default:
                    ToolTipService.SetToolTip(ItemsBorder, "Hide items");
                    ItemsBorderTextBlock.Text = "Hide items";
                    break;
            }
        }

        private void NewItemBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Border frame = sender as Border;
            if (frame != null)
            {
                NewItemStackpanel.Visibility = NewItemStackpanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                SetToolTipNewItemStackpanel();
            }
        }

        private void SetToolTipNewItemStackpanel()
        {
            switch (NewItemStackpanel.Visibility)
            {
                case Visibility.Visible:
                    ToolTipService.SetToolTip(NewItemBorder, "Hide new item");
                    NewItemBorderTextBlock.Text = "Hide new item";
                    break;
                case Visibility.Collapsed:
                    ToolTipService.SetToolTip(NewItemBorder, "Show new item");
                    NewItemBorderTextBlock.Text = "Show new item";
                    break;
                default:
                    ToolTipService.SetToolTip(NewItemBorder, "Hide new item");
                    NewItemBorderTextBlock.Text = "Hide new item";
                    break;
            }
        }

        private void RemoveInvoiceItemButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var invoiceItemViewModelId = Guid.Parse(button.Tag.ToString());

                InvoiceItemViewModel invoiceItemViewModel = InvoiceViewModel.InvoiceItemViewModels.FirstOrDefault(o => o.InvoiceItemViewModelId == invoiceItemViewModelId);
                if (invoiceItemViewModel != null)
                {
                    InvoiceViewModel.InvoiceItemViewModels.Remove(invoiceItemViewModel);

                    FillInvoiceItemListView();

                    ItemListView.SelectedItem = null;

                    ItemViewModelId = Guid.NewGuid();

                    MainPage.NotifyUser("Invoice item was deleted.", NotifyType.StatusMessage);

                    InvoiceItemViewModel existingInvoiceItemViewModel = InvoiceItemListView.Items.FirstOrDefault() as InvoiceItemViewModel;
                    if (existingInvoiceItemViewModel != null)
                    {
                        InvoiceItemListView.SelectedItem = existingInvoiceItemViewModel;
                    }
                }
                else
                {
                    MainPage.NotifyUser("Select an invoice item first.", NotifyType.StatusMessage);
                }
            }
        }

        private void InvoiceItemListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (InvoiceItemListView.SelectedItem != null)
            {
                e.Handled = true;

                InvoiceItemViewModel invoiceItemViewModel = InvoiceItemListView.SelectedItem as InvoiceItemViewModel;

                NewItemGrid.DataContext = null;

                NewItemViewModel.Name = invoiceItemViewModel.Name;
                NewItemViewModel.BigTax = invoiceItemViewModel.Tax * 100;
                NewItemViewModel.Tax = invoiceItemViewModel.Tax;
                NewItemViewModel.Price = invoiceItemViewModel.Price;

                NewItemGrid.DataContext = NewItemViewModel;

                if (NewItemStackpanel.Visibility == Visibility.Collapsed)
                {
                    NewItemStackpanel.Visibility = Visibility.Visible;
                }

                MainPage.NotifyUser("Invoice item is present in 'New item'.", NotifyType.StatusMessage);
            }
        }

        private void ItemListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemListView.SelectedItem != null)
            {
                e.Handled = true;

                ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;

                NewItemGrid.DataContext = null;

                NewItemViewModel.Name = itemViewModel.Name;
                NewItemViewModel.BigTax = itemViewModel.Tax * 100;
                NewItemViewModel.Tax = itemViewModel.Tax;
                NewItemViewModel.Price = itemViewModel.Price;

                NewItemGrid.DataContext = NewItemViewModel;

                if (NewItemStackpanel.Visibility == Visibility.Collapsed)
                {
                    NewItemStackpanel.Visibility = Visibility.Visible;
                }

                MainPage.NotifyUser("Item is present in 'New item'.", NotifyType.StatusMessage);
            }
        }

        private void ItemListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ItemListView.SelectedItem != null)
            {
                ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;

                if (InvoiceViewModel != null)
                {
                    e.Handled = true;

                    InvoiceId = InvoiceViewModel.InvoiceViewModelId;

                    if (itemViewModel != null)
                    {
                        InvoiceItem newInvoiceItem = new InvoiceItem(itemViewModel.Name, InvoiceId)
                        {
                            Tax = itemViewModel.Tax,
                            Price = itemViewModel.Price
                        };

                        InvoiceItemViewModel invoiceItemViewModel = ProjectToViewModel.NewInvoiceItemViewModel(newInvoiceItem);

                        InvoiceItemViewModelId = invoiceItemViewModel.InvoiceItemViewModelId;

                        InvoiceViewModel.InvoiceItemViewModels.Add(invoiceItemViewModel);

                        if (InvoiceItemListView.Visibility == Visibility.Collapsed)
                        {
                            InvoiceItemListView.Visibility = Visibility.Visible;
                        }

                        FillInvoiceItemListView();

                        ItemListView.SelectedItem = itemViewModel;

                        MainPage.NotifyUser("Item was added to invoice items.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private void AddItemToInvoiceItemsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ItemId = Guid.Parse(button.Tag.ToString());
                ItemViewModel itemViewModel = ItemViewModels.FirstOrDefault(o => o.ItemViewModelId == ItemId);

                if (InvoiceViewModel != null)
                {
                    e.Handled = true;

                    InvoiceId = InvoiceViewModel.InvoiceViewModelId;

                    if (itemViewModel != null)
                    {
                        InvoiceItem newInvoiceItem = new InvoiceItem(itemViewModel.Name, InvoiceId)
                        {
                            Tax = itemViewModel.Tax,
                            Price = itemViewModel.Price
                        };

                        InvoiceItemViewModel invoiceItemViewModel = ProjectToViewModel.NewInvoiceItemViewModel(newInvoiceItem);

                        InvoiceItemViewModelId = invoiceItemViewModel.InvoiceItemViewModelId;

                        InvoiceViewModel.InvoiceItemViewModels.Add(invoiceItemViewModel);

                        if (InvoiceItemListView.Visibility == Visibility.Collapsed)
                        {
                            InvoiceItemListView.Visibility = Visibility.Visible;
                        }

                        FillInvoiceItemListView();

                        ItemListView.SelectedItem = itemViewModel;

                        MainPage.NotifyUser("Item was added to invoice items.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private async void AddItemToItemsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewItemViewModel.Name))
            {
                MainPage.NotifyUser("Item name is required.", NotifyType.StatusMessage);
                return;
            }

            Item newItem = new Item(NewItemViewModel.Name)
            {
                Tax = NewItemViewModel.Tax,
                Price = NewItemViewModel.Price,
            };

            var existingItem = await App.Repository.Items.UpsertAsync(newItem).ConfigureAwait(false);
            if (existingItem != null)
            {
                ItemViewModelId = existingItem.ItemId;

                AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);

                //if (ItemListView.Visibility == Visibility.Collapsed)
                //{
                //    ItemListView.Visibility = Visibility.Visible;
                //}

                FillItemListView();

                MainPage.NotifyUser("Item was added to items.", NotifyType.StatusMessage);
            }
            else
            {
                MainPage.NotifyUser("Item was not created. Something went wrong. Try again.", NotifyType.StatusMessage);
            }

            if (existingItem != null)
            {
                ItemId = existingItem.ItemId;
                ItemViewModel itemViewModel = ItemViewModels.FirstOrDefault(o => o.ItemViewModelId == ItemId);

                if (InvoiceViewModel != null)
                {
                    InvoiceId = InvoiceViewModel.InvoiceViewModelId;

                    if (itemViewModel != null)
                    {
                        InvoiceItem newInvoiceItem = new InvoiceItem(itemViewModel.Name, InvoiceId)
                        {
                            Tax = itemViewModel.Tax,
                            Price = itemViewModel.Price
                        };

                        InvoiceItemViewModel invoiceItemViewModel = ProjectToViewModel.NewInvoiceItemViewModel(newInvoiceItem);

                        InvoiceItemViewModelId = invoiceItemViewModel.InvoiceItemViewModelId;

                        InvoiceViewModel.InvoiceItemViewModels.Add(invoiceItemViewModel);

                        if (InvoiceItemListView.Visibility == Visibility.Collapsed)
                        {
                            InvoiceItemListView.Visibility = Visibility.Visible;
                        }

                        FillInvoiceItemListView();

                        ItemListView.SelectedItem = itemViewModel;

                        MainPage.NotifyUser("Item was added to invoice items.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private async void RemoveItemFromItemsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemListView.SelectedItem != null)
            {
                ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;

                await App.Repository.Items.DeleteAsync(itemViewModel.ItemViewModelId).ConfigureAwait(false);

                AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);

                if (ItemListView.Visibility == Visibility.Collapsed)
                {
                    ItemListView.Visibility = SearchItemStackPanel.Visibility = Visibility.Visible;
                }

                FillItemListView();

                MainPage.NotifyUser("Item was deleted.", NotifyType.StatusMessage);
            }
            else
            {
                MainPage.NotifyUser("Select an item first.", NotifyType.StatusMessage);
            }
        }

        private void SearchBuyerTextChanged(object sender, TextChangedEventArgs e)
        {
            string buyerSearchText = SearchBuyerTextBox.Text.Trim();
            if (string.IsNullOrEmpty(buyerSearchText))
            {
                MainPage.NotifyUser("Search string is empty.", NotifyType.StatusMessage);

                if (BuyerSearchWasActive)
                {
                    FillBuyerGrid();
                    BuyerSearchWasActive = false;
                }

                return;
            }

            string[] parameters = buyerSearchText.Split(new char[] { ' ' },
                       StringSplitOptions.RemoveEmptyEntries);
            var matches = AllBuyers.Where(buyer => parameters
                 .Any(parameter =>
                     buyer.BuyerName.Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (buyer.BuyerEmail != null && buyer.BuyerEmail.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (buyer.BuyerPhonenumber != null && buyer.BuyerPhonenumber.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (buyer.BuyerAddress != null && buyer.BuyerAddress.Contains(parameter, StringComparison.OrdinalIgnoreCase))))
                 .OrderByDescending(buyer => parameters.Count(parameter =>
                     buyer.BuyerName.Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (buyer.BuyerEmail != null && buyer.BuyerEmail.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (buyer.BuyerPhonenumber != null && buyer.BuyerPhonenumber.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (buyer.BuyerAddress != null && buyer.BuyerAddress.Contains(parameter, StringComparison.OrdinalIgnoreCase))))
                 .ToList();

            if (matches.Count == 0)
            {
                MainPage.NotifyUser("No matches found. Try again.", NotifyType.StatusMessage);

                return;
            }

            BuyerSearchWasActive = true;

            BuyerViewModels.Clear();
            foreach (Buyer buyer in matches)
            {
                BuyerViewModels.Add(ProjectToViewModel.NewBuyerViewModel(buyer));
            }
        }

        private async void CancelSearchBuyerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBuyerTextBox.Text))
            {
                MainPage.NotifyUser("Nothing to cancel.", NotifyType.StatusMessage);
            }
            SearchBuyerTextBox.Text = string.Empty;

            if (BuyerSearchWasActive)
            {
                AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);
                FillBuyerGrid();
                BuyerSearchWasActive = false;
            }
        }

        private void SearchSellerTextChanged(object sender, TextChangedEventArgs e)
        {
            string sellerSearchText = SearchSellerTextBox.Text.Trim();
            if (string.IsNullOrEmpty(sellerSearchText))
            {
                MainPage.NotifyUser("Search string is empty.", NotifyType.StatusMessage);

                if (SellerSearchWasActive)
                {
                    FillSellerGrid();
                    SellerSearchWasActive = false;
                }

                return;
            }

            string[] parameters = sellerSearchText.Split(new char[] { ' ' },
                       StringSplitOptions.RemoveEmptyEntries);
            var matches = AllSellers.Where(seller => parameters
                 .Any(parameter =>
                     seller.SellerName.Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (seller.SellerEmail != null && seller.SellerEmail.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerPhonenumber != null && seller.SellerPhonenumber.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerAddress != null && seller.SellerAddress.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerSWIFTBIC != null && seller.SellerSWIFTBIC.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerIBAN != null && seller.SellerIBAN.Contains(parameter, StringComparison.OrdinalIgnoreCase))))
                 .OrderByDescending(seller => parameters.Count(parameter =>
                     seller.SellerName.Contains(parameter, StringComparison.OrdinalIgnoreCase) ||
                     (seller.SellerEmail != null && seller.SellerEmail.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerPhonenumber != null && seller.SellerPhonenumber.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerAddress != null && seller.SellerAddress.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerSWIFTBIC != null && seller.SellerSWIFTBIC.Contains(parameter, StringComparison.OrdinalIgnoreCase)) ||
                     (seller.SellerIBAN != null && seller.SellerIBAN.Contains(parameter, StringComparison.OrdinalIgnoreCase))))
                 .ToList();

            if (matches.Count == 0)
            {
                MainPage.NotifyUser("No matches found. Try again.", NotifyType.StatusMessage);
                return;
            }

            SellerSearchWasActive = true;

            SellerViewModels.Clear();
            foreach (Seller seller in matches)
            {
                SellerViewModels.Add(ProjectToViewModel.NewSellerViewModel(seller));
            }
        }

        private async void CancelSearchSellerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchSellerTextBox.Text))
            {
                MainPage.NotifyUser("Nothing to cancel.", NotifyType.StatusMessage);
            }

            SearchSellerTextBox.Text = string.Empty;

            if (SellerSearchWasActive)
            {
                AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);
                FillSellerGrid();
                SellerSearchWasActive = false;
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Guid invoiceItemViewModelId = Guid.Parse(textBox.Tag.ToString());

                InvoiceItemViewModel invoiceItemViewModel = InvoiceViewModel.InvoiceItemViewModels.FirstOrDefault(o => o.InvoiceItemViewModelId == invoiceItemViewModelId);
                if (invoiceItemViewModel != null)
                {
                    decimal value = invoiceItemViewModel.Quantity;
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.Text = string.Format(CultureInfo.CurrentCulture, "{0:n2}", value);
                    }
                    else
                    {
                        if (decimal.TryParse(textBox.Text, out value))
                        {
                            invoiceItemViewModel.Quantity = Math.Round(value, 2);
                            CalculateMoney();
                        }
                        else
                        {
                            MainPage.NotifyUser("Quantity is required.", NotifyType.ErrorMessage);
                        }
                    }
                }
            }
        }

        private void TaxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                decimal value = NewItemViewModel.BigTax;
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = string.Format(CultureInfo.CurrentCulture, "{0:n2}", value);
                }
                else
                {
                    decimal taxValue;
                    if (decimal.TryParse(TaxTextBox.Text, out taxValue))
                    {
                        if (taxValue < 0 || taxValue > 100)
                        {
                            MainPage.NotifyUser("Tax is required in range of 0 to 100.", NotifyType.ErrorMessage);
                            return;
                        }
                        taxValue = Math.Round(taxValue, 2);
                        NewItemViewModel.BigTax = taxValue;
                        NewItemViewModel.Tax = Math.Round(taxValue / 100, 4);
                    }
                    else
                    {
                        MainPage.NotifyUser("Tax is required.", NotifyType.ErrorMessage);
                        return;
                    }
                }
            }
        }

        private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                decimal value = NewItemViewModel.Price;
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = string.Format(CultureInfo.CurrentCulture, "{0:n2}", value);
                }
                else
                {
                    decimal priceValue;
                    if (decimal.TryParse(PriceTextBox.Text, out priceValue))
                    {
                        if (priceValue < 0)
                        {
                            MainPage.NotifyUser("Price is required. Positive values only.", NotifyType.ErrorMessage);
                            return;
                        }
                        NewItemViewModel.Price = Math.Round(priceValue, 2);
                    }
                    else
                    {
                        MainPage.NotifyUser("Price is required.", NotifyType.ErrorMessage);
                        return;
                    }
                }
            }
        }
    }
}
