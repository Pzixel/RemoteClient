using System.Globalization;

namespace WcfRestClient.Helpers
{
    public static class LibraryCulture
    {
        public const string DatetimeFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        public static CultureInfo CultureInfo { get; }

        static LibraryCulture()
        {
            var myCulture = new CultureInfo(CultureInfo.InvariantCulture.LCID);
            var dtFormat = myCulture.DateTimeFormat;
            dtFormat.FullDateTimePattern = DatetimeFormat;

            CultureInfo = myCulture;
        }
    }
}
