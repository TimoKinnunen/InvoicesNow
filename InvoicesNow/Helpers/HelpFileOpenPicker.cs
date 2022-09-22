using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace InvoicesNow.Helpers
{
    public class HelpFileOpenPicker
    {
        public static async Task<StorageFile> PickJsonFileAsync()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List
            };
            fileOpenPicker.FileTypeFilter.Add(".json");
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fileOpenPicker.CommitButtonText = "Pick .json-file";
            fileOpenPicker.SettingsIdentifier = "Import data";

            return await fileOpenPicker.PickSingleFileAsync();
        }
    }
}