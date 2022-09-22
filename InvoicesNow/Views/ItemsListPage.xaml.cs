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
    public sealed partial class ItemsListPage : Page
    {
        Guid ItemId { get; set; }

        MainPage MainPage { get; }

        CultureInfo CurrentCulture { get; } = CultureInfo.CurrentCulture;

        ObservableCollection<ItemViewModel> ItemViewModels { get; set; } = new ObservableCollection<ItemViewModel>();

        IEnumerable<Item> AllItems { get; set; }

        HeaderItemViewModel HeaderItemViewModel { get; set; } = new HeaderItemViewModel();

        bool SearchWasActive { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public ItemsListPage()
        {
            InitializeComponent();

            Loaded += ItemsListPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void ItemsListPage_Loaded(object sender, RoutedEventArgs e)
        {
            AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);

            FillItemListView();
        }

        private void FillItemListView()
        {
            ExportDataAppBarButton.IsEnabled = AllItems.Count() > 0 ? true : false;

            ItemListView.DataContext = new HeaderItemViewModel();

            ItemViewModels.Clear();
            foreach (Item item in AllItems
                                    .OrderBy(v => v.Name)
                                    .ThenBy(v => v.Tax)
                                    .ThenBy(v => v.Price))
            {
                ItemViewModel itemViewModelToAdd = ProjectToViewModel.NewItemViewModel(item);
                ItemViewModels.Add(itemViewModelToAdd);
            }

            ItemViewModel itemListViewSelectedItem = ItemViewModels.FirstOrDefault(o => o.ItemViewModelId == ItemId);
            if (itemListViewSelectedItem != null)
            {
                ItemListView.SelectedItem = itemListViewSelectedItem;
            }
            else
            {
                var first = ItemViewModels.FirstOrDefault();
                if (first != null)
                {
                    ItemListView.SelectedItem = first;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                ItemId = Guid.Parse(e.Parameter.ToString());
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
                if (ItemListView.SelectedItem != null)
                {
                    ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;
                    ItemId = itemViewModel.ItemViewModelId;
                    MainPage.GoToItemPage("Edit item", ItemId);
                }
            }
        }

        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemListView.SelectedItem == null)
            {
                EditAppBarButton.IsEnabled = false;
                DeleteAppBarButton.IsEnabled = false;
                CopyAppBarButton.IsEnabled = false;
            }
            else
            {
                ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;
                ItemId = itemViewModel.ItemViewModelId;

                EditAppBarButton.IsEnabled = true;
                DeleteAppBarButton.IsEnabled = true;
                CopyAppBarButton.IsEnabled = true;
            }
        }

        private async void DeleteAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (ItemListView.SelectedItem != null)
                {
                    ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;

                    ItemId = itemViewModel.ItemViewModelId; // this will be deleted

                    await App.Repository.Items.DeleteAsync(itemViewModel.ItemViewModelId).ConfigureAwait(false);

                    MainPage.NotifyUser($"Item was deleted.", NotifyType.StatusMessage);

                    AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);

                    FillItemListView();
                }
            }
        }

        private void ItemListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ItemListView.SelectedItem != null)
            {
                ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;
                ItemId = itemViewModel.ItemViewModelId;
                MainPage.GoToItemPage("Edit item", ItemId);
            }
        }

        private void NewAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToItemPage("New item", Guid.Empty);
            }
        }

        private async void CopyAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (ItemListView.SelectedItem != null)
                {
                    ItemViewModel itemViewModel = ItemListView.SelectedItem as ItemViewModel;

                    ItemId = itemViewModel.ItemViewModelId;

                    Item copiedItem = new Item
                    {
                        Name = itemViewModel.Name,
                        Tax = itemViewModel.Tax,
                        Price = itemViewModel.Price
                    };

                    Item savedItem = await App.Repository.Items.UpsertAsync(copiedItem).ConfigureAwait(false);

                    if (savedItem != null)
                    {
                        ItemId = savedItem.ItemId;

                        AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);

                        FillItemListView();

                        MainPage.NotifyUser($"Item was copied.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser($"Item was not copied. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        #region sort table header
        bool sortAscendingByName;
        private void TableHeaderNameTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = ItemListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByName)
                {
                    case true:
                        HeaderItemViewModel.HeaderName = "Name ↓";
                        HeaderItemViewModel.HeaderTax = "Tax";
                        HeaderItemViewModel.HeaderPrice = "Price";
                        break;
                    default:
                        HeaderItemViewModel.HeaderName = "Name ↑";
                        HeaderItemViewModel.HeaderTax = "Tax";
                        HeaderItemViewModel.HeaderPrice = "Price";
                        break;
                }
                ItemListView.DataContext = HeaderItemViewModel;
                ItemListView.ItemsSource = sortAscendingByName == true ? ItemViewModels.OrderBy(v => v.Name) : ItemViewModels.OrderByDescending(v => v.Name);
                sortAscendingByName = !sortAscendingByName;
            }
            if (selectedItem != null)
            {
                ItemListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByTax;
        private void TableHeaderTaxTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = ItemListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByTax)
                {
                    case true:
                        HeaderItemViewModel.HeaderName = "Name";
                        HeaderItemViewModel.HeaderTax = "Tax ↓";
                        HeaderItemViewModel.HeaderPrice = "Price";
                        break;
                    default:
                        HeaderItemViewModel.HeaderName = "Name";
                        HeaderItemViewModel.HeaderTax = "Tax ↑";
                        HeaderItemViewModel.HeaderPrice = "Price";
                        break;
                }
                ItemListView.DataContext = HeaderItemViewModel;
                ItemListView.ItemsSource = sortAscendingByTax == true ? ItemViewModels.OrderBy(v => v.Tax) : ItemViewModels.OrderByDescending(v => v.Tax);
                sortAscendingByTax = !sortAscendingByTax;
            }
            if (selectedItem != null)
            {
                ItemListView.SelectedItem = selectedItem;
            }
        }

        bool sortAscendingByPrice;
        private void TableHeaderPriceTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            object selectedItem = ItemListView.SelectedItem;
            if (sender as TextBlock != null)
            {
                switch (sortAscendingByPrice)
                {
                    case true:
                        HeaderItemViewModel.HeaderName = "Name";
                        HeaderItemViewModel.HeaderTax = "Tax";
                        HeaderItemViewModel.HeaderPrice = "Price ↓";
                        break;
                    default:
                        HeaderItemViewModel.HeaderName = "Name";
                        HeaderItemViewModel.HeaderTax = "Tax";
                        HeaderItemViewModel.HeaderPrice = "Price ↑";
                        break;
                }
                ItemListView.DataContext = HeaderItemViewModel;
                ItemListView.ItemsSource = sortAscendingByPrice == true ? ItemViewModels.OrderBy(v => v.Price) : ItemViewModels.OrderByDescending(v => v.Price);
                sortAscendingByPrice = !sortAscendingByPrice;
            }
            if (selectedItem != null)
            {
                ItemListView.SelectedItem = selectedItem;
            }
        }
        #endregion sort table header

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

        private async void ExportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                string suggestedFileName = $"{HelpFileName.AddDateTimeNowToFileName("InvoicesNow_Items")}.json";
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

                    IEnumerable<Item> allItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(false);

                    List<DTOItem> allDTOItems = new List<DTOItem>();
                    foreach (var item in allItems)
                    {
                        allDTOItems.Add(new DTOItem()
                        {
                            ItemId = item.ItemId,
                            Name = item.Name,

                            CreatedAtDateTime = item.CreatedAtDateTime,
                            UpdatedAtDateTime = item.UpdatedAtDateTime,

                            Tax = item.Tax,
                            Price = item.Price,
                        });
                    }

                    string jsonData = await Task.Run(() => JsonConvert.SerializeObject(allDTOItems)).ConfigureAwait(false);

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

                        if (ItemListView.SelectedItem != null)
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
                    int importedDTOItemCount = 0;
                    int existingItemCount = 0;

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
                        List<DTOItem> ImportedDTOItems = JsonConvert.DeserializeObject<List<DTOItem>>(fileContent);
                        foreach (DTOItem ImportedDTOItem in ImportedDTOItems)
                        {
                            Item newImportedItem = new Item(ImportedDTOItem);

                            if (!newImportedItem.ItemId.Equals(Guid.Empty))
                            {
                                Item existingItem = await App.Repository.Items.GetItemAsync(newImportedItem.ItemId).ConfigureAwait(false);
                                if (existingItem != null)
                                {
                                    if (ImportedDTOItem.UpdatedAtDateTime > existingItem.UpdatedAtDateTime)
                                    {
                                        Item savedItem = await App.Repository.Items.UpsertAsync(newImportedItem).ConfigureAwait(false);
                                        if (savedItem != null)
                                        {
                                            importedDTOItemCount++;
                                        }
                                        else
                                        {
                                            throw new ArgumentNullException("Failure when importing item.");
                                        }
                                    }
                                    else
                                    {
                                        existingItemCount++;
                                    }
                                }
                                else
                                {
                                    Item savedItem = await App.Repository.Items.UpsertAsync(newImportedItem).ConfigureAwait(false);
                                    if (savedItem != null)
                                    {
                                        importedDTOItemCount++;
                                    }
                                }
                            }
                        }

                        if (importedDTOItemCount > 0)
                        {
                            AllItems = await App.Repository.Items.GetAllItemsAsync().ConfigureAwait(true);

                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                FillItemListView();

                                MainPage.NotifyUser($"Imported {importedDTOItemCount}({importedDTOItemCount + existingItemCount}) {(importedDTOItemCount == 1 ? "item" : "items")}.", NotifyType.StatusMessage);
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (existingItemCount == 0)
                                {
                                    MainPage.NotifyUser($"File didn't contain items. Try again.", NotifyType.ErrorMessage);
                                }
                                else
                                {
                                    MainPage.NotifyUser($"Didn't import {existingItemCount}({importedDTOItemCount + existingItemCount}) {(existingItemCount == 1 ? "duplicate item" : "duplicate items")}.", NotifyType.StatusMessage);
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

                            if (ItemListView.SelectedItem != null)
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

