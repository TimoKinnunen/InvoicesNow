using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow.Views
{
    public sealed partial class AboutPage : Page
    {
        MainPage MainPage { get; }

        public AboutPage()
        {
            InitializeComponent();

            SizeChanged += AboutPage_SizeChanged;

            Loaded += AboutPage_Loaded;

            MainPage = MainPage.CurrentMainPage;
        }

        private void AboutPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPageContentStackPanelWidth();
        }

        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetPageContentStackPanelWidth();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // code here
            Package package = Package.Current;

            InvoicesNowImage.Source = new BitmapImage(package.Logo);

            AppDisplayNameTextBlock.Text = package.DisplayName;

            PublisherTextBlock.Text = package.PublisherDisplayName;

            PackageId packageId = package.Id;

            PackageVersion packageVersion = packageId.Version;

            VersionTextBlock.Text = $"Version {packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
            // code here
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // code here
            // code here
        }

        private void SetPageContentStackPanelWidth()
        {
            PageContentStackPanel.Width = ActualWidth -
                PageContentScrollViewer.Margin.Left -
                PageContentScrollViewer.Padding.Right;
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

