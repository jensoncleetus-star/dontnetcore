namespace QuickSoft.Helpers
{
    public class Alert
    {
        public const string TempDataKey = "TempDataAlerts";
        public string AlertStyle { get; set; }
        public string Message { get; set; }
        public bool Dismissable { get; set; }
        public string Heading { get; set; }
        public string AlertIcon { get; set; }
    }
    public static class AlertStyles
    {
        public const string Success = "success";
        public const string Information = "info";
        public const string Warning = "warning";
        public const string Danger = "danger";
    }

    public static class AlertIcons
    {
        public const string Success = "fa-check";
        public const string Information = "fa-info";
        public const string Warning = "fa-warning";
        public const string Danger = "fa-ban";
    }
}