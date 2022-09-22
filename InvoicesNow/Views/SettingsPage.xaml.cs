using InvoicesNow.Helpers;
using InvoicesNow.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class SettingsPage : Page
    {
        MainPage MainPage { get; }

        IEnumerable<Invoice> AllInvoices { get; set; }

        public SettingsPage()
        {
            InitializeComponent();

            Loaded += SettingsPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            StateForInvoiceNumbersTextBlock.Text = App.UseSerieAsInvoiceNumber ? "Your invoice numbers use serie for now." : "Your invoice numbers use date for now.";
            object latestSerie = App.LocalSettings.Values["LatestUsedInvoiceNumberSerie"];
            if (latestSerie != null)
            {
                SerieTextBox.Text = latestSerie.ToString();
            }
            else
            {
                SerieTextBox.Text = 1000.ToString();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            // code here
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // code here
            // code here
        }

        #region MenuAppBarButton
        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToHomePage();
            }
        }
        #endregion MenuAppBarButton

        private async void SerieInvoiceNumberButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int number;
            if (int.TryParse(SerieTextBox.Text, out number))
            {
                if (number < 0)
                {
                    MainPage.NotifyUser("Serie is required. Only positive numbers. Try again.", NotifyType.ErrorMessage);
                    return;
                }

                AllInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

                foreach (var existingInvoice in AllInvoices.OrderBy(o => o.InvoiceDate).ThenByDescending(o=>o.CreatedAtDateTime))
                {
                    var invoice = await App.Repository.Invoices.SetNewInvoiceNumberAsync(existingInvoice.InvoiceId, number).ConfigureAwait(false);
                    if (invoice != null)
                    {
                        MainPage.NotifyUser($" New invoice number set {number}.", NotifyType.StatusMessage);
                    }
                    number++;
                }
                App.UseSerieAsInvoiceNumber = true;
                StateForInvoiceNumbersTextBlock.Text = "Your invoice numbers use serie for now.";
                App.LocalSettings.Values["UseSerieAsInvoiceNumber"] = App.UseSerieAsInvoiceNumber;
                App.LocalSettings.Values["LatestUsedInvoiceNumberSerie"] = SerieTextBox.Text;

                MainPage.GoToInvoicesListPage(App.LatestVisitedInvoiceId);
            }
            else
            {
                MainPage.NotifyUser("Serie is required. Try again.", NotifyType.ErrorMessage);
            }
        }

        private async void DateInvoiceNumberButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AllInvoices = await App.Repository.Invoices.GetAllInvoicesAsync().ConfigureAwait(false);

            foreach (var existingInvoice in AllInvoices.OrderBy(o => o.InvoiceDate).ThenByDescending(o => o.CreatedAtDateTime))
            {
                var invoiceNumber = await HelpInvoiceNumber.GetNewDateInvoiceNumberAsync(existingInvoice.InvoiceDate).ConfigureAwait(false);

                var invoice = await App.Repository.Invoices.SetNewInvoiceNumberAsync(existingInvoice.InvoiceId, invoiceNumber).ConfigureAwait(false);
                if (invoice != null)
                {
                    MainPage.NotifyUser($" New invoice number set {invoiceNumber}.", NotifyType.StatusMessage);
                }
            }
            App.UseSerieAsInvoiceNumber = false;
            StateForInvoiceNumbersTextBlock.Text = "Your invoice numbers use date for now.";
            App.LocalSettings.Values["UseSerieAsInvoiceNumber"] = App.UseSerieAsInvoiceNumber;

            MainPage.GoToInvoicesListPage(App.LatestVisitedInvoiceId);
        }
    }
}
