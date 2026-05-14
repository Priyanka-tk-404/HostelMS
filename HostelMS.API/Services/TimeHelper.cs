namespace HostelMS.API.Services
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo IST =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");

        public static DateTime NowIST() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

        public static DateTime ToIST(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(
            utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc), IST);
    }
}
