using InvoicesNow.Data;
using InvoicesNow.Printing.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace InvoicesNow.Helpers
{
    public static class HelpTranslationsFromLocalFolder
    {
        public async static Task<List<TranslationViewModel>> GetCurrentCultureTranslationsAsync()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            List<TranslationViewModel> TranslationViewModels = new List<TranslationViewModel>();

            string fileName = $"InvoicesNowTranslations_{currentCulture.Name}.json";

            try
            {
                StorageFile storageFile = await App.LocalFolder.GetFileAsync(fileName);
                if (storageFile != null)
                {
                    string jsonData = await FileIO.ReadTextAsync(storageFile);
                    TranslationViewModels = await Task.Run(() => JsonConvert.DeserializeObject<List<TranslationViewModel>>(jsonData)).ConfigureAwait(false);
                }
            }
            catch (FileNotFoundException)
            {
                TranslationViewModels.Clear();
                foreach (string baseTranslationInEnglish in BaseTranslations.EnglishBaseTranslations)
                {
                    string[] englishText = baseTranslationInEnglish.Split(':');
                    TranslationViewModel translationViewModel = new TranslationViewModel(englishText[1], englishText[2]);
                    TranslationViewModels.Add(translationViewModel);
                }
            }

            return TranslationViewModels;
        }
    }
}
