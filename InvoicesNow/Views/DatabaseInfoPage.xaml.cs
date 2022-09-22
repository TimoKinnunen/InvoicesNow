using InvoicesNow.Helpers;
using System;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class DatabaseInfoPage : Page
    {
        MainPage MainPage { get; }

        const string databaseNameWithExtension = "InvoicesNow.db";

        public DatabaseInfoPage()
        {
            InitializeComponent();

            Loaded += DatabaseInfoPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void DatabaseInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFolder localState = ApplicationData.Current.LocalFolder; //this is LocalState
            StorageFile storageFile = await localState.GetFileAsync(databaseNameWithExtension);
            if (storageFile != null)
            {
                BasicProperties basicPropertiesInvoicesNow = await storageFile.GetBasicPropertiesAsync();
                InvoicesNowFileSize.Text = $"{databaseNameWithExtension} size on disk is {HelpToFileSize.ToFileSize(basicPropertiesInvoicesNow.Size)}.";
                InvoicesNowFilePath.Text = storageFile.Path;
            }
            else
            {
                InvoicesNowFileSize.Text = $"File {databaseNameWithExtension} is missing.";
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
    }
}

