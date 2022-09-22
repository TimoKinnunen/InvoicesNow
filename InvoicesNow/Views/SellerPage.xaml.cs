using InvoicesNow.Models;
using InvoicesNow.Projections;
using InvoicesNow.ViewModels;
using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class SellerPage : Page
    {
        MainPage MainPage { get; }

        Guid SellerId { get; set; }

        Seller ExistingSeller { get; set; }

        SellerViewModel SellerViewModel { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public SellerPage()
        {
            InitializeComponent();

            Loaded += SellerPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void SellerPage_Loaded(object sender, RoutedEventArgs e)
        {
            ExistingSeller = await App.Repository.Sellers.GetSellerAsync(SellerId).ConfigureAwait(false);
            if (ExistingSeller != null)
            {
                SellerViewModel = ProjectToViewModel.NewSellerViewModel(ExistingSeller);
            }
            else
            {
                SellerViewModel = new SellerViewModel(new Seller());
            }

            SellerStackPanel.DataContext = SellerViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                string parameter = e.Parameter.ToString();
                string[] parameters = parameter.Split(':');

                PageTitleTextBlock.Text = parameters[0]; // 'New seller' or 'Edit seller'
                SellerId = Guid.Parse(parameters[1]);
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
                if (string.IsNullOrWhiteSpace(SellerViewModel.SellerName))
                {
                    MainPage.NotifyUser("Name is required.", NotifyType.ErrorMessage);

                    return;
                }

                Seller savedSeller;
                if (ExistingSeller == null)
                {
                    Seller newSeller = new Seller(SellerViewModel.SellerName)
                    {
                        CreatedAtDateTime = SellerViewModel.CreatedAtDateTime,
                        UpdatedAtDateTime = SellerViewModel.UpdatedAtDateTime,

                        SellerEmail = SellerViewModel.SellerEmail,
                        SellerAddress = SellerViewModel.SellerAddress,
                        SellerPhonenumber = SellerViewModel.SellerPhonenumber,
                        SellerAccount = SellerViewModel.SellerAccount,
                        SellerSWIFTBIC = SellerViewModel.SellerSWIFTBIC,
                        SellerIBAN = SellerViewModel.SellerIBAN,
                    };

                    savedSeller = await App.Repository.Sellers.UpsertAsync(newSeller).ConfigureAwait(false);
                    if (savedSeller != null)
                    {
                        MainPage.GoToSellersListPage(savedSeller.SellerId);
                        MainPage.NotifyUser("Seller was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Seller was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
                else
                {
                    ExistingSeller.SellerName = SellerViewModel.SellerName;
                    ExistingSeller.SellerEmail = SellerViewModel.SellerEmail;
                    ExistingSeller.SellerAddress = SellerViewModel.SellerAddress;
                    ExistingSeller.SellerPhonenumber = SellerViewModel.SellerPhonenumber;
                    ExistingSeller.SellerAccount = SellerViewModel.SellerAccount;
                    ExistingSeller.SellerSWIFTBIC = SellerViewModel.SellerSWIFTBIC;
                    ExistingSeller.SellerIBAN = SellerViewModel.SellerIBAN;

                    savedSeller = await App.Repository.Sellers.UpsertAsync(ExistingSeller).ConfigureAwait(false);
                    if (savedSeller != null)
                    {
                        MainPage.GoToSellersListPage(savedSeller.SellerId);
                        MainPage.NotifyUser("Seller was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Seller was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private void BackAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToSellersListPage(SellerId);
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
    }
}