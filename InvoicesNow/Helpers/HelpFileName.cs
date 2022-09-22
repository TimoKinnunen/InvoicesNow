using System;
using System.Globalization;

namespace InvoicesNow.Helpers
{
    public static class HelpFileName
    {
        public static string AddDateTimeNowToFileName(string fileName)
        {
            var dateTimeNow = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            return $"{fileName}_{dateTimeNow}";
        }
    }
}
