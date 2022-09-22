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
    public sealed partial class BuyersListPage : Page
    {
        Guid BuyerId { get; set; }

        MainPage MainPage { get; }

        ObservableCollection<BuyerViewModel> BuyerViewModels { get; set; } = new ObservableCollection<BuyerViewModel>();

        IEnumerable<Buyer> AllBuyers { get; set; }

        HeaderBuyerViewModel HeaderBuyerViewModel { get; set; } = new HeaderBuyerViewModel();

        bool SearchWasActive { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public BuyersListPage()
        {
            InitializeComponent();

            Loaded += BuyerListPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void BuyerListPage_Loaded(object sender, RoutedEventArgs e)
        {
            AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);

            FillBuyerListView();
        }

        private void FillBuyerListView()
        {
            ExportDataAppBarButton.IsEnabled = AllBuyers.Count() > 0 ? true : false;

            BuyerListView.DataContext = new HeaderBuyerViewModel();

            BuyerViewModels.Clear();
            foreach (Buyer buyer in AllBuyers
                                    .OrderBy(v => v.BuyerName)
                                    .ThenBy(v => v.BuyerEmail)
                                    .ThenBy(v => v.BuyerAddress)
                                    .ThenBy(v => v.BuyerPhonenumber))
            {
                BuyerViewModel buyerViewModelToAdd = ProjectToViewModel.NewBuyerViewModel(buyer);
                BuyerViewModels.Add(buyerViewModelToAdd);
            }

            BuyerViewModel buyerListViewSelectedItem = BuyerViewModels.FirstOrDefault(o => o.BuyerViewModelId == BuyerId);
            if (buyerListViewSelectedItem != null)
            {
                BuyerListView.SelectedItem = buyerListViewSelectedItem;
            }
            else
            {
                var first = BuyerViewModels.FirstOrDefault();
                if (first != null)
                {
                    BuyerListView.SelectedItem = first;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                BuyerId = Guid.Parse(e.Parameter.ToString());
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
            if (sender is AppBarButton)
            {
                if (BuyerListView.SelectedItem != null)
                {
                    BuyerViewModel buyerViewModel = BuyerListView.SelectedItem as BuyerViewModel;
                    BuyerId = buyerViewModel.BuyerViewModelId;
                    MainPage.GoToBuyerPage("Edit buyer", BuyerId);
                }
            }
        }

        private void BuyerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BuyerListView.SelectedItem == null)
            {
                EditAppBarButton.IsEnabled = false;
                DeleteAppBarButton.IsEnabled = false;
                CopyAppBarButton.IsEnabled = false;
            }
            else
            {
                EditAppBarButton.IsEnabled = true;
                DeleteAppBarButton.IsEnabled = true;
                CopyAppBarButton.IsEnabled = true;
            }
        }

        private async void DeleteAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (BuyerListView.SelectedItem != null)
                {
                    BuyerViewModel buyerViewModel = BuyerListView.SelectedItem as BuyerViewModel;

                    BuyerId = buyerViewModel.BuyerViewModelId; // this will be deleted

                    await App.Repository.Buyers.DeleteAsync(buyerViewModel.BuyerViewModelId).ConfigureAwait(false);

                    MainPage.NotifyUser($"Buyer was deleted.", NotifyType.StatusMessage);

                    AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);

                    FillBuyerListView();
                }
            }
        }

        private void BuyerListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (BuyerListView.SelectedItem != null)
            {
                BuyerViewModel buyerViewModel = BuyerListView.SelectedItem as BuyerViewModel;
                BuyerId = buyerViewModel.BuyerViewModelId;
                MainPage.GoToBuyerPage("Edit buyer", BuyerId);
            }
        }

        private void NewAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToBuyerPage("New buyer", Guid.Empty);
            }
        }

        private async void CopyAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (BuyerListView.SelectedItem != null)
                {
                    BuyerViewModel buyerViewModel = BuyerListView.SelectedItem as BuyerViewModel;

                    BuyerId = buyerViewModel.BuyerViewModelId;

                    Buyer newBuyer = new Buyer(buyerViewModel.BuyerName)
                    {
                        BuyerEmail = buyerViewModel.BuyerEmail,
                        BuyerAddress = buyerViewModel.BuyerAddress,
                        BuyerPhonenumber = buyerViewModel.BuyerPhonenumber,
                    };

                    Buyer savedBuyer = await App.Repository.Buyers.UpsertAsync(newBuyer).ConfigureAwait(false);

                    if (savedBuyer != null)
                    {
                        BuyerId = savedBuyer.BuyerId;

                        AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);

                        FillBuyerListView();

                        MainPage.NotifyUser($"Buyer was copied.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser($"Buyer was not copied. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        #region sort table header
        bool sortAscendingByName;
        private void TableHeaderBuyerNameTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = BuyerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByName)
                {
                    case true:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name ↓";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address";
                        HeaderBuyerViewModel.HeaderAddress = "Address";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber";
                        break;
                    default:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name ↑";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address";
                        HeaderBuyerViewModel.HeaderAddress = "Address";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber";
                        break;
                }
                BuyerListView.DataContext = HeaderBuyerViewModel;
                BuyerListView.ItemsSource = sortAscendingByName == true ? BuyerViewModels.OrderBy(v => v.BuyerName) : BuyerViewModels.OrderByDescending(v => v.BuyerName);
                sortAscendingByName = !sortAscendingByName;
            }
            if (selectedItem != null)
            {
                BuyerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByEmail;
        private void TableHeaderEmailTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = BuyerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByEmail)
                {
                    case true:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address ↓";
                        HeaderBuyerViewModel.HeaderAddress = "Address";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber";
                        break;
                    default:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address ↑";
                        HeaderBuyerViewModel.HeaderAddress = "Address";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber";
                        break;
                }
                BuyerListView.DataContext = HeaderBuyerViewModel;
                BuyerListView.ItemsSource = sortAscendingByEmail == true ? BuyerViewModels.OrderBy(v => v.BuyerEmail) : BuyerViewModels.OrderByDescending(v => v.BuyerEmail);
                sortAscendingByEmail = !sortAscendingByEmail;
            }
            if (selectedItem != null)
            {
                BuyerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByAddress;
        private void TableHeaderAddressTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = BuyerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByAddress)
                {
                    case true:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address";
                        HeaderBuyerViewModel.HeaderAddress = "Address ↓";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber";
                        break;
                    default:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address";
                        HeaderBuyerViewModel.HeaderAddress = "Address ↑";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber";
                        break;
                }
                BuyerListView.DataContext = HeaderBuyerViewModel;
                BuyerListView.ItemsSource = sortAscendingByAddress == true ? BuyerViewModels.OrderBy(v => v.BuyerAddress) : BuyerViewModels.OrderByDescending(v => v.BuyerAddress);
                sortAscendingByAddress = !sortAscendingByAddress;
            }
            if (selectedItem != null)
            {
                BuyerListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByPhonenumber;
        private void TableHeaderPhonenumberTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = BuyerListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByPhonenumber)
                {
                    case true:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address";
                        HeaderBuyerViewModel.HeaderAddress = "Address";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber ↓";
                        break;
                    default:
                        HeaderBuyerViewModel.HeaderBuyerName = "Name";
                        HeaderBuyerViewModel.HeaderEmail = "E-mail address";
                        HeaderBuyerViewModel.HeaderAddress = "Address";
                        HeaderBuyerViewModel.HeaderPhonenumber = "Phonenumber ↑";
                        break;
                }
                BuyerListView.DataContext = HeaderBuyerViewModel;
                BuyerListView.ItemsSource = sortAscendingByPhonenumber == true ? BuyerViewModels.OrderBy(v => v.BuyerPhonenumber) : BuyerViewModels.OrderByDescending(v => v.BuyerPhonenumber);
                sortAscendingByPhonenumber = !sortAscendingByPhonenumber;
            }
            if (selectedItem != null)
            {
                BuyerListView.SelectedItem = selectedItem;
            }
        }
        #endregion sort table header

        private void SearchBuyerTextChanged(object sender, TextChangedEventArgs e)
        {
            string buyerSearchText = SearchBuyerTextBox.Text.Trim();
            if (string.IsNullOrEmpty(buyerSearchText))
            {
                MainPage.NotifyUser("Search string is empty.", NotifyType.StatusMessage);

                if (SearchWasActive)
                {
                    FillBuyerListView();
                    SearchWasActive = false;
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

            SearchWasActive = true;

            BuyerListView.ItemsSource = null;
            BuyerListView.DataContext = new HeaderBuyerViewModel();
            BuyerViewModels.Clear();
            foreach (Buyer buyer in matches
                        .OrderBy(v => v.BuyerName)
                        .ThenBy(v => v.BuyerEmail)
                        .ThenBy(v => v.BuyerAddress)
                        .ThenBy(v => v.BuyerPhonenumber))
            {
                BuyerViewModels.Add(ProjectToViewModel.NewBuyerViewModel(buyer));
            }
            BuyerListView.ItemsSource = BuyerViewModels;

            BuyerViewModel buyerListViewSelectedItem = BuyerViewModels.FirstOrDefault(o => o.BuyerViewModelId == BuyerId);
            if (buyerListViewSelectedItem != null)
            {
                BuyerListView.SelectedItem = buyerListViewSelectedItem;
            }
        }

        private async void CancelSearchBuyerButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBuyerTextBox.Text))
            {
                MainPage.NotifyUser("Nothing to cancel.", NotifyType.StatusMessage);
            }
            SearchBuyerTextBox.Text = string.Empty;

            if (SearchWasActive)
            {
                AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);
                FillBuyerListView();
                SearchWasActive = false;
            }
        }

        private async void ExportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                string suggestedFileName = $"{HelpFileName.AddDateTimeNowToFileName("InvoicesNow_Buyers")}.json";
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

                    IEnumerable<Buyer> allBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(false);

                    List<DTOBuyer> allDTOBuyers = new List<DTOBuyer>();
                    foreach (var buyer in allBuyers)
                    {
                        allDTOBuyers.Add(new DTOBuyer()
                        {
                            BuyerId = buyer.BuyerId,
                            BuyerName = buyer.BuyerName,

                            CreatedAtDateTime = buyer.CreatedAtDateTime,
                            UpdatedAtDateTime = buyer.UpdatedAtDateTime,

                            BuyerEmail = buyer.BuyerEmail,
                            BuyerAddress = buyer.BuyerAddress,
                            BuyerPhonenumber = buyer.BuyerPhonenumber,
                        });
                    }

                    string jsonData = await Task.Run(() => JsonConvert.SerializeObject(allDTOBuyers)).ConfigureAwait(false);

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

                        if (BuyerListView.SelectedItem != null)
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
                    int importedDTOBuyerCount = 0;
                    int existingBuyerCount = 0;

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
                        List<DTOBuyer> ImportedDTOBuyers = JsonConvert.DeserializeObject<List<DTOBuyer>>(fileContent);
                        foreach (DTOBuyer ImportedDTOBuyer in ImportedDTOBuyers)
                        {
                            Buyer newImportedBuyer = new Buyer(ImportedDTOBuyer);

                            if (!newImportedBuyer.BuyerId.Equals(Guid.Empty))
                            {
                                Buyer existingBuyer = await App.Repository.Buyers.GetBuyerAsync(newImportedBuyer.BuyerId).ConfigureAwait(false);
                                if (existingBuyer != null)
                                {
                                    if (ImportedDTOBuyer.UpdatedAtDateTime > existingBuyer.UpdatedAtDateTime)
                                    {
                                        Buyer savedBuyer = await App.Repository.Buyers.UpsertAsync(newImportedBuyer).ConfigureAwait(false);
                                        if (savedBuyer != null)
                                        {
                                            importedDTOBuyerCount++;
                                        }
                                        else
                                        {
                                            throw new ArgumentNullException("Failure when importing buyer.");
                                        }
                                    }
                                    else
                                    {
                                        existingBuyerCount++;
                                    }
                                }
                                else
                                {
                                    Buyer savedBuyer = await App.Repository.Buyers.UpsertAsync(newImportedBuyer).ConfigureAwait(false);
                                    if (savedBuyer != null)
                                    {
                                        importedDTOBuyerCount++;
                                    }
                                }
                            }
                        }

                        if (importedDTOBuyerCount > 0)
                        {
                            AllBuyers = await App.Repository.Buyers.GetAllBuyersAsync().ConfigureAwait(true);

                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                FillBuyerListView();

                                MainPage.NotifyUser($"Imported {importedDTOBuyerCount}({importedDTOBuyerCount + existingBuyerCount}) {(importedDTOBuyerCount == 1 ? "buyer" : "buyers")}.", NotifyType.StatusMessage);
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (existingBuyerCount == 0)
                                {
                                    MainPage.NotifyUser($"File didn't contain buyers. Try again.", NotifyType.ErrorMessage);
                                }
                                else
                                {
                                    MainPage.NotifyUser($"Didn't import {existingBuyerCount}({importedDTOBuyerCount + existingBuyerCount}) {(existingBuyerCount == 1 ? "duplicate buyer" : "duplicate buyers")}.", NotifyType.StatusMessage);
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
                            ExportDataProgressRing.IsActive = false;

                            NewAppBarButton.IsEnabled = true;

                            if (BuyerListView.SelectedItem != null)
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
