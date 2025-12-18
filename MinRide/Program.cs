namespace MinRide;

using MinRide.Utils;

/// <summary>
/// Entry point for the MinRide application.
/// </summary>
class Program
{
    private static int GetSafeWidth(int preferred = 66, int min = 44)
    {
        try
        {
            int w = Console.WindowWidth;
            if (w <= 0) return preferred;
            // Keep 1 char safety to avoid wrapping on some terminals
            int safe = Math.Max(min, Math.Min(preferred, w - 1));
            return safe;
        }
        catch
        {
            return preferred;
        }
    }

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            int w = GetSafeWidth();
            Console.WriteLine();
            UIHelper.Line(w);
            UIHelper.Title("MINRIDE", w);
            UIHelper.Title("Hệ thống quản lý đặt xe thông minh", w);
            UIHelper.Title("Phiên bản 1.0", w);
            UIHelper.Line(w);
            Console.WriteLine();

            var system = new MinRideSystem();
            system.Run();

            Console.WriteLine();
            UIHelper.Line(w);
            UIHelper.Title("Cảm ơn bạn đã sử dụng MinRide! Hẹn gặp lại!", w);
            UIHelper.Line(w);
        }
        catch (Exception ex)
        {
            int w = GetSafeWidth();
            Console.WriteLine();
            UIHelper.Line(w);
            UIHelper.Title("LỖI KHÔNG MONG MUỐN", w);
            UIHelper.Line(w);
            UIHelper.Row($"Chi tiết: {ex.Message}", w);
            UIHelper.Line(w);
            UIHelper.Row("Vui lòng liên hệ bộ phận hỗ trợ nếu lỗi tiếp tục xảy ra.", w);
            UIHelper.Line(w);
        }
    }
}
