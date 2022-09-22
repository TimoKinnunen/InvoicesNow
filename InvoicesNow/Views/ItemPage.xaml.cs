using InvoicesNow.Models;
using InvoicesNow.ViewModels;
using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class ItemPage : Page
    {
        MainPage MainPage { get; }

        Guid ItemId { get; set; }

        Item ExistingItem { get; set; }

        ItemViewModel ItemViewModel { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public ItemPage()
        {
            InitializeComponent();

            Loaded += ItemPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void ItemPage_Loaded(object sender, RoutedEventArgs e)
        {
            ExistingItem = await App.Repository.Items.GetItemAsync(ItemId).ConfigureAwait(false);

            if (ExistingItem != null)
            {
                ItemViewModel = new ItemViewModel(ExistingItem);
            }
            else
            {
                ItemViewModel = new ItemViewModel(new Item());
            }

            ItemGrid.DataContext = ItemViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                string parameter = e.Parameter.ToString();
                string[] parameters = parameter.Split(':');

                PageTitleTextBlock.Text = parameters[0]; // 'New item' or 'Edit item'
                ItemId = Guid.Parse(parameters[1]);
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

        private async void SaveAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                if (string.IsNullOrWhiteSpace(ItemViewModel.Name))
                {
                    MainPage.NotifyUser("Name is required.", NotifyType.ErrorMessage);
                    return;
                }

                Item savedItem;
                if (ExistingItem == null)
                {
                    Item newItem = new Item(ItemViewModel.Name)
                    {
                        CreatedAtDateTime = ItemViewModel.CreatedAtDateTime,
                        UpdatedAtDateTime = ItemViewModel.UpdatedAtDateTime,

                        Tax = ItemViewModel.Tax,
                        Price = ItemViewModel.Price,
                    };

                    savedItem = await App.Repository.Items.UpsertAsync(newItem).ConfigureAwait(false);
                    if (savedItem != null)
                    {
                        MainPage.GoToItemsListPage(savedItem.ItemId);
                        MainPage.NotifyUser("Item was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Item was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
                else
                {
                    ExistingItem.Name = ItemViewModel.Name;
                    ExistingItem.Tax = ItemViewModel.Tax;
                    ExistingItem.Price = ItemViewModel.Price;

                    savedItem = await App.Repository.Items.UpsertAsync(ExistingItem).ConfigureAwait(false);
                    if (savedItem != null)
                    {
                        MainPage.GoToItemsListPage(savedItem.ItemId);
                        MainPage.NotifyUser("Item was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Item was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private void BackAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToItemsListPage(ItemId);
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameTextBox.Text))
            {
                SaveAppBarButton.IsEnabled = true;
            }
            else
            {
                SaveAppBarButton.IsEnabled = false;
            }
        }

        private void TaxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                decimal value = ItemViewModel.BigTax;
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
                        ItemViewModel.BigTax = taxValue;
                        ItemViewModel.Tax = Math.Round(taxValue / 100, 4);
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
                decimal value = ItemViewModel.Price;
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
                        ItemViewModel.Price = Math.Round(priceValue, 2);
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
