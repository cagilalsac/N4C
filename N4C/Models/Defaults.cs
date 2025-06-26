namespace N4C.Models
{
    public static class Defaults
    {
        public static string EN => "en-US";
        public static string TR => "tr-TR";
        public static int SystemId => 1;
        public static string System => "system";
        public static int AdminId => 2;
        public static string Admin => "admin";
        public static int UserId => 3;
        public static string User => "user";
        public static int ActiveId => 1;
        public static string Active => "Active";
        public static int InactiveId => 2;
        public static string Inactive => "Inactive";
        public static int[] RecordsPerPageCounts => [5, 10, 25, 50, 100];
    }
}
