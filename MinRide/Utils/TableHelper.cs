namespace MinRide.Utils;

/// <summary>
/// Helper class for drawing consistent ASCII tables in console.
/// Uses String Interpolation with Alignment for perfect column alignment.
/// </summary>
public static class TableHelper
{
    // Standard column widths
    public const int COL_STT = 5;
    public const int COL_ID = 8;
    public const int COL_NAME = 24;
    public const int COL_RATING = 8;
    public const int COL_LOCATION = 15;
    public const int COL_RIDES = 10;
    public const int COL_DISTRICT = 16;
    public const int COL_DISTANCE = 12;
    public const int COL_FARE = 15;
    public const int COL_STATUS = 12;
    public const int COL_TIME = 12;

    /// <summary>
    /// Draws a horizontal separator line.
    /// </summary>
    /// <param name="widths">Array of column widths.</param>
    /// <returns>A separator line string.</returns>
    public static string DrawSeparator(params int[] widths)
    {
        var parts = new List<string>();
        foreach (int w in widths)
        {
            parts.Add(new string('-', w + 2)); // +2 for padding spaces
        }
        return "+" + string.Join("+", parts) + "+";
    }

    /// <summary>
    /// Draws a data row with proper alignment.
    /// </summary>
    /// <param name="values">Array of (value, width, alignLeft) tuples.</param>
    /// <returns>A formatted row string.</returns>
    public static string DrawRow(params (string Value, int Width, bool AlignLeft)[] values)
    {
        var parts = new List<string>();
        foreach (var (value, width, alignLeft) in values)
        {
            string truncated = TruncateString(value, width);
            string formatted = alignLeft 
                ? $" {truncated.PadRight(width)} " 
                : $" {truncated.PadLeft(width)} ";
            parts.Add(formatted);
        }
        return "|" + string.Join("|", parts) + "|";
    }

    /// <summary>
    /// Draws a centered title row.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <param name="totalWidth">Total width of the table (excluding outer borders).</param>
    /// <returns>A centered title row.</returns>
    public static string DrawTitle(string title, int totalWidth)
    {
        int padding = (totalWidth - title.Length) / 2;
        if (padding < 0) padding = 0;
        string centered = title.PadLeft(padding + title.Length).PadRight(totalWidth);
        return "| " + centered + " |";
    }

    /// <summary>
    /// Truncates a string to fit within a specified width.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">Maximum length.</param>
    /// <returns>Truncated string with "..." if needed.</returns>
    public static string TruncateString(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Length <= maxLength) return value;
        if (maxLength <= 3) return value.Substring(0, maxLength);
        return value.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Calculates total table width from column widths.
    /// </summary>
    /// <param name="widths">Column widths.</param>
    /// <returns>Total width including separators.</returns>
    public static int CalculateTotalWidth(params int[] widths)
    {
        // Each column: width + 2 (spaces) + 1 (separator)
        // Plus 1 for the final separator
        return widths.Sum() + (widths.Length * 3) + 1;
    }

    /// <summary>
    /// Draws a full table header with title and column headers.
    /// </summary>
    public static void DrawTableHeader(string title, string[] headers, int[] widths)
    {
        string sep = DrawSeparator(widths);
        int innerWidth = CalculateTotalWidth(widths) - 4; // -4 for "| " and " |"
        
        Console.WriteLine(sep);
        Console.WriteLine(DrawTitle(title, innerWidth));
        Console.WriteLine(sep);
        
        // Draw column headers
        var headerValues = new (string, int, bool)[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            headerValues[i] = (headers[i], widths[i], true);
        }
        Console.WriteLine(DrawRow(headerValues));
        Console.WriteLine(sep);
    }

    /// <summary>
    /// Draws table footer (just a separator).
    /// </summary>
    public static void DrawTableFooter(params int[] widths)
    {
        Console.WriteLine(DrawSeparator(widths));
    }

    // ========== Pre-defined Table Formats ==========

    /// <summary>
    /// Driver table columns: STT, ID, Ten, Rating, Vi tri, So chuyen
    /// </summary>
    public static readonly int[] DriverTableWidths = { COL_STT, COL_ID, COL_NAME, COL_RATING, COL_LOCATION, COL_RIDES };
    public static readonly string[] DriverTableHeaders = { "STT", "ID", "Ten", "Rating", "Vi tri", "So chuyen" };

    /// <summary>
    /// Customer table columns: STT, ID, Ten, Quan/Huyen, Vi tri
    /// </summary>
    public static readonly int[] CustomerTableWidths = { COL_STT, COL_ID, COL_NAME, COL_DISTRICT, COL_LOCATION };
    public static readonly string[] CustomerTableHeaders = { "STT", "ID", "Ten", "Quan/Huyen", "Vi tri" };

    /// <summary>
    /// Ride table columns: STT, RideID, Khach hang, Quang duong, Gia cuoc
    /// </summary>
    public static readonly int[] RideHistoryWidths = { COL_STT, COL_ID, COL_DISTRICT, COL_DISTANCE, COL_FARE };
    public static readonly string[] RideHistoryHeaders = { "STT", "RideID", "Khach hang", "Quang duong", "Gia cuoc" };

    /// <summary>
    /// Pending/InProgress ride table columns: STT, RideID, Khach hang, Tai xe, Quang duong, Gia cuoc, Con lai
    /// </summary>
    public static readonly int[] PendingRideWidths = { COL_STT, COL_ID, COL_STATUS, COL_STATUS, COL_DISTANCE, COL_FARE, COL_TIME };
    public static readonly string[] PendingRideHeaders = { "STT", "RideID", "Khach hang", "Tai xe", "Quang duong", "Gia cuoc", "Con lai" };

    /// <summary>
    /// District table columns: STT, Quan/Huyen, So khach hang
    /// </summary>
    public static readonly int[] DistrictTableWidths = { COL_STT, COL_NAME, COL_RIDES };
    public static readonly string[] DistrictTableHeaders = { "STT", "Quan/Huyen", "So khach hang" };
}

