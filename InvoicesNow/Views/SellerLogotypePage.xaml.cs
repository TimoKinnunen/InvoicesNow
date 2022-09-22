using InvoicesNow.Helpers;
using InvoicesNow.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class SellerLogotypePage : Page
    {
        MainPage MainPage { get; }

        Guid SellerId { get; set; }

        string SellerName { get; set; }

        Seller ExistingSeller { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        StorageFile pickedFile { get; set; }

        StorageFile temporaryFileFromLogotypeMaker { get; set; }

        public SellerLogotypePage()
        {
            InitializeComponent();

            Loaded += SellerLogotypePage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private async void SellerLogotypePage_Loaded(object sender, RoutedEventArgs e)
        {
            ExistingSeller = await App.Repository.Sellers.GetSellerAsync(SellerId).ConfigureAwait(false);
            if (ExistingSeller != null)
            {
                SellerNameTextBlock.Text = SellerName = ExistingSeller.SellerName;
                GetExistingSellerLogotype();
            }
            else
            {
                NewAppBarButton.IsEnabled = false;
                MainPage.NotifyUser("Seller's name is required.", NotifyType.ErrorMessage);
            }
        }

        private async void GetExistingSellerLogotype()
        {
            StorageFolder logotypesStorageFolder = await GetLogotypesStorageFolder();
            IReadOnlyList<StorageFile> fileList = await logotypesStorageFolder.GetFilesAsync();
            StorageFile existingLogotype = fileList.FirstOrDefault(o => o.DisplayName.ToUpper() == SellerId.ToString().ToUpper());
            if (existingLogotype != null)
            {
                RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromFile(existingLogotype);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(await stream.OpenReadAsync());
                LogotypeBitmapImage.Source = bitmapImage;

                DeleteAppBarButton.IsEnabled = true;
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

        private void BackAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToSellersListPage(SellerId);
            }
        }

        private async void NewAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                FileOpenPicker fileOpenPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    CommitButtonText = "Open picture",
                    SettingsIdentifier = "Open picture",
                };
                // Filter to include a sample subset of file types
                fileOpenPicker.FileTypeFilter.Clear();
                fileOpenPicker.FileTypeFilter.Add(".bmp");
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.FileTypeFilter.Add(".jpeg");
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.FileTypeFilter.Add(".tif");

                pickedFile = await fileOpenPicker.PickSingleFileAsync();

                if (pickedFile == null)
                {
                    MainPage.NotifyUser("Operation canceled.", NotifyType.StatusMessage);
                    return;
                }

                DeleteAppBarButton.IsEnabled = true;
                SaveAppBarButton.IsEnabled = true;
                LogotypeWidthTextBlock.Visibility = Visibility.Visible;
                LogotypeWidthSlider.Visibility = Visibility.Visible;
                OriginalSizedBitmapTextBlock.Visibility = Visibility.Visible;
                OriginalSizedBitmapImageScrollViewer.Visibility = Visibility.Visible;

                await ShowLogotypeBitmapImage();

                await ShowOriginalSizedBitmapImage();
            }
        }

        private void SaveAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                SaveLogotype();
            }
        }

        private async Task ShowLogotypeBitmapImage()
        {
            using (IRandomAccessStream fileStream = await pickedFile.OpenReadAsync())
            {
                // LogotypeMaker saves temporay images to App.TemporaryFolder
                HelpLogotypeMaker logotypeMaker = new HelpLogotypeMaker(SellerId);
                temporaryFileFromLogotypeMaker = await logotypeMaker.GenerateLogotypeAsThumbnailAsync(pickedFile, (uint)LogotypeWidthSlider.Value);
                RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromFile(temporaryFileFromLogotypeMaker);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(await stream.OpenReadAsync());
                LogotypeBitmapImage.Source = bitmapImage;
            }
        }

        private async Task ShowOriginalSizedBitmapImage()
        {
            using (IRandomAccessStream fileStream = await pickedFile.OpenReadAsync())
            {
                OriginalSizedBitmapImage.Source = LoadWriteableBitmap(fileStream);
            }
        }

        private async void LogotypeWidthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                if (pickedFile != null)
                {
                    try
                    {
                        await ShowLogotypeBitmapImage();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        LogotypeWidthTextBlock.Text = $"{"Pull here to change logotype's width"} ({e.NewValue} pixels):";
                    }
                }
            }
        }

        private static WriteableBitmap LoadWriteableBitmap(IRandomAccessStream stream)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(1, 1);
            writeableBitmap.SetSource(stream);
            writeableBitmap.Invalidate();
            return writeableBitmap;
        }

        private async void DeleteAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                StorageFolder logotypesStorageFolder = await GetLogotypesStorageFolder();
                IReadOnlyList<StorageFile> fileList = await logotypesStorageFolder.GetFilesAsync();
                StorageFile existingLogotype = fileList.FirstOrDefault(o => o.DisplayName.ToUpper() == SellerId.ToString().ToUpper());
                if (existingLogotype != null)
                {
                    LogotypeStackPanel.Visibility = Visibility.Collapsed;
                    LogotypeBitmapImage.Source = null;
                    await existingLogotype.DeleteAsync();
                    MainPage.NotifyUser($"Logotype for {SellerName} was deleted.", NotifyType.StatusMessage);
                    MainPage.GoToSellersListPage(SellerId);
                }
            }
        }

        private async void SaveLogotype()
        {
            if (temporaryFileFromLogotypeMaker != null)
            {
                string fileName = temporaryFileFromLogotypeMaker.Name;

                StorageFolder logotypesStorageFolder = await GetLogotypesStorageFolder();

                StorageFile savedLogotype = await temporaryFileFromLogotypeMaker.CopyAsync(logotypesStorageFolder, fileName, NameCollisionOption.ReplaceExisting);
                if (savedLogotype != null)
                {
                    MainPage.NotifyUser($"Logotype for {SellerName} was saved.", NotifyType.StatusMessage);
                    MainPage.GoToSellersListPage(SellerId);
                }
                else
                {
                    MainPage.NotifyUser($"Logotype for {SellerName} was not saved.", NotifyType.StatusMessage);
                }
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
    }
}

