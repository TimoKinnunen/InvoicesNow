using InvoicesNow.Helpers;
using InvoicesNow.Models;
using InvoicesNow.Printing.ViewModels;
using InvoicesNow.Printing.Views;
using InvoicesNow.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Printing;

namespace InvoicesNow.Printing
{
    public class InvoicePrintHelper
    {
        /// <summary>
        ///  A reference back to the main page used to access mainPage.NotifyUser()
        /// </summary>
        MainPage MainPage { get; }

        /// <summary>
        /// print preview page's size change event will update layout too often
        /// every snall increment on screen and pagination is needed thousand times
        /// avoid race to update the layout
        /// allow pagination happen only every 2 seconds
        /// </summary>
        DispatcherTimer PaginateTextTimer { get; }

        int CounterIntervalSeconds { get; set; }

        /// <summary>
        ///  A reference back to the print page used to access XAML elements on the PrintInvoicePage page
        ///  On this page we have GetPrintCanvas() and GetMeasurePrintCanvas()
        /// </summary>
        PrintInvoicePage PrintInvoicePage { get; }

        /// <summary>
        ///  A hidden canvas used to hold pages we wish to print
        /// </summary>
        protected Canvas GetPrintCanvas()
        {
            return PrintInvoicePage.FindName("PrintCanvas") as Canvas;
        }

        /// <summary>
        ///  A hidden canvas used to hold pages we wish to measure
        ///  RichTextBlock and RichTextBlockOverflow are not suitable here because they handle text(for text they are superb) and not grid with columns
        /// </summary>
        protected Canvas GetMeasurePrintCanvas()
        {
            return PrintInvoicePage.FindName("MeasurePrintCanvas") as Canvas;
        }

        MeasureInvoicePage InvoicePage { get; set; }

        MeasureInvoicePage MeasureInvoicePage { get; set; }

        /// <summary>
        /// Create an invoice.
        /// </summary>
        Invoice Invoice { get; set; }

        /// <summary>
        /// Use this as name "Invoice " + invoiceNumber for the print task.
        /// </summary>
        int InvoiceNumber { get; set; }

        List<UIElement> PrintPreviewPages { get; set; }

        PrintManager PrintManager { get; set; }
        PrintDocument PrintDocument { get; set; }
        IPrintDocumentSource PrintDocumentSource { get; set; }

        protected double ApplicationContentMarginTop { get; set; } = 0.03;
        protected double ApplicationContentMarginLeft { get; set; } = 0.075;

        List<PrintInvoiceItemViewModel> PrintInvoiceItemViewModels { get; set; }

        List<TranslationViewModel> TranslationViewModels { get; set; }

        int InvoicePageNumber { get; set; } = 1;

        string InvoiceText { get; set; } = "Invoice";

        Guid SellerId { get; set; }

        BitmapImage invoicePageBitmapImage { get; set; }

        bool pageSizeIsTooSmall { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printInvoicePage">The invoice Guid constructing us</param>
        public InvoicePrintHelper(PrintInvoicePage printInvoicePage)
        {
            PrintInvoicePage = printInvoicePage; //we get our hands on PrintCanvas and MeasurePrintCanvas

            PrintPreviewPages = new List<UIElement>();

            PaginateTextTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

            MainPage = MainPage.CurrentMainPage;

            PaginateTextTimer.Tick += PaginateTextTimer_Tick;
        }

        private void PaginateTextTimer_Tick(object sender, object e)
        {
            CounterIntervalSeconds++;
            if (CounterIntervalSeconds >= 2)
            {
                CounterIntervalSeconds = 0;
                //after 2 seconds stop the timer
                PaginateTextTimer.Stop();
            }
        }

        /// <summary>
        /// Method that will generate print content for the invoice
        /// </summary>
        /// <param name="invoiceId">The invoice to print</param>
        public async virtual void GetDataForPrintContent(Guid invoiceId)
        {
            Invoice = await App.Repository.Invoices.GetInvoiceAsync(invoiceId).ConfigureAwait(false);
            if (Invoice == null)
            {
                MainPage.NotifyUser("Invoice is missing. Try again.", NotifyType.ErrorMessage);
            }

            //used as print task name for this invoice
            InvoiceNumber = Invoice.InvoiceNumber;

            TranslationViewModels = await HelpTranslationsFromLocalFolder.GetCurrentCultureTranslationsAsync().ConfigureAwait(false);

            TranslationViewModel translationViewModel = TranslationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice");
            if (translationViewModel != null)
            {
                InvoiceText = translationViewModel.TranslatedText;
            }

            SellerId = Invoice.SellerId;

            await MainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                invoicePageBitmapImage = await GetExistingSellerLogotype().ConfigureAwait(true);
            });
        }


        /// <summary>
        /// This function registers the app for printing with Windows and sets up the necessary event handlers for the print process.
        /// </summary>
        public virtual void RegisterForPrinting()
        {
            // Register for PrintTaskRequested event
            PrintManager = PrintManager.GetForCurrentView();
            PrintManager.PrintTaskRequested += PrintTaskRequested;

            // Build a PrintDocument and register for callbacks
            PrintDocument = new PrintDocument();
            PrintDocumentSource = PrintDocument.DocumentSource;
            PrintDocument.Paginate += Paginate;
            PrintDocument.AddPages += PrintDocument_AddPages;
            PrintDocument.GetPreviewPage += GetPreviewPage;
        }

        /// <summary>
        /// This function unregisters the app for printing with Windows.
        /// </summary>
        public virtual void UnregisterForPrinting()
        {
            if (PrintDocument == null)
            {
                return;
            }

            PrintDocument.Paginate -= Paginate;
            PrintDocument.AddPages += PrintDocument_AddPages;
            PrintDocument.GetPreviewPage -= GetPreviewPage;

            // Remove the handler for printing initialization.
            PrintManager = PrintManager.GetForCurrentView();
            PrintManager.PrintTaskRequested -= PrintTaskRequested;

            GetPrintCanvas().Children.Clear();
            GetMeasurePrintCanvas().Children.Clear();
        }

        /// <summary>
        /// This is the event handler for PrintDocument.AddPages. It provides all pages to be printed, in the form of
        /// UIElements, to an instance of PrintDocument. PrintDocument subsequently converts the UIElements
        /// into a pages that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Add page event arguments containing a print task options reference</param>
        protected virtual void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            // Loop over all of the preview pages and add each one to  add each page to be printied
            for (int i = 0; i < PrintPreviewPages.Count; i++)
            {
                // We should have all pages ready at this point...
                PrintDocument.AddPage(PrintPreviewPages[i]);
            }

            PrintDocument printDoc = sender as PrintDocument;

            // Indicate that all of the print pages have been provided
            printDoc.AddPagesComplete();
        }

        #region Showing the print dialog

        /// <summary>
        /// This is the event handler for PrintManager.PrintTaskRequested.
        /// </summary>
        /// <param name="sender">PrintManager</param>
        /// <param name="e">PrintTaskRequestedEventArgs </param>
        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null;
            printTask = e.Request.CreatePrintTask($"{InvoiceText} {InvoiceNumber}", sourceRequested =>
            {
                PrintTaskOptions printTaskOptions = printTask.Options;

                PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(printTask.Options);

                ICollection<string> displayedOptions = printDetailedOptions.DisplayedOptions;

                displayedOptions.Clear();
                displayedOptions.Add(StandardPrintTaskOptions.MediaSize);
                displayedOptions.Add(StandardPrintTaskOptions.Orientation);
                displayedOptions.Add(StandardPrintTaskOptions.Copies);

                // Preset the default value of the printer option
                printTaskOptions.MediaSize = PrintMediaSize.IsoA4;
                printTaskOptions.Orientation = PrintOrientation.Portrait;

                // Print Task event handler is invoked when the print job is completed.
                printTask.Completed += async (s, args) =>
                {
                    await PrintInvoicePage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // Notify the user when the print operation fails.
                        if (args.Completion == PrintTaskCompletion.Failed)
                        {
                            MainPage.NotifyUser("Failed to print.", NotifyType.ErrorMessage);
                        }

                        if (args.Completion == PrintTaskCompletion.Submitted)
                        {
                            MainPage.NotifyUser("Print task was submitted.", NotifyType.StatusMessage);
                        }

                        if (args.Completion == PrintTaskCompletion.Abandoned)
                        {
                            MainPage.NotifyUser("Print task was abandoned.", NotifyType.StatusMessage);
                        }

                        if (args.Completion == PrintTaskCompletion.Canceled)
                        {
                            MainPage.NotifyUser("Print task was canceled.", NotifyType.StatusMessage);
                        }
                    });
                };

                sourceRequested.SetSource(PrintDocumentSource);
            });
        }
        #endregion Showing the print dialog

        #region Print preview
        private async void Paginate(object sender, PaginateEventArgs e)
        {
            // PrintDocument: Print preview page's size change event creates a race to update the layout
            // Allow pagination every 2 seconds only and avoid race to update the layout
            if (PaginateTextTimer.IsEnabled)
            {
                return;
            }

            CounterIntervalSeconds = 0;
            if (!PaginateTextTimer.IsEnabled)
            {
                PaginateTextTimer.Start();
                // now paginateTextTimer.IsEnabled for 2 seconds
                // every call to paginate method during 2 seconds will return (bounce back)
            }

            //Data for InvoicePage ListView items
            PrintInvoiceItemViewModels = new List<PrintInvoiceItemViewModel>();
            foreach (InvoiceItem invoiceItem in Invoice.InvoiceItems.OrderBy(v => v.Name).ThenBy(v => v.Quantity).ThenBy(v => v.Tax).ThenBy(v => v.Price))
            {
                PrintInvoiceItemViewModels.Add(NewPrintInvoiceItemViewModel(invoiceItem));
            }

            lock (PrintPreviewPages)
            {
                PrintDocument.SetPreviewPageCount(0, PreviewPageCountType.Intermediate);

                // Clear the cache of preview pages
                PrintPreviewPages.Clear();

                // Clear the print canvas of preview pages
                GetPrintCanvas().Children.Clear();

                InvoicePageNumber = 1;

                // Get the PrintTaskOptions
                PrintTaskOptions printTaskOptions = e.PrintTaskOptions;

                // Get the page description to determine how big the page is
                PrintPageDescription printPageDescription = printTaskOptions.GetPageDescription(0);

                if (PrintInvoiceItemViewModels.Count == 0)
                {
                    // We know there is at least one page to be printed. Add the first page.
                    AddInvoicePageToPrintPreviewPages(printPageDescription, 0);

                    // Report the number of preview pages created
                    PrintDocument.SetPreviewPageCount(PrintPreviewPages.Count, PreviewPageCountType.Intermediate);

                    return;
                }

                //have more to add
                while (PrintInvoiceItemViewModels.Count > 0)
                {
                    int itemsCountToAdd = MeasureInvoicePageToPrintPreviewPages(printPageDescription);

                    if (itemsCountToAdd < 0)
                    {
                        pageSizeIsTooSmall = true;
                        break;
                    }

                    // We know there is at least one page to be printed. Add the first page.
                    AddInvoicePageToPrintPreviewPages(printPageDescription, itemsCountToAdd);
                }

                // Report the number of preview pages created
                PrintDocument.SetPreviewPageCount(PrintPreviewPages.Count, PreviewPageCountType.Final);
            }

            if (pageSizeIsTooSmall)
            {
                pageSizeIsTooSmall = false;
                MessageDialog messageDialog = new MessageDialog("Paper size is too small. Select a bigger paper size to print!", "Paper size");
                await messageDialog.ShowAsync();
            }

        }

        /// <summary>
        /// Count how many rows does fit on one page
        /// </summary>
        /// <param name="printPageDescription"></param>
        /// <returns></returns>
        int MeasureInvoicePageToPrintPreviewPages(PrintPageDescription printPageDescription)
        {
            int itemsCountToAdd = 0;

            List<PrintInvoiceItemViewModel> thisPrintInvoiceItemViewModels = new List<PrintInvoiceItemViewModel>();
            foreach (PrintInvoiceItemViewModel printInvoiceItemViewModel in PrintInvoiceItemViewModels)
            {

                // Clear the measure canvas of preview pages
                GetMeasurePrintCanvas().Children.Clear();

                MeasureInvoicePage = new MeasureInvoicePage();

                if (PrintPreviewPages.Count == 0)
                {
                }
                else
                {
                    //this page is not first page
                    //Grid headerGrid = measureInvoicePage.FindName("HeaderGrid") as Grid;
                    //headerGrid.Visibility = Visibility.Collapsed;
                    Grid sellerAccountGrid = MeasureInvoicePage.FindName("SellerAccountGrid") as Grid;
                    sellerAccountGrid.Visibility = Visibility.Collapsed;
                    Grid headerMoneyGrid = MeasureInvoicePage.FindName("HeaderMoneyGrid") as Grid;
                    headerMoneyGrid.Visibility = Visibility.Collapsed;
                    Grid buyerGrid = MeasureInvoicePage.FindName("BuyerGrid") as Grid;
                    buyerGrid.Visibility = Visibility.Collapsed;
                    Grid sellerGrid = MeasureInvoicePage.FindName("SellerGrid") as Grid;
                    sellerGrid.Visibility = Visibility.Collapsed;
                }

                //seller's logotype
                Image SellerLogotypeImage = MeasureInvoicePage.FindName("SellerLogotypeImage") as Image;
                if (SellerLogotypeImage != null)
                {
                    SellerLogotypeImage.Visibility = Visibility.Visible;
                    SellerLogotypeImage.Source = invoicePageBitmapImage;
                }

                // header data for InvoicePage
                StackPanel printInvoiceStackPanel = MeasureInvoicePage.FindName("PrintInvoiceStackPanel") as StackPanel;
                PrintInvoiceViewModel PrintInvoiceViewModel = NewPrintInvoiceViewModel(Invoice);
                printInvoiceStackPanel.DataContext = TranslatePrintInvoiceViewModel(PrintInvoiceViewModel, TranslationViewModels);

                //header for InvoicePage ListView
                ListView printInvoiceItemListView = MeasureInvoicePage.FindName("PrintInvoiceItemListView") as ListView;
                printInvoiceItemListView.DataContext = TranslatePrintInvoiceItemHeaderViewModel(TranslationViewModels);

                //add one item and measure if content of page can still fit on PrintableArea?
                thisPrintInvoiceItemViewModels.Add(printInvoiceItemViewModel);
                printInvoiceItemListView.ItemsSource = thisPrintInvoiceItemViewModels;

                // XAML element that is used to represent to "printing page"
                FrameworkElement page = MeasureInvoicePage;

                // Set "paper" width
                page.Width = printPageDescription.PageSize.Width;
                page.Height = printPageDescription.PageSize.Height;

                Grid printableArea = page.FindName("PrintableArea") as Grid;

                // Get the margins size
                // If the ImageableRect is smaller than the app provided margins use the ImageableRect
                double marginWidth = Math.Max(printPageDescription.PageSize.Width - printPageDescription.ImageableRect.Width, printPageDescription.PageSize.Width * ApplicationContentMarginLeft * 2);
                double marginHeight = Math.Max(printPageDescription.PageSize.Height - printPageDescription.ImageableRect.Height, printPageDescription.PageSize.Height * ApplicationContentMarginTop * 2);

                // Set-up "printable area" on the "paper"
                printableArea.Width = page.Width - marginWidth;
                printableArea.Height = page.Height - marginHeight;

                // Add the (newley created) page to the print canvas which is part of the visual tree and force it to go through visual tree
                GetMeasurePrintCanvas().Children.Add(page);
                GetMeasurePrintCanvas().InvalidateMeasure();
                GetMeasurePrintCanvas().UpdateLayout();

                StackPanel footerStackPanel = page.FindName("FooterStackPanel") as StackPanel;

                //measure
                if ((printInvoiceStackPanel.ActualHeight + footerStackPanel.ActualHeight) < printableArea.Height)
                {
                    itemsCountToAdd++;
                }
                else
                {
                    itemsCountToAdd--;
                    break;
                }
            }

            return itemsCountToAdd;
        }

        /// <summary>
        /// This function creates and adds one print preview page to the internal cache of print preview
        /// pages stored in printPreviewPages.
        /// </summary>
        /// <param name="printPageDescription">Printer's page description</param>
        protected virtual void AddInvoicePageToPrintPreviewPages(PrintPageDescription printPageDescription, int itemsCountToAdd)
        {
            InvoicePage = new MeasureInvoicePage();

            if (PrintPreviewPages.Count == 0)
            {

            }
            else
            {
                //this page is not first page
                //Grid headerGrid = invoicePage.FindName("HeaderGrid") as Grid;
                //headerGrid.Visibility = Visibility.Collapsed;
                Grid sellerAccountGrid = InvoicePage.FindName("SellerAccountGrid") as Grid;
                sellerAccountGrid.Visibility = Visibility.Collapsed;
                Grid headerMoneyGrid = InvoicePage.FindName("HeaderMoneyGrid") as Grid;
                headerMoneyGrid.Visibility = Visibility.Collapsed;
                Grid buyerGrid = InvoicePage.FindName("BuyerGrid") as Grid;
                buyerGrid.Visibility = Visibility.Collapsed;
                Grid sellerGrid = InvoicePage.FindName("SellerGrid") as Grid;
                sellerGrid.Visibility = Visibility.Collapsed;
            }

            //seller's logotype
            Image SellerLogotypeImage = InvoicePage.FindName("SellerLogotypeImage") as Image;
            if (SellerLogotypeImage != null)
            {
                SellerLogotypeImage.Visibility = Visibility.Visible;
                SellerLogotypeImage.Source = invoicePageBitmapImage;
            }

            // header data for InvoicePage
            StackPanel printInvoiceStackPanel = InvoicePage.FindName("PrintInvoiceStackPanel") as StackPanel;
            PrintInvoiceViewModel PrintInvoiceViewModel = NewPrintInvoiceViewModel(Invoice);
            printInvoiceStackPanel.DataContext = TranslatePrintInvoiceViewModel(PrintInvoiceViewModel, TranslationViewModels);

            //header for InvoicePage ListView
            ListView printInvoiceItemListView = InvoicePage.FindName("PrintInvoiceItemListView") as ListView;
            printInvoiceItemListView.DataContext = TranslatePrintInvoiceItemHeaderViewModel(TranslationViewModels);

            int i = 0;
            //Data for InvoicePage ListView items
            List<PrintInvoiceItemViewModel> thisPagePrintInvoiceItemViewModels = new List<PrintInvoiceItemViewModel>();
            List<PrintInvoiceItemViewModel> remainingPrintInvoiceItemViewModels = new List<PrintInvoiceItemViewModel>();
            foreach (PrintInvoiceItemViewModel printInvoiceItemViewModel in PrintInvoiceItemViewModels)
            {
                if (i <= itemsCountToAdd)
                {
                    thisPagePrintInvoiceItemViewModels.Add(printInvoiceItemViewModel);
                }
                else
                {
                    //these go to a new page
                    remainingPrintInvoiceItemViewModels.Add(printInvoiceItemViewModel);
                }
                i++;
            }

            PrintInvoiceItemViewModels = remainingPrintInvoiceItemViewModels;

            printInvoiceItemListView.ItemsSource = thisPagePrintInvoiceItemViewModels;

            // XAML element that is used to represent to "printing page"
            FrameworkElement invoicePage = InvoicePage;

            // Set "paper" width
            invoicePage.Width = printPageDescription.PageSize.Width;
            invoicePage.Height = printPageDescription.PageSize.Height;

            Grid printableArea = invoicePage.FindName("PrintableArea") as Grid;

            // Get the margins size
            // If the ImageableRect is smaller than the app provided margins use the ImageableRect
            double marginWidth = Math.Max(printPageDescription.PageSize.Width - printPageDescription.ImageableRect.Width, printPageDescription.PageSize.Width * ApplicationContentMarginLeft * 2);
            double marginHeight = Math.Max(printPageDescription.PageSize.Height - printPageDescription.ImageableRect.Height, printPageDescription.PageSize.Height * ApplicationContentMarginTop * 2);

            // Set-up "printable area" on the "paper"
            printableArea.Width = invoicePage.Width - marginWidth;
            printableArea.Height = invoicePage.Height - marginHeight;

            TextBlock pageNumberTextBlock = invoicePage.FindName("PageNumberTextBlock") as TextBlock;

            pageNumberTextBlock.Text = $"{PrintInvoiceViewModel.TranslatePageNumber} {InvoicePageNumber++}.";

            // Add the (newley created) page to the print canvas which is part of the visual tree and force it to go through visual tree.
            GetPrintCanvas().Children.Add(invoicePage);
            GetPrintCanvas().InvalidateMeasure();
            GetPrintCanvas().UpdateLayout();

            // Add the page to the page preview collection
            PrintPreviewPages.Add(invoicePage);
        }

        private void GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            if (PrintPreviewPages.Count > 0)
            {
                try
                {
                    PrintDocument.SetPreviewPage(e.PageNumber, PrintPreviewPages[e.PageNumber - 1]);
                }
                catch
                {
                }
            }
        }
        #endregion Print preview

        internal async Task ShowPrintUIAsync()
        {
            // Catch and print out any errors reported
            try
            {
                await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception e)
            {
                MainPage.NotifyUser($"Error printing: {e.Message}, hr={e.HResult}", NotifyType.ErrorMessage);
            }
        }

        private static PrintInvoiceItemHeaderViewModel TranslatePrintInvoiceItemHeaderViewModel(List<TranslationViewModel> translationViewModels)
        {
            PrintInvoiceItemHeaderViewModel printInvoiceItemHeaderViewModel = new PrintInvoiceItemHeaderViewModel();

            TranslationViewModel translationViewModel;
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Name");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderName = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Quantity");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderQuantity = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Tax");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Price");
            if (translationViewModel != null)
            {
                printInvoiceItemHeaderViewModel.TranslateHeaderPrice = translationViewModel.TranslatedText;
            }

            return printInvoiceItemHeaderViewModel;
        }

        private static PrintInvoiceViewModel TranslatePrintInvoiceViewModel(PrintInvoiceViewModel printInvoiceViewModel, List<TranslationViewModel> translationViewModels)
        {
            TranslationViewModel translationViewModel;
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateInvoice = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice number");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateInvoiceNumber = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Please pay latest at due date");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.InvoiceInfoToBuyer = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Invoice date");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateInvoiceDate = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Due date");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateDueDate = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Amount to pay");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateAmountToPay = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Payment to account");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslatePaymentToAccount = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "SWIFT/BIC");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSWIFTBIC = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "IBAN");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateIBAN = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Total including tax");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateTotalIncludingTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Total excluding tax");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateTotalExcludingTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Total tax");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateTotalTax = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's name");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerName = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's e-mail address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerEmailAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Buyer's phonenumber");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateBuyerPhonenumber = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's name");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerName = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's e-mail address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerEmailAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's address");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerAddress = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Seller's phonenumber");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslateSellerPhonenumber = translationViewModel.TranslatedText;
            }
            translationViewModel = translationViewModels.FirstOrDefault(t => t.EnglishText == "Page number");
            if (translationViewModel != null)
            {
                printInvoiceViewModel.TranslatePageNumber = translationViewModel.TranslatedText;
            }

            return printInvoiceViewModel;
        }

        private static PrintInvoiceViewModel NewPrintInvoiceViewModel(Invoice invoice)
        {
            return new PrintInvoiceViewModel
            {
                PrintInvoiceViewModelId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,

                InvoiceInfoToBuyer = invoice.InvoiceInfoToBuyer,

                TotalIncludingTax = invoice.TotalIncludingTax,
                TotalExcludingTax = invoice.TotalExcludingTax,
                TotalTax = invoice.TotalTax,

                NetPaymentDueDate = invoice.NetPaymentDueDate,
                NetPaymentTermDays = invoice.NetPaymentTermDays,

                SellerName = invoice.SellerName,
                SellerEmail = invoice.SellerEmail,
                SellerAddress = invoice.SellerAddress,
                SellerPhonenumber = invoice.SellerPhonenumber,
                SellerAccount = invoice.SellerAccount,
                SellerSWIFTBIC = invoice.SellerSWIFTBIC,
                SellerIBAN = invoice.SellerIBAN,
                SellerId = invoice.SellerId,

                BuyerName = invoice.BuyerName,
                BuyerEmail = invoice.BuyerEmail,
                BuyerAddress = invoice.BuyerAddress,
                BuyerPhonenumber = invoice.BuyerPhonenumber,
                BuyerId = invoice.BuyerId,
            };
        }

        private static PrintInvoiceItemViewModel NewPrintInvoiceItemViewModel(InvoiceItem invoiceItem)
        {
            return new PrintInvoiceItemViewModel
            {
                PrintInvoiceItemViewModelId = invoiceItem.InvoiceItemId,
                Name = invoiceItem.Name,
                Quantity = invoiceItem.Quantity,
                Tax = invoiceItem.Tax,
                Price = invoiceItem.Price,
            };
        }

        private async Task<BitmapImage> GetExistingSellerLogotype()
        {
            BitmapImage bitmapImage = new BitmapImage();
            StorageFolder logotypesStorageFolder = await GetLogotypesStorageFolder();
            IReadOnlyList<StorageFile> fileList = await logotypesStorageFolder.GetFilesAsync();
            StorageFile existingLogotype = fileList.FirstOrDefault(o => o.DisplayName.ToUpper() == SellerId.ToString().ToUpper());
            if (existingLogotype != null)
            {
                RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromFile(existingLogotype);
                bitmapImage.SetSource(await stream.OpenReadAsync());
            }
            return bitmapImage;
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