namespace InvoicesNow.Data
{
    //InvoicePage.xaml
    internal class BaseTranslations
    {
        /// <summary>
        /// Base (paragraphs) in English for translations.
        /// </summary>
        internal static readonly string[] EnglishBaseTranslations =
        {
            "TranslateInvoiceTextBlock:Invoice:Invoice",
            "TranslateInvoiceNumberTextBlock:Invoice number:Invoice number",
            "TranslateInvoiceDateTextBlock:Invoice date:Invoice date",
            "InviceInfoToBuyerTextBlock:Please pay latest at due date:Please pay latest at due date",
            "TranslateDueDateTextBlock:Due date:Due date",
            "TranslateAmountToPayTextBlock:Amount to pay:Amount to pay",
            "TranslatePaymentToAccountTextBlock:Payment to account:Payment to account",
            "TranslateSWIFTBICTextBlock:SWIFT/BIC:SWIFT/BIC",
            "TranslateIBANTextBlock:IBAN:IBAN",
            "TranslateTotalIncludingTaxTextBlock:Total including tax:Total including tax",
            "TranslateTotalExcludingTaxTextBlock:Total excluding tax:Total excluding tax",
            "TranslateTotalTaxTextBlock:Total tax:Total tax",
            "TranslateBuyerNameTextBlock:Buyer's name:Buyer's name",
            "TranslateBuyerEmailAddressTextBlock:Buyer's e-mail address:Buyer's e-mail address",
            "TranslateBuyerAddressTextBlock:Buyer's address:Buyer's address",
            "TranslateBuyerPhonenumberTextBlock:Buyer's phonenumber:Buyer's phonenumber",
            "TranslateSellerNameTextBlock:Seller's name:Seller's name",
            "TranslateSellerEmailAddressTextBlock:Seller's e-mail address:Seller's e-mail address",
            "TranslateSellerAddressTextBlock:Seller's address:Seller's address",
            "TranslateSellerPhonenumberTextBlock:Seller's phonenumber:Seller's phonenumber",
            "TranslatePageNumberTextBlock:Page number:Page number",

            "TableHeaderNameTextBlock:Name:Name",
            "TableHeaderQuantityTextBlock:Quantity:Quantity",
            "TableHeaderTaxTextBlock:Tax:Tax",
            "TableHeaderPriceTextBlock:Price:Price",
        };

        /// <summary>
        /// This string is pasted to Bing Translator field.
        /// And translated result is copied to this app.
        /// </summary>
        internal const string EnglishBaseTranslationsString = 
            "Invoice|Invoice number|Invoice date|Please pay latest at due date|Due date|Amount to pay|Payment to account|SWIFT/BIC|IBAN|Total including tax|Total excluding tax|Total tax|Buyer's name|Buyer's e-mail address|Buyer's address|Buyer's phonenumber|Seller's name|Seller's e-mail address|Seller's address|Seller's phonenumber|Page number|Name|Quantity|Tax|Price|";
    }
}
