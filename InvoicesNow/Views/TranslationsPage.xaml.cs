using InvoicesNow.Data;
using InvoicesNow.Helpers;
using InvoicesNow.Printing.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.System.UserProfile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class TranslationsPage : Page
    {
        MainPage MainPage { get; }

        CultureInfo CurrentCulture { get; } = CultureInfo.CurrentCulture;

        string CurrentCultureName { get; set; }

        CultureInfo SelectedCulture { get; set; }

        List<TranslationViewModel> TranslationViewModels { get; set; }

        string PageTitleCultureName { get; set; } = $"{CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.NativeName}";

        public TranslationsPage()
        {
            InitializeComponent();

            Loaded += TranslationsPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private void TranslationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentCultureName = CurrentCulture.Name;

            CurrentCultureTextBlock.Text = $"The current culture is {CurrentCulture.EnglishName} " +
                $"{CurrentCulture.NativeName} " +
                $"[{CurrentCulture.Name}] and " +
                $"currency symbol is [{CurrentCulture.NumberFormat.CurrencySymbol}], " +
                $"number decimal separator is '{CurrentCulture.NumberFormat.NumberDecimalSeparator}'.";

            PleaseTranslateToTextBlock.Text = $"Please translate 'English text' to {CurrentCulture.NativeName} in order to localize invoice paragraphs:";

            IReadOnlyList<string> installedLanguagesByUser = GlobalizationPreferences.Languages.ToList();

            List<CultureInfo> userCultureInfoViewModels = new List<CultureInfo>();

            foreach (string installedLanguageByUser in installedLanguagesByUser)
            {
                CultureInfo userCulture = new CultureInfo(installedLanguageByUser);

                try
                {
                    RegionInfo regionInfo = new RegionInfo(userCulture.Name);
                    if (regionInfo != null)
                    {
                        userCultureInfoViewModels.Add(userCulture); // RegionInfo works, currency symbol
                    }
                }
                catch
                {
                    userCultureInfoViewModels.Add(userCulture); // RegionInfo works, currency symbol
                    var yy = userCulture.NumberFormat.CurrencySymbol;
                }
            }

            InstalledLanguagesComboBox.ItemsSource = userCultureInfoViewModels;
            InstalledLanguagesComboBox.SelectedItem = CurrentCulture;

            EnglishBaseTranslationsStringTextBox.Text = BaseTranslations.EnglishBaseTranslationsString;

            ReadFileFromLocalFolder();
        }

        private async void ReadFileFromLocalFolder()
        {
            TranslationViewModels = await HelpTranslationsFromLocalFolder.GetCurrentCultureTranslationsAsync().ConfigureAwait(false);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TranslationListView.ItemsSource = TranslationViewModels;
            });
        }

        private async void InstalledLanguagesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstalledLanguagesComboBox.SelectedItem != null)
            {
                SelectedCulture = InstalledLanguagesComboBox.SelectedItem as CultureInfo;

                CultureInfo.CurrentCulture = SelectedCulture;
                CultureInfo.CurrentUICulture = SelectedCulture;

                CurrentCultureName = SelectedCulture.Name;

                CurrentCultureTextBlock.Text = $"The current culture is {SelectedCulture.EnglishName} " +
                    $"{SelectedCulture.NativeName} " +
                    $"[{SelectedCulture.Name}] and " +
                    $"currency symbol is [{SelectedCulture.NumberFormat.CurrencySymbol}], " +
                    $"number decimal separator is '{SelectedCulture.NumberFormat.NumberDecimalSeparator}'.";

                PleaseTranslateToTextBlock.Text = $"Please translate 'English text' to {SelectedCulture.NativeName} in order to localize invoice paragraphs:";

                try
                {
                    string fileName = $"InvoicesNowTranslations_{CurrentCultureName}.json";
                    StorageFile existingStorageFile = await App.LocalFolder.GetFileAsync(fileName);
                    if (existingStorageFile != null)
                    {
                        PageTitleCultureNameTextBlock.Text = $"{SelectedCulture.Name} {SelectedCulture.NativeName}";
                    }
                }
                catch (FileNotFoundException)
                {
                    PageTitleCultureNameTextBlock.Text = $"{SelectedCulture.Name} {SelectedCulture.NativeName}. Translations are missing! Using English paragraphs.";
                }

                ReadFileFromLocalFolder();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            // code here
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // code here
            CultureInfo.CurrentCulture = SelectedCulture;
            // code here
        }

        private async void SaveAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                string fileName = $"InvoicesNowTranslations_{CurrentCultureName}.json";

                foreach (TranslationViewModel translationViewModel in TranslationViewModels)
                {
                    //ensure TranslatedText is not empty
                    if (string.IsNullOrWhiteSpace(translationViewModel.TranslatedText))
                    {
                        translationViewModel.TranslatedText = translationViewModel.EnglishText;
                    }
                }

                //string jsonData = await Task.Run(() => JsonConvert.SerializeObject(TranslationViewModels.OrderBy(t => t.EnglishText))).ConfigureAwait(false);
                string jsonData = await Task.Run(() => JsonConvert.SerializeObject(TranslationViewModels)).ConfigureAwait(false);

                StorageFile storageFile = await App.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(storageFile);
                // write to file
                await FileIO.WriteTextAsync(storageFile, jsonData);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(storageFile);
                if (status == FileUpdateStatus.Complete)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainPage.NotifyUser($"File '{storageFile.Name}' was saved to local folder.", NotifyType.StatusMessage);
                        MainPage.GoToInvoicesListPage(App.LatestVisitedInvoiceId);
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MainPage.NotifyUser($"File '{storageFile.Name}' couldn't be saved.", NotifyType.ErrorMessage);
                    });
                }
            }
        }

        //private void DeleteAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    TranslationListView.ItemsSource = null;

        //    TranslationViewModels.Clear();
        //    foreach (string baseTranslationInEnglish in BaseTranslations.EnglishBaseTranslations)
        //    {
        //        string[] englishText = baseTranslationInEnglish.Split(':');
        //        TranslationViewModel translationViewModel = new TranslationViewModel(englishText[1], englishText[2]);
        //        TranslationViewModels.Add(translationViewModel);
        //    }

        //    TranslationListView.ItemsSource = TranslationViewModels;
        //}

        private async void ExportDataAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                string suggestedFileName = $"InvoicesNowTranslations_{CurrentCultureName}.json";
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

                        SaveAppBarButton.IsEnabled = false;
                    //DeleteAppBarButton.IsEnabled = false;

                    MainPage.NotifyUser("Export data. Please wait.", NotifyType.StatusMessage);
                    });

                    string jsonData = JsonConvert.SerializeObject(TranslationViewModels);

                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(storageFile);
                    // write to file
                    await FileIO.WriteTextAsync(storageFile, jsonData);
                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(storageFile);
                    if (status == FileUpdateStatus.Complete)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            MainPage.NotifyUser($"File '{storageFile.Name}' was saved.", NotifyType.StatusMessage);
                        });
                    }
                    else
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            MainPage.NotifyUser($"File '{storageFile.Name}' couldn't be saved.", NotifyType.ErrorMessage);
                        });
                    }

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ExportDataAppBarButton.IsEnabled = true;
                        ImportDataAppBarButton.IsEnabled = true;
                        ExportDataProgressRing.Visibility = Visibility.Collapsed;
                        ExportDataProgressRing.IsEnabled = false;
                        ExportDataProgressRing.IsActive = false;

                        SaveAppBarButton.IsEnabled = true;
                    //DeleteAppBarButton.IsEnabled = true;
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
                    List<TranslationViewModel> ImportedTranslationViewModels;
                    List<TranslationViewModel> CheckedImportedTranslationViewModels = new List<TranslationViewModel>();

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ExportDataAppBarButton.IsEnabled = false;
                        ImportDataAppBarButton.IsEnabled = false;
                        ExportDataProgressRing.Visibility = Visibility.Visible;
                        ExportDataProgressRing.IsEnabled = true;
                        ExportDataProgressRing.IsActive = true;

                        SaveAppBarButton.IsEnabled = false;
                    //DeleteAppBarButton.IsEnabled = false;
                });

                    try
                    {
                        string fileContent = await FileIO.ReadTextAsync(storageFile);
                        ImportedTranslationViewModels = JsonConvert.DeserializeObject<List<TranslationViewModel>>(fileContent);
                        foreach (TranslationViewModel ImportedTranslationViewModel in ImportedTranslationViewModels)
                        {
                            if (!string.IsNullOrEmpty(ImportedTranslationViewModel.EnglishText) && !string.IsNullOrEmpty(ImportedTranslationViewModel.TranslatedText))
                            {
                                CheckedImportedTranslationViewModels.Add(new TranslationViewModel(ImportedTranslationViewModel.EnglishText, ImportedTranslationViewModel.TranslatedText));
                            }
                        }

                        if (CheckedImportedTranslationViewModels.Count > 0)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                TranslationViewModels = CheckedImportedTranslationViewModels.ToList();
                                TranslationListView.ItemsSource = TranslationViewModels;

                                MainPage.NotifyUser($"Imported data from {storageFile.DisplayName}. Please save translations.", NotifyType.StatusMessage);
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                MainPage.NotifyUser($"File didn't contain translations. Try again.", NotifyType.ErrorMessage);
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
                            ExportDataProgressRing.Visibility = Visibility.Collapsed;
                            ExportDataProgressRing.IsEnabled = false;
                            ExportDataProgressRing.IsActive = false;

                            SaveAppBarButton.IsEnabled = true;
                        //DeleteAppBarButton.IsEnabled = true;
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

        private void HomeAppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is AppBarButton)
            {
                MainPage.GoToHomePage();
            }
        }

        private void FillTranslatedTextButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TranslatedStringTextBox.Text))
            {
                string[] paragraphs = TranslatedStringTextBox.Text.Trim().Split(new char[] { '|' },
                       StringSplitOptions.RemoveEmptyEntries);

                if (paragraphs.Count() != 25)
                {
                    MainPage.NotifyUser($"No matches found. Expected 25 paragraphs? Try again. Found {paragraphs.Count()}.", NotifyType.StatusMessage);
                    return;
                }

                TranslationListView.ItemsSource = null;

                int i = 0;
                foreach (var paragraph in paragraphs)
                {
                    TranslationViewModels[i].TranslatedText = paragraph.Trim();
                    i++;
                }

                TranslationListView.ItemsSource = TranslationViewModels;
                MainPage.NotifyUser("Done. Please save translations.", NotifyType.StatusMessage);
            }
            else
            {
                MainPage.NotifyUser("Please copy string in English. Go to Microsoft Bing Translator to translate! Translate to specific language. Copy translated string and paste it back. Press button 'Fill'.", NotifyType.StatusMessage);
            }
        }
    }
}