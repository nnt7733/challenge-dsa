namespace MinRide.Utils;

/// <summary>
/// Provides helper methods for displaying menus and formatted output.
/// </summary>
public static class MenuHelper
{
    /// <summary>
    /// Displays a decorative header with the specified title.
    /// </summary>
    /// <param name="title">The title to display.</param>
    public static void DisplayHeader(string title)
    {
        const int width = 40;
        string paddedTitle = title.Length > width - 4
            ? title.Substring(0, width - 7) + "..."
            : title;

        int padding = (width - 2 - paddedTitle.Length) / 2;
        string leftPad = new string(' ', padding);
        string rightPad = new string(' ', width - 2 - padding - paddedTitle.Length);

        Console.WriteLine($"+{new string('-', width - 2)}+");
        Console.WriteLine($"|{leftPad}{paddedTitle}{rightPad}|");
        Console.WriteLine($"+{new string('-', width - 2)}+");
    }

    /// <summary>
    /// Displays a menu and gets the user's choice.
    /// </summary>
    /// <param name="options">Array of menu options to display.</param>
    /// <param name="prompt">The prompt to display. Default is "Chon chuc nang".</param>
    /// <returns>The selected option (1-based index).</returns>
    public static int GetMenuChoice(string[] options, string prompt = "Chon chuc nang")
    {
        while (true)
        {
            DisplayHeader(prompt);
            Console.WriteLine();

            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {options[i]}");
            }

            Console.WriteLine();
            Console.Write("Lựa chọn của bạn: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= options.Length)
            {
                return choice;
            }

            ShowError($"Vui lòng nhập số từ 1 đến {options.Length}.");
        }
    }

    /// <summary>
    /// Displays data in a formatted table.
    /// </summary>
    /// <typeparam name="T">The type of items to display.</typeparam>
    /// <param name="items">The list of items to display.</param>
    /// <param name="columns">Dictionary mapping column names to value extractors.</param>
    public static void DisplayTable<T>(List<T> items, Dictionary<string, Func<T, string>> columns)
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Không có dữ liệu để hiển thị.");
            return;
        }

        // Calculate column widths
        Dictionary<string, int> columnWidths = new Dictionary<string, int>();
        foreach (var col in columns)
        {
            int maxWidth = col.Key.Length;
            foreach (var item in items)
            {
                int valueWidth = col.Value(item).Length;
                if (valueWidth > maxWidth)
                {
                    maxWidth = valueWidth;
                }
            }
            columnWidths[col.Key] = Math.Min(maxWidth + 2, 30); // Cap at 30 chars
        }

        // Build format strings
        string topBorder = "+";
        string bottomBorder = "+";
        string headerSeparator = "+";

        foreach (var col in columns.Keys)
        {
            int width = columnWidths[col];
            topBorder += new string('-', width) + "+";
            bottomBorder += new string('-', width) + "+";
            headerSeparator += new string('-', width) + "+";
        }

        // Print table
        Console.WriteLine(topBorder);

        // Print header
        string headerRow = "|";
        foreach (var col in columns.Keys)
        {
            int width = columnWidths[col];
            headerRow += $" {col.PadRight(width - 1)}|";
        }
        Console.WriteLine(headerRow);
        Console.WriteLine(headerSeparator);

        // Print data rows
        foreach (var item in items)
        {
            string dataRow = "|";
            foreach (var col in columns)
            {
                int width = columnWidths[col.Key];
                string value = col.Value(item);
                if (value.Length > width - 1)
                {
                    value = value.Substring(0, width - 4) + "...";
                }
                dataRow += $" {value.PadRight(width - 1)}|";
            }
            Console.WriteLine(dataRow);
        }

        Console.WriteLine(bottomBorder);
    }

    /// <summary>
    /// Prompts for confirmation.
    /// </summary>
    /// <param name="message">The confirmation message.</param>
    /// <returns><c>true</c> if user confirms; otherwise, <c>false</c>.</returns>
    public static bool Confirm(string message)
    {
        while (true)
        {
            Console.Write($"{message} (Y/N): ");
            string? input = Console.ReadLine()?.Trim().ToUpper();

            if (input == "Y")
            {
                return true;
            }
            if (input == "N")
            {
                return false;
            }

            ShowError("Vui long nhap Y hoac N.");
        }
    }

    /// <summary>
    /// Waits for user to press any key to continue.
    /// </summary>
    public static void PressAnyKey()
    {
        Console.WriteLine();
        Console.WriteLine("Nhan phim bat ky de tiep tuc...");
        Console.ReadKey(true);
        Console.Clear();
    }

    /// <summary>
    /// Displays a success message in green with a checkmark symbol.
    /// </summary>
    /// <param name="message">The success message.</param>
    public static void ShowSuccess(string message)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {message}");
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Displays an error message in red with an X symbol.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static void ShowError(string message)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[X] {message}");
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Displays a warning message in yellow.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public static void ShowWarning(string message)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[!] {message}");
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Displays an info message in cyan.
    /// </summary>
    /// <param name="message">The info message.</param>
    public static void ShowInfo(string message)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[i] {message}");
        Console.ForegroundColor = originalColor;
    }
}
