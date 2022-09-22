using System;

namespace InvoicesNow.Helpers
{
    public sealed class HelpToFileSize
    {
        public static string ToFileSize(ulong source)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(source);

            if (bytes >= Math.Pow(byteConversion, 3)) //GB Range
            {
                return $"{Math.Round(bytes / Math.Pow(byteConversion, 3), 2)} GB";
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) //MB Range
            {
                return $"{Math.Round(bytes / Math.Pow(byteConversion, 2), 2)} MB";
            }
            else if (bytes >= byteConversion) //KB Range
            {
                return $"{Math.Round(bytes / byteConversion, 0)} kB";
            }
            else //Bytes
            {
                return $"{bytes} Bytes";
            }
        }
    }
}