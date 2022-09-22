namespace InvoicesNow.Printing.ViewModels
{
    public class TranslationViewModel
    {
        public TranslationViewModel(string englishText, string translatedText)
        {
            EnglishText = englishText;
            TranslatedText = translatedText;
        }

        public string EnglishText { get; set; }
        public string TranslatedText { get; set; }
    }
}
