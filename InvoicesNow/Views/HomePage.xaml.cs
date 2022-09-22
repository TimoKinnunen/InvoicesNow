using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class HomePage : Page
    {
        MainPage MainPage { get; }

        CultureInfo CurrentCulture { get; } = CultureInfo.CurrentCulture;

        public HomePage()
        {
            InitializeComponent();

            Loaded += HomePage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentCultureTextBlock.Text = $"The current culture is {CurrentCulture.EnglishName} " +
                $"{CurrentCulture.NativeName} " +
                $"[{CurrentCulture.Name}] and " +
                $"currency symbol is [{CurrentCulture.NumberFormat.CurrencySymbol}], " +
                $"number decimal separator is '{CurrentCulture.NumberFormat.NumberDecimalSeparator}'.";
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
            // code here
        }
    }
}

