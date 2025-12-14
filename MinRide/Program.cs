namespace MinRide;

/// <summary>
/// Entry point for the MinRide application.
/// </summary>
class Program
{
    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║     ███╗   ███╗██╗███╗   ██╗██████╗ ██╗██████╗ ███████╗    ║");
            Console.WriteLine("║     ████╗ ████║██║████╗  ██║██╔══██╗██║██╔══██╗██╔════╝    ║");
            Console.WriteLine("║     ██╔████╔██║██║██╔██╗ ██║██████╔╝██║██║  ██║█████╗      ║");
            Console.WriteLine("║     ██║╚██╔╝██║██║██║╚██╗██║██╔══██╗██║██║  ██║██╔══╝      ║");
            Console.WriteLine("║     ██║ ╚═╝ ██║██║██║ ╚████║██║  ██║██║██████╔╝███████╗    ║");
            Console.WriteLine("║     ╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝╚═════╝ ╚══════╝    ║");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║           Hệ thống quản lý đặt xe thông minh               ║");
            Console.WriteLine("║                     Version 1.0                            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var system = new MinRideSystem();
            system.Run();

            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Cảm ơn bạn đã sử dụng MinRide! Hẹn gặp lại!            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    LỖI KHÔNG MONG MUỐN                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Chi tiết: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Vui lòng liên hệ bộ phận hỗ trợ nếu lỗi tiếp tục xảy ra.");
        }
    }
}
