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

            // Optional: regenerate sample data quickly for friends to clone & test
            if (args.Any(a => string.Equals(a, "--generate-data", StringComparison.OrdinalIgnoreCase)))
            {
                int driverCount = 10, customerCount = 10, rideCount = 10;
                
                // Parse command line arguments: --generate-data [drivers] [customers] [rides]
                if (args.Length >= 2 && int.TryParse(args[1], out int d)) driverCount = d;
                if (args.Length >= 3 && int.TryParse(args[2], out int c)) customerCount = c;
                if (args.Length >= 4 && int.TryParse(args[3], out int r)) rideCount = r;
                
                DataGenerator.GenerateAndSaveData(driverCount, customerCount, rideCount, "Data");
                Console.WriteLine();
                UIHelper.Success($"Đã sinh lại dữ liệu mẫu ({driverCount} tài xế, {customerCount} khách hàng, {rideCount} chuyến đi).");
                UIHelper.Info("Bạn có thể chạy lại chương trình để bắt đầu sử dụng dữ liệu mới.");
                return;
            }

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
