namespace MinRide.Utils;

using System.Text;

/// <summary>
/// Helper class for consistent console UI rendering using String Interpolation with Alignment.
/// </summary>
public static class UIHelper
{
    // ==================== CONSTANTS ====================
    public const int BoxWidth = 52;
    
    // Column widths for tables
    public const int ColSTT = 5;
    public const int ColID = 8;
    public const int ColName = 20;
    public const int ColRating = 8;
    public const int ColLocation = 14;
    public const int ColRides = 9;
    public const int ColDistance = 10;
    public const int ColFare = 14;
    public const int ColStatus = 12;
    public const int ColDate = 12;

    // ==================== BOX DRAWING ====================
    
    /// <summary>
    /// Draws a horizontal separator line.
    /// </summary>
    public static void Line(int width = BoxWidth)
    {
        Console.WriteLine($"+{new string('-', width - 2)}+");
    }

    /// <summary>
    /// Back-compat: DrawLine wrapper.
    /// </summary>
    public static void DrawLine(int width = BoxWidth) => Line(width);

    /// <summary>
    /// Draws a centered title row.
    /// </summary>
    public static void Title(string text, int width = BoxWidth)
    {
        text = Normalize(text);
        int innerWidth = width - 2;
        if (innerWidth <= 0)
        {
            Console.WriteLine("| |");
            return;
        }

        if (text.Length > innerWidth)
        {
            text = Truncate(text, innerWidth);
        }

        int textLen = text.Length;
        int leftPad = (innerWidth - textLen) / 2;
        int rightPad = innerWidth - textLen - leftPad;
        Console.WriteLine($"|{new string(' ', leftPad)}{text}{new string(' ', rightPad)}|");
    }

    /// <summary>
    /// Back-compat: DrawCenteredRow wrapper.
    /// </summary>
    public static void DrawCenteredRow(string text, int width = BoxWidth) => Title(text, width);

    /// <summary>
    /// Draws a left-aligned row.
    /// </summary>
    public static void Row(string text, int width = BoxWidth)
    {
        int innerWidth = width - 4;
        string content = Truncate(text, innerWidth).PadRight(innerWidth);
        Console.WriteLine($"| {content} |");
    }

    /// <summary>
    /// Back-compat: DrawRow wrapper.
    /// </summary>
    public static void DrawRow(string text, int width = BoxWidth) => Row(text, width);

    /// <summary>
    /// Draws a label-value row with fixed label width.
    /// </summary>
    public static void LabelValue(string label, string value, int labelWidth = 16, int width = BoxWidth)
    {
        int valueWidth = width - 4 - labelWidth;
        string truncLabel = Truncate(label, labelWidth);
        string truncValue = Truncate(value, valueWidth);
        string left = truncLabel.PadRight(labelWidth);
        string right = truncValue.PadRight(valueWidth);
        Console.WriteLine($"| {left}{right} |");
    }

    /// <summary>
    /// Back-compat: DrawLabelValue wrapper.
    /// </summary>
    public static void DrawLabelValue(string label, string value, int labelWidth = 16, int width = BoxWidth) =>
        LabelValue(label, value, labelWidth, width);

    /// <summary>
    /// Draws a menu option row.
    /// </summary>
    public static void MenuItem(string number, string text, int width = BoxWidth)
    {
        int innerWidth = width - 4;
        string content = $"{number,2}. {text}";
        string cell = Truncate(content, innerWidth).PadRight(innerWidth);
        Console.WriteLine($"| {cell} |");
    }

    /// <summary>
    /// Back-compat: DrawMenuOption wrapper.
    /// </summary>
    public static void DrawMenuOption(string number, string text, int width = BoxWidth) => MenuItem(number, text, width);

    /// <summary>
    /// Draws a complete header box.
    /// </summary>
    public static void Header(string title, int width = BoxWidth)
    {
        Line(width);
        Title(title, width);
        Line(width);
    }

    /// <summary>
    /// Back-compat: DrawHeader wrapper.
    /// </summary>
    public static void DrawHeader(string title, int width = BoxWidth) => Header(title, width);

    // ==================== TABLE DRAWING ====================

    /// <summary>
    /// Draws a table separator with custom column widths.
    /// </summary>
    public static void TableSeparator(params int[] colWidths)
    {
        var sb = new StringBuilder("+");
        foreach (int w in colWidths)
        {
            sb.Append(new string('-', w + 2));
            sb.Append('+');
        }
        Console.WriteLine(sb.ToString());
    }

    /// <summary>
    /// Draws a centered title row for a table based on column widths.
    /// </summary>
    public static void TableTitle(string title, params int[] colWidths)
    {
        title = Normalize(title);
        int totalWidth = colWidths.Sum() + (colWidths.Length * 3) + 1;
        int innerWidth = totalWidth - 2;
        int leftPad = (innerWidth - title.Length) / 2;
        if (leftPad < 0) leftPad = 0;
        int rightPad = innerWidth - title.Length - leftPad;
        if (rightPad < 0) rightPad = 0;
        Console.WriteLine($"|{new string(' ', leftPad)}{title}{new string(' ', rightPad)}|");
    }

    // ==================== PREDEFINED TABLES ====================

    /// <summary>
    /// Driver table columns: STT, ID, Ten, Rating, Vi tri, So chuyen
    /// </summary>
    public static class DriverTable
    {
        private const int WStt = 4;
        private const int WId = 6;
        private const int WName = 18;
        private const int WRating = 7;
        private const int WLoc = 13;
        private const int WRides = 10;

        private static string Sep => $"+{new string('-', WStt + 2)}+{new string('-', WId + 2)}+{new string('-', WName + 2)}+{new string('-', WRating + 2)}+{new string('-', WLoc + 2)}+{new string('-', WRides + 2)}+";

        public static void DrawHeader()
        {
            Console.WriteLine(Sep);
            Console.WriteLine($"| {"STT",WStt} | {"ID",WId} | {"Tên",-WName} | {"Đ.giá",WRating} | {"Vị trí",-WLoc} | {"Số chuyến",WRides} |");
            Console.WriteLine(Sep);
        }

        public static void DrawSeparator() => Console.WriteLine(Sep);

        public static void DrawRow(int stt, int id, string name, double rating, string location, int rides)
        {
            string n = Truncate(name, WName);
            string loc = Truncate(location, WLoc);
            Console.WriteLine($"| {stt,WStt} | {id,WId} | {n,-WName} | {rating,WRating:F1} | {loc,-WLoc} | {rides,WRides} |");
        }
    }

    /// <summary>
    /// Customer table columns: STT, ID, Ten, Quan/Huyen, Vi tri
    /// </summary>
    public static class CustomerTable
    {
        private const int WStt = 4;
        private const int WId = 6;
        private const int WName = 18;
        private const int WDist = 14;
        private const int WLoc = 13;

        private static string Sep => $"+{new string('-', WStt + 2)}+{new string('-', WId + 2)}+{new string('-', WName + 2)}+{new string('-', WDist + 2)}+{new string('-', WLoc + 2)}+";

        public static void DrawHeader()
        {
            Console.WriteLine(Sep);
            Console.WriteLine($"| {"STT",WStt} | {"ID",WId} | {"Tên",-WName} | {"Quận/Huyện",-WDist} | {"Vị trí",-WLoc} |");
            Console.WriteLine(Sep);
        }

        public static void DrawSeparator() => Console.WriteLine(Sep);

        public static void DrawRow(int stt, int id, string name, string district, string location)
        {
            string n = Truncate(name, WName);
            string d = Truncate(district, WDist);
            string loc = Truncate(location, WLoc);
            Console.WriteLine($"| {stt,WStt} | {id,WId} | {n,-WName} | {d,-WDist} | {loc,-WLoc} |");
        }
    }

    /// <summary>
    /// Ride history table columns: RideID, Tai xe, Quang dg, Gia cuoc, Danh gia
    /// </summary>
    public static class RideTable
    {
        private const int WRide = 6;
        private const int WDriver = 16;
        private const int WDist = 9;
        private const int WFare = 13;
        private const int WRate = 7;

        private static string Sep => $"+{new string('-', WRide + 2)}+{new string('-', WDriver + 2)}+{new string('-', WDist + 2)}+{new string('-', WFare + 2)}+{new string('-', WRate + 2)}+";

        public static void DrawHeader()
        {
            Console.WriteLine(Sep);
            Console.WriteLine($"| {"Mã",WRide} | {"Tài xế",-WDriver} | {"Quãng",WDist} | {"Giá cước",WFare} | {"Đ.giá",-WRate} |");
            Console.WriteLine(Sep);
        }

        public static void DrawSeparator() => Console.WriteLine(Sep);

        public static void DrawRow(int rideId, string driverName, string distance, string fare, string rating)
        {
            string d = Truncate(driverName, WDriver);
            string dist = Truncate(distance, WDist);
            string f = Truncate(fare, WFare);
            string r = Truncate(rating, WRate);
            Console.WriteLine($"| {rideId,WRide} | {d,-WDriver} | {dist,WDist} | {f,WFare} | {r,-WRate} |");
        }
    }

    /// <summary>
    /// Unrated rides table: RideID, Tai xe, Quang dg, Ngay
    /// </summary>
    public static class UnratedRideTable
    {
        private const int WRide = 6;
        private const int WDriver = 18;
        private const int WDist = 9;
        private const int WDate = 14;

        private static string Sep => $"+{new string('-', WRide + 2)}+{new string('-', WDriver + 2)}+{new string('-', WDist + 2)}+{new string('-', WDate + 2)}+";

        public static void DrawHeader()
        {
            Console.WriteLine(Sep);
            Console.WriteLine($"| {"Mã",WRide} | {"Tài xế",-WDriver} | {"Quãng",WDist} | {"Ngày",-WDate} |");
            Console.WriteLine(Sep);
        }

        public static void DrawSeparator() => Console.WriteLine(Sep);

        public static void DrawRow(int rideId, string driverName, string distance, string date)
        {
            string d = Truncate(driverName, WDriver);
            string dist = Truncate(distance, WDist);
            string dt = Truncate(date, WDate);
            Console.WriteLine($"| {rideId,WRide} | {d,-WDriver} | {dist,WDist} | {dt,-WDate} |");
        }
    }

    /// <summary>
    /// Driver ride history: RideID, Ngay, Quang duong, Gia cuoc, Danh gia
    /// </summary>
    public static class DriverRideTable
    {
        private const int WRide = 6;
        private const int WDate = 10;
        private const int WDist = 10;
        private const int WFare = 14;
        private const int WRate = 7;

        private static string Sep => $"+{new string('-', WRide + 2)}+{new string('-', WDate + 2)}+{new string('-', WDist + 2)}+{new string('-', WFare + 2)}+{new string('-', WRate + 2)}+";

        public static void DrawHeader()
        {
            Console.WriteLine(Sep);
            Console.WriteLine($"| {"Mã",WRide} | {"Ngày",-WDate} | {"Quãng",WDist} | {"Giá cước",WFare} | {"Đ.giá",-WRate} |");
            Console.WriteLine(Sep);
        }

        public static void DrawSeparator() => Console.WriteLine(Sep);

        public static void DrawRow(int rideId, string date, string distance, string fare, string rating)
        {
            string dt = Truncate(date, WDate);
            string dist = Truncate(distance, WDist);
            string f = Truncate(fare, WFare);
            string r = Truncate(rating, WRate);
            Console.WriteLine($"| {rideId,WRide} | {dt,-WDate} | {dist,WDist} | {f,WFare} | {r,-WRate} |");
        }

        public static void DrawTotalRow(string totalDist, string totalFare)
        {
            string dist = Truncate(totalDist, WDist);
            string fare = Truncate(totalFare, WFare);
            Console.WriteLine($"| {"",WRide} | {"TỔNG",-WDate} | {dist,WDist} | {fare,WFare} | {"",-WRate} |");
        }
    }

    // ==================== UTILITY METHODS ====================

    /// <summary>
    /// Truncates text to fit within specified width.
    /// </summary>
    public static string Truncate(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return "";
        text = Normalize(text);
        if (text.Length <= maxWidth) return text;
        if (maxWidth <= 3) return text.Substring(0, maxWidth);
        return text.Substring(0, maxWidth - 2) + "..";
    }

    /// <summary>
    /// Pads a string to specified width.
    /// </summary>
    public static string Pad(string text, int width, bool rightAlign = false)
    {
        string truncated = Truncate(text, width);
        return rightAlign ? truncated.PadLeft(width) : truncated.PadRight(width);
    }

    // ==================== MESSAGE HELPERS ====================

    public static void Success(string message) => Console.WriteLine($"[OK] {message}");
    public static void Error(string message) => Console.WriteLine($"[X] {message}");
    public static void Info(string message) => Console.WriteLine($"[!] {message}");

    public static string? Prompt(string message)
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim();
    }

    public static string? PromptChoice(string message = "Chọn: ")
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim();
    }

    /// <summary>
    /// Normalizes text to NFC to avoid combining-mark width issues in console alignment.
    /// </summary>
    private static string Normalize(string text)
    {
        return string.IsNullOrEmpty(text) ? "" : text.Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Maps internal status codes to Vietnamese display text.
    /// </summary>
    public static string FormatStatus(string status)
    {
        return status switch
        {
            "PENDING" => "ĐANG CHỜ",
            "IN_PROGRESS" => "ĐANG CHẠY",
            "COMPLETED" => "HOÀN THÀNH",
            "CANCELLED" => "ĐÃ HỦY",
            _ => status
        };
    }
}
