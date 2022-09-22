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
    public sealed partial class BuyerPage : Page
    {
        MainPage MainPage { get; }

        Guid BuyerId { get; set; }

        Buyer ExistingBuyer { get; set; }

        BuyerViewModel BuyerViewModel { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public BuyerPage()
        {
            InitializeComponent();

            Loaded += BuyerPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void BuyerPage_Loaded(object sender, RoutedEventArgs e)
        {
            ExistingBuyer = await App.Repository.Buyers.GetBuyerAsync(BuyerId).ConfigureAwait(false);
            if (ExistingBuyer != null)
            {
                BuyerViewModel = ProjectToViewModel.NewBuyerViewModel(ExistingBuyer);
            }
            else
            {
                BuyerViewModel = new BuyerViewModel(new Buyer());
            }

            BuyerStackPanel.DataContext = BuyerViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            if (e.Parameter != null)
            {
                string parameter = e.Parameter.ToString();
                string[] parameters = parameter.Split(':');

                PageTitleTextBlock.Text = parameters[0]; // 'New buyer' or 'Edit buyer'
                BuyerId = Guid.Parse(parameters[1]);
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
                if (string.IsNullOrWhiteSpace(BuyerViewModel.BuyerName))
                {
                    MainPage.NotifyUser("Name is required.", NotifyType.ErrorMessage);

                    return;
                }

                Buyer savedBuyer;
                if (ExistingBuyer == null)
                {
                    Buyer newBuyer = new Buyer(BuyerViewModel.BuyerName)
                    {
                        CreatedAtDateTime = BuyerViewModel.CreatedAtDateTime,
                        UpdatedAtDateTime = BuyerViewModel.UpdatedAtDateTime,

                        BuyerEmail = BuyerViewModel.BuyerEmail,
                        BuyerAddress = BuyerViewModel.BuyerAddress,
                        BuyerPhonenumber = BuyerViewModel.BuyerPhonenumber
                    };

                    savedBuyer = await App.Repository.Buyers.UpsertAsync(newBuyer).ConfigureAwait(false);
                    if (savedBuyer != null)
                    {
                        MainPage.GoToBuyersListPage(savedBuyer.BuyerId);
                        MainPage.NotifyUser("Buyer was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Buyer was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
                else
                {
                    ExistingBuyer.BuyerName = BuyerViewModel.BuyerName;
                    ExistingBuyer.BuyerEmail = BuyerViewModel.BuyerEmail;
                    ExistingBuyer.BuyerAddress = BuyerViewModel.BuyerAddress;
                    ExistingBuyer.BuyerPhonenumber = BuyerViewModel.BuyerPhonenumber;

                    savedBuyer = await App.Repository.Buyers.UpsertAsync(ExistingBuyer).ConfigureAwait(false);
                    if (savedBuyer != null)
                    {
                        MainPage.GoToBuyersListPage(savedBuyer.BuyerId);
                        MainPage.NotifyUser("Buyer was saved.", NotifyType.StatusMessage);
                    }
                    else
                    {
                        MainPage.NotifyUser("Buyer was not saved. Something went wrong. Try again.", NotifyType.StatusMessage);
                    }
                }
            }
        }

        private void BackAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToBuyersListPage(BuyerId);
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
