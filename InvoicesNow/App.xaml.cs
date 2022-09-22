using InvoicesNow.Repository;
using InvoicesNow.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.Storage;
using Windows.System.Display;
using Windows.System.UserProfile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace InvoicesNow
{
    sealed partial class App : Application
    {
        /// <summary>
        /// Pipeline for interacting with backend service or database.
        /// </summary>
        public static RepositoryInvoicesNow Repository { get; set; }

        //don't let screensaver pop up when using this app
        DisplayRequest MediaElementDisplayRequest { get; } = new DisplayRequest();

        // Singleton
        public static ApplicationDataContainer LocalSettings { get; set; }
        public static StorageFolder LocalFolder { get; set; }

        public static Guid LatestVisitedInvoiceId { get; set; } = Guid.NewGuid(); // set in InvoiceListPage and InvoicePage and read in TranslationsPage 

        public static bool UseSerieAsInvoiceNumber { get; set; }

        public static int SerieStartsWith { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            LocalSettings = ApplicationData.Current.LocalSettings;
            LocalFolder = ApplicationData.Current.LocalFolder;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            ApplicationLanguages.PrimaryLanguageOverride = GlobalizationPreferences.Languages[0];

            SetLatestUsedCultureName();

            GetUseSerieAsInvoiceNumber();

            UseSqlite();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (MediaElementDisplayRequest != null)
                {
                    MediaElementDisplayRequest.RequestActive();
                }

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            if (MediaElementDisplayRequest != null)
            {
                MediaElementDisplayRequest.RequestRelease();
            }

            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            App.LocalSettings.Values["LatestUsedCultureName"] = currentCulture.Name;

            deferral.Complete();
        }

        private void UseSqlite()
        {
            //string demoDatabasePath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\InvoicesNow.db");
            string databasePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "InvoicesNow.db");
            //if (!File.Exists(databasePath))
            //{
            //    File.Copy(demoDatabasePath, databasePath);
            //}

            //need these for ".UseSqlite("Data Source="..."
            //Install - Package Microsoft.EntityFrameworkCore
            //Install - Package Microsoft.EntityFrameworkCore.Design
            //Install - Package Microsoft.EntityFrameworkCore.Tools
            //Install - Package Microsoft.EntityFrameworkCore.Sqlite

            DbContextOptionsBuilder<InvoicesNowDbContext> dbOptions =
                new DbContextOptionsBuilder<InvoicesNowDbContext>().UseSqlite("Data Source=" + databasePath);

            Repository = new SqlInvoicesNow(dbOptions);
        }

        private void SetLatestUsedCultureName()
        {
            object value = LocalSettings.Values["LatestUsedCultureName"];
            if (value != null)
            {
                try
                {
                    string currentCultureName = value.ToString();
                    CultureInfo userCulture = new CultureInfo(currentCultureName);
                    CultureInfo.CurrentCulture = userCulture;
                    CultureInfo.CurrentUICulture = userCulture;
                }
                catch
                {
                }
            }
        }

        private void GetUseSerieAsInvoiceNumber()
        {
            object useSerie = App.LocalSettings.Values["UseSerieAsInvoiceNumber"];
            if (useSerie != null)
            {
                bool useSerieAsInvoiceNumber = bool.Parse(useSerie.ToString());
                App.UseSerieAsInvoiceNumber = useSerieAsInvoiceNumber;
            }
        }
    }
}
