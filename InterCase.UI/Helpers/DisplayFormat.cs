using System.Globalization;
using InterCase.Common;

namespace InterCase.UI.Helpers
{
    public static class DisplayFormat
    {
        private static readonly TimeZoneInfo TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

        public static DateTime AsUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        public static string Date(DateTime value) =>
            ToTurkeyTime(value).ToString(UiText.IsTurkish ? "dd.MM.yyyy" : "yyyy-MM-dd", CultureInfo.InvariantCulture);

        public static string Clock(DateTime value) =>
            ToTurkeyTime(value).ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        public static string Iso(DateTime value) =>
            AsUtc(value).ToString("O", CultureInfo.InvariantCulture);

        public static string ShortId(Guid id) => id.ToString("N")[^8..];

        public static string StatusClass(OrderStatus status) => $"status-{status.ToString().ToLowerInvariant()}";

        private static DateTime ToTurkeyTime(DateTime value) =>
            TimeZoneInfo.ConvertTimeFromUtc(AsUtc(value), TurkeyTimeZone);
    }
}
