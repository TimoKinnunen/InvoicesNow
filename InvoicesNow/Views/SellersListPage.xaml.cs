using InvoicesNow.Helpers;
using InvoicesNow.Models;
using InvoicesNow.Projections;
using InvoicesNow.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class SellersListPage : Page
    {
        Guid SellerId { get; set; }

        MainPage MainPage { get; }

        ObservableCollection<SellerViewModel> SellerViewModels { get; set; } = new ObservableCollection<SellerViewModel>();

        IEnumerable<Seller> AllSellers { get; set; }

        HeaderSellerViewModel HeaderSellerViewModel { get; set; } = new HeaderSellerViewModel();

        bool SearchWasActive { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public SellersListPage()
        {
            InitializeComponent();

            Loaded += SellersListPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void SellersListPage_Loaded(object sender, RoutedEventArgs e)
        {
            AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);

            FillSellerListView();
        }

        private void FillSellerListView()
        {
            ExportDataAppBarButton.IsEnabled = AllSellers.Count() > 0 ? true : false;

            SellerListView.DataContext = new HeaderSellerViewModel();

            SellerViewModels.Clear();
            foreach (Seller seller in AllSellers
                                    .OrderBy(v => v.SellerName)
                                    .ThenBy(v => v.SellerEmail)
                                    .ThenBy(v => v.SellerAddress)
                                    .ThenBy(v => v.SellerPhonenumber)
                                    .ThenBy(v => v.SellerAccount)
                                    .ThenBy(v => v.SellerIBAN)
                                    .ThenBy(v => v.SellerSWIFTBIC))
            {
                SellerViewModel sellerViewModelToAdd = ProjectToViewModel.NewSellerViewModel(seller);
                SellerViewModels.Add(sellerViewModelToAdd);
            }

            SellerViewModel sellerListViewSelectedItem = SellerViewModels.FirstOrDefault(o => o.SellerViewModelId == SellerId);
            if (sellerListViewSelectedItem != null)
            {
                SellerListView.SelectedItem = sellerListViewSelectedItem;
            }
            else
            {
                var first = SellerViewModels.FirstOrDefault();
                if (first != null)
                {
                    SellerListView.SelectedItem = first;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                SellerId = Guid.Parse(e.Parameter.ToString());
            }
            // code here
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // code here
            // code here
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
            if (SellerListView.SelectedItem != null)
            {
                SellerViewModel sellerViewModel = SellerListView.SelectedItem as SellerViewModel;
                SellerId = sellerViewModel.SellerViewModelId;
                MainPage.GoToSellerPage("Edit seller", SellerId);
            }
        }

        private void SellerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SellerListView.SelectedItem == null)
            {
                EditAppBarButton.IsEnabled = false;
                DeleteAppBarButton.IsEnabled = false;
                CopyAppBarButton.IsEnabled = false;
                LogotypeAppBarButton.IsEnabled = false;
            }
            else
            {
                EditAppBarButton.IsEnabled = true;
                DeleteAppBarButton.IsEnabled = true;
                CopyAppBarButton.IsEnabled = true;
                LogotypeAppBarButton.IsEnabled = true;
            }
        }

        private async void DeleteAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (SellerListView.SelectedItem != null)
                {
                    SellerViewModel sellerViewModel = SellerListView.SelectedItem as SellerViewModel;

                    SellerId = sellerViewModel.SellerViewModelId; // this will be deleted

                    await App.Repository.Sellers.DeleteAsync(SellerId).ConfigureAwait(false);

                    MainPage.NotifyUser($"Item was deleted.", NotifyType.StatusMessage);

                    AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);

                    FillSellerListView();

                    DeleteSellerLogotype(SellerId);
                }
            }
        }

        private async void DeleteSellerLogotype(Guid sellerId)
        {
            StorageFolder logotypesStorageFolder = await GetLogotypesStorageFolder();
            IReadOnlyList<StorageFile> fileList = await logotypesStorageFolder.GetFilesAsync();
            StorageFile existingLogotype = fileList.FirstOrDefault(o => o.DisplayName.ToUpper() == SellerId.ToString().ToUpper());
            if (existingLogotype != null)
            {
                await existingLogotype.DeleteAsync();
                MainPage.NotifyUser($"Seller's logotype was deleted.", NotifyType.StatusMessage);
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

        private void SellerListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is ListView)
            {
                if (SellerListView.SelectedItem != null)
                {
                    SellerViewModel sellerViewModel = SellerListView.SelectedItem as SellerViewModel;
                    SellerId = sellerViewModel.SellerViewModelId;
                    MainPage.GoToSellerPage("Edit seller", SellerId);
                }
            }
        }

        private void NewAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToSellerPage("New seller", Guid.Empty);
            }
        }

        private async void CopyAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (SellerListView.SelectedItem != null)
                {
                    SellerViewModel sellerViewModel = SellerListView.SelectedItem as SellerViewModel;

                    SellerId = sellerViewModel.SellerViewModelId;

                    Seller copiedSeller = new Seller
                    {
                        SellerName = sellerViewModel.SellerName,
                        SellerEmail = sellerViewModel.SellerEmail,
                        SellerAddress = sellerViewModel.SellerAddress,
                        SellerPhonenumber = sellerViewModel.SellerPhonenumber,
                        SellerAccount = sellerViewModel.SellerAccount,
                        SellerSWIFTBIC = sellerViewModel.SellerSWIFTBIC,
                        SellerIBAN = sellerViewModel.SellerIBAN
                    };

                    Seller savedSeller = await App.Repository.Sellers.UpsertAsync(copiedSeller).ConfigureAwait(false);

                    if (savedSeller != null)
                    {
                        SellerId = savedSeller.SellerId;

                        AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);

                        FillSellerListView();

                        MainPage.NotifyUser($"Seller was copied. Logotype was not copied.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser($"Seller was not copied. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private void LogotypeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (SellerListView.SelectedItem != null)
                {
                    SellerViewModel sellerViewModel = SellerListView.SelectedItem as SellerViewModel;
                    SellerId = sellerViewModel.SellerViewModelId;
                    MainPage.GoToSellerLogotypePage(SellerId);
                }
            }
        }
        #region sort table header
        bool sortAscendingByName;
        private void TableHeaderSellerNameTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByName)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name ↓";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name ↑";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingByName == true ? SellerViewModels.OrderBy(v => v.SellerName) : SellerViewModels.OrderByDescending(v => v.SellerName);
                sortAscendingByName = !sortAscendingByName;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByEmail;
        private void TableHeaderEmailTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByEmail)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address ↓";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address ↑";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingByEmail == true ? SellerViewModels.OrderBy(v => v.SellerEmail) : SellerViewModels.OrderByDescending(v => v.SellerEmail);
                sortAscendingByEmail = !sortAscendingByEmail;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByAddress;
        private void TableHeaderAddressTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByAddress)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address ↓";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address ↑";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingByAddress == true ? SellerViewModels.OrderBy(v => v.SellerAddress) : SellerViewModels.OrderByDescending(v => v.SellerAddress);
                sortAscendingByAddress = !sortAscendingByAddress;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByPhonenumber;
        private void TableHeaderPhonenumberTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByPhonenumber)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber ↓";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber ↑";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingByPhonenumber == true ? SellerViewModels.OrderBy(v => v.SellerPhonenumber) : SellerViewModels.OrderByDescending(v => v.SellerPhonenumber);
                sortAscendingByPhonenumber = !sortAscendingByPhonenumber;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByAccount;
        private void TableHeaderAccountTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByAccount)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account ↓";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account ↑";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingByAccount == true ? SellerViewModels.OrderBy(v => v.SellerAccount) : SellerViewModels.OrderByDescending(v => v.SellerAccount);
                sortAscendingByAccount = !sortAscendingByAccount;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingBySWIFTBIC;
        private void TableHeaderSWIFTBICTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingBySWIFTBIC)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC ↓";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC ↑";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingBySWIFTBIC == true ? SellerViewModels.OrderBy(v => v.SellerSWIFTBIC) : SellerViewModels.OrderByDescending(v => v.SellerSWIFTBIC);
                sortAscendingBySWIFTBIC = !sortAscendingBySWIFTBIC;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByIBAN;
        private void TableHeaderIBANTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = SellerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByIBAN)
                {
                    case true:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN ↓";
                        break;
                    default:
                        HeaderSellerViewModel.HeaderSellerName = "Name";
                        HeaderSellerViewModel.HeaderEmail = "E-mail address";
                        HeaderSellerViewModel.HeaderAddress = "Address";
                        HeaderSellerViewModel.HeaderPhonenumber = "Phonenumber";
                        HeaderSellerViewModel.HeaderAccount = "Account";
                        HeaderSellerViewModel.HeaderSWIFTBIC = "SWIFT/BIC";
                        HeaderSellerViewModel.HeaderIBAN = "IBAN ↑";
                        break;
                }
                SellerListView.DataContext = HeaderSellerViewModel;
                SellerListView.ItemsSource = sortAscendingByIBAN == true ? SellerViewModels.OrderBy(v => v.SellerIBAN) : SellerViewModels.OrderByDescending(v => v.SellerIBAN);
                sortAscendingByIBAN = !sortAscendingByIBAN;
            }
            if (selectedItem != null)
            {
                SellerListView.SelectedItem = selectedItem;
            }
        }
        #endregion sort table header

        private void SearchSellerTextChanged(object sender, TextChangedEventArgs e)
        {
            string sellerSearchText = SearchSellerTextBox.Text.Trim();
            if (string.IsNullOrEmpty(sellerSearchText))
            {
                MainPage.NotifyUser("Search string is empty.", NotifyType.StatusMessage);

                if (SearchWasActive)
                {
                    FillSellerListView();
                    SearchWasActive = false;
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

            SearchWasActive = true;

            SellerListView.ItemsSource = null;
            SellerListView.DataContext = new HeaderSellerViewModel();
            SellerViewModels.Clear();
            foreach (Seller seller in matches
                                    .OrderBy(v => v.SellerName)
                                    .ThenBy(v => v.SellerEmail)
                                    .ThenBy(v => v.SellerAddress)
                                    .ThenBy(v => v.SellerPhonenumber)
                                    .ThenBy(v => v.SellerAccount)
                                    .ThenBy(v => v.SellerIBAN)
                                    .ThenBy(v => v.SellerSWIFTBIC))
            {
                SellerViewModels.Add(ProjectToViewModel.NewSellerViewModel(seller));
            }
            SellerListView.ItemsSource = SellerViewModels;

            SellerViewModel sellerListViewSelectedItem = SellerViewModels.FirstOrDefault(o => o.SellerViewModelId == SellerId);
            if (sellerListViewSelectedItem != null)
            {
                SellerListView.SelectedItem = sellerListViewSelectedItem;
            }
        }

        private async void CancelSearchSellerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchSellerTextBox.Text))
            {
                MainPage.NotifyUser("Nothing to cancel.", NotifyType.StatusMessage);
            }

            SearchSellerTextBox.Text = string.Empty;

            if (SearchWasActive)
            {
                AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);
                FillSellerListView();
                SearchWasActive = false;
            }
        }

        private async void ExportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                string suggestedFileName = $"{HelpFileName.AddDateTimeNowToFileName("InvoicesNow_Sellers")}.json";
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

                        EditAppBarButton.IsEnabled = false;
                        DeleteAppBarButton.IsEnabled = false;
                        NewAppBarButton.IsEnabled = false;
                        CopyAppBarButton.IsEnabled = false;
                    });

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainPage.NotifyUser("Export data. Please wait.", NotifyType.StatusMessage);
                    });

                    IEnumerable<Seller> allSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(false);

                    List<DTOSeller> allDTOSellers = new List<DTOSeller>();
                    foreach (var seller in allSellers)
                    {
                        allDTOSellers.Add(new DTOSeller()
                        {
                            SellerId = seller.SellerId,
                            SellerName = seller.SellerName,

                            CreatedAtDateTime = seller.CreatedAtDateTime,
                            UpdatedAtDateTime = seller.UpdatedAtDateTime,

                            SellerEmail = seller.SellerEmail,
                            SellerAddress = seller.SellerAddress,
                            SellerPhonenumber = seller.SellerPhonenumber,
                            SellerAccount = seller.SellerAccount,
                            SellerSWIFTBIC = seller.SellerSWIFTBIC,
                            SellerIBAN = seller.SellerIBAN,
                        });
                    }

                    string jsonData = await Task.Run(() => JsonConvert.SerializeObject(allDTOSellers)).ConfigureAwait(false);

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

                        if (SellerListView.SelectedItem != null)
                        {
                            EditAppBarButton.IsEnabled = true;
                            DeleteAppBarButton.IsEnabled = true;
                            CopyAppBarButton.IsEnabled = true;
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
                    int importedDTOSellerCount = 0;
                    int existingSellerCount = 0;

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ExportDataAppBarButton.IsEnabled = false;
                        ImportDataAppBarButton.IsEnabled = false;
                        ImportDataProgressRing.Visibility = Visibility.Visible;
                        ImportDataProgressRing.IsEnabled = true;
                        ImportDataProgressRing.IsActive = true;

                        EditAppBarButton.IsEnabled = false;
                        DeleteAppBarButton.IsEnabled = false;
                        NewAppBarButton.IsEnabled = false;
                        CopyAppBarButton.IsEnabled = false;
                    });

                    try
                    {
                        string fileContent = await FileIO.ReadTextAsync(storageFile);
                        List<DTOSeller> ImportedDTOSellers = JsonConvert.DeserializeObject<List<DTOSeller>>(fileContent);
                        foreach (DTOSeller ImportedDTOSeller in ImportedDTOSellers)
                        {
                            Seller newImportedSeller = new Seller(ImportedDTOSeller);

                            if (!newImportedSeller.SellerId.Equals(Guid.Empty))
                            {
                                Seller existingSeller = await App.Repository.Sellers.GetSellerAsync(newImportedSeller.SellerId).ConfigureAwait(false);
                                if (existingSeller != null)
                                {
                                    if (ImportedDTOSeller.UpdatedAtDateTime > existingSeller.UpdatedAtDateTime)
                                    {
                                        Seller savedSeller = await App.Repository.Sellers.UpsertAsync(newImportedSeller).ConfigureAwait(false);
                                        if (savedSeller != null)
                                        {
                                            importedDTOSellerCount++;
                                        }
                                        else
                                        {
                                            throw new ArgumentNullException("Failure when importing seller.");
                                        }
                                    }
                                    else
                                    {
                                        existingSellerCount++;
                                    }
                                }
                                else
                                {
                                    Seller savedSeller = await App.Repository.Sellers.UpsertAsync(newImportedSeller).ConfigureAwait(false);
                                    if (savedSeller != null)
                                    {
                                        importedDTOSellerCount++;
                                    }
                                }
                            }
                        }

                        if (importedDTOSellerCount > 0)
                        {
                            AllSellers = await App.Repository.Sellers.GetAllSellersAsync().ConfigureAwait(true);

                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                FillSellerListView();

                                MainPage.NotifyUser($"Imported {importedDTOSellerCount}({importedDTOSellerCount + existingSellerCount}) {(importedDTOSellerCount == 1 ? "seller" : "sellers")}.", NotifyType.StatusMessage);
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (existingSellerCount == 0)
                                {
                                    MainPage.NotifyUser($"File didn't contain sellers. Try again.", NotifyType.ErrorMessage);
                                }
                                else
                                {
                                    MainPage.NotifyUser($"Didn't import {existingSellerCount}({importedDTOSellerCount + existingSellerCount}) {(existingSellerCount == 1 ? "duplicate seller" : "duplicate sellers")}.", NotifyType.StatusMessage);
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

                            if (SellerListView.SelectedItem != null)
                            {
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
