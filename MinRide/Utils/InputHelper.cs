using System.Globalization;

namespace MinRide.Utils;

/// <summary>
/// Provides helper methods for getting validated user input.
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Gets an integer from user input with validation.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <param name="min">Minimum allowed value. Default is int.MinValue.</param>
    /// <param name="max">Maximum allowed value. Default is int.MaxValue.</param>
    /// <returns>A valid integer within the specified range.</returns>
    public static int GetInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int value))
            {
                if (value >= min && value <= max)
                {
                    return value;
                }
                UIHelper.Error($"Giá trị phải từ {min} đến {max}.");
            }
            else
            {
                UIHelper.Error("Vui lòng nhập số nguyên hợp lệ.");
            }
        }
    }

    /// <summary>
    /// Gets a double from user input with validation.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <param name="min">Minimum allowed value. Default is double.MinValue.</param>
    /// <param name="max">Maximum allowed value. Default is double.MaxValue.</param>
    /// <returns>A valid double within the specified range.</returns>
    public static double GetDouble(string prompt, double min = double.MinValue, double max = double.MaxValue)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine();

            if (double.TryParse(input, out double value))
            {
                if (value >= min && value <= max)
                {
                    return value;
                }
                UIHelper.Error($"Giá trị phải từ {min} đến {max}.");
            }
            else
            {
                UIHelper.Error("Vui lòng nhập số hợp lệ.");
            }
        }
    }

    /// <summary>
    /// Gets a string from user input with validation.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <param name="allowEmpty">Whether empty strings are allowed. Default is false.</param>
    /// <returns>A trimmed string (non-empty if allowEmpty is false).</returns>
    public static string GetString(string prompt, bool allowEmpty = false)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine()?.Trim();

            if (allowEmpty || !string.IsNullOrEmpty(input))
            {
                return input ?? "";
            }

            UIHelper.Error("Giá trị không được để trống.");
        }
    }

    /// <summary>
    /// Gets an enum value from user input.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>A valid enum value of type T.</returns>
    public static T GetEnum<T>(string prompt) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        while (true)
        {
            Console.WriteLine(prompt);
            int index = 1;
            foreach (var value in values)
            {
                Console.WriteLine($"  {index}. {value}");
                index++;
            }

            Console.Write("Lựa chọn: ");
            string? input = Console.ReadLine();

            // Try to parse as number (1-based index)
            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= values.Length)
            {
                return values[choice - 1];
            }

            // Try to parse as enum name
            if (Enum.TryParse<T>(input, true, out T result))
            {
                return result;
            }

            UIHelper.Error("Lựa chọn không hợp lệ.");
        }
    }

    /// <summary>
    /// Gets a DateTime from user input with a specific format.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <param name="format">The expected date format. Default is "dd/MM/yyyy".</param>
    /// <returns>A valid DateTime.</returns>
    public static DateTime GetDateTime(string prompt, string format = "dd/MM/yyyy")
    {
        while (true)
        {
            Console.Write($"{prompt} ({format}): ");
            string? input = Console.ReadLine();

            if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            UIHelper.Error($"Định dạng không hợp lệ. Vui lòng nhập theo định dạng {format}.");
        }
    }

    /// <summary>
    /// Gets a DateTime or null if user skips.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <param name="format">The expected date format. Default is "dd/MM/yyyy".</param>
    /// <returns>A DateTime or null if skipped.</returns>
    public static DateTime? GetDateTimeOptional(string prompt, string format = "dd/MM/yyyy")
    {
        Console.Write($"{prompt} ({format}, Enter để bỏ qua): ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        while (true)
        {
            if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            UIHelper.Error($"Định dạng không hợp lệ. Vui lòng nhập theo định dạng {format}.");
            Console.Write($"{prompt} ({format}): ");
            input = Console.ReadLine();
        }
    }

    /// <summary>
    /// Gets an optional integer (user can skip by pressing Enter).
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>An integer or null if skipped.</returns>
    public static int? GetIntOptional(string prompt)
    {
        Console.Write($"{prompt} (Enter để bỏ qua): ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        while (!int.TryParse(input, out int value))
        {
            UIHelper.Error("Vui lòng nhập số nguyên hợp lệ.");
            Console.Write($"{prompt}: ");
            input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
        }

        return int.Parse(input);
    }

    /// <summary>
    /// Gets an optional double (user can skip by pressing Enter).
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>A double or null if skipped.</returns>
    public static double? GetDoubleOptional(string prompt)
    {
        Console.Write($"{prompt} (Enter để bỏ qua): ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        while (!double.TryParse(input, out double value))
        {
            UIHelper.Error("Vui lòng nhập số hợp lệ.");
            Console.Write($"{prompt}: ");
            input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
        }

        return double.Parse(input);
    }
}

