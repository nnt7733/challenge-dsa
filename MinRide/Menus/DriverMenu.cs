namespace MinRide.Menus;

using MinRide.Auth;
using MinRide.Managers;
using MinRide.Models;
using MinRide.Utils;

/// <summary>
/// Menu for driver users.
/// </summary>
public class DriverMenu
{
    private readonly AuthManager authManager;
    private readonly DriverManager driverManager;
    private readonly RideManager rideManager;

    public DriverMenu(AuthManager auth, DriverManager dm, RideManager rm)
    {
        authManager = auth;
        driverManager = dm;
        rideManager = rm;
    }

    public void Show()
    {
        bool back = false;

        while (!back && authManager.Session.IsLoggedIn)
        {
            rideManager.ProcessRides(driverManager);

            var driver = authManager.GetCurrentDriver();
            if (driver == null)
            {
                UIHelper.Error("Không tìm thấy thông tin tài xế.");
                authManager.Logout();
                return;
            }

            Console.WriteLine();
            UIHelper.Line();
            UIHelper.Title("MINRIDE - TÀI XẾ");
            UIHelper.Row($"Xin chào, {UIHelper.Truncate(driver.Name, 35)}");
            UIHelper.Row($"Đánh giá: {driver.Rating:F1} sao | Số chuyến: {driver.TotalRides}");
            UIHelper.Line();
            UIHelper.MenuItem("1", "Xem thông tin cá nhân");
            UIHelper.MenuItem("2", "Cập nhật thông tin");
            UIHelper.MenuItem("3", "Xem lịch sử chuyến đi");
            UIHelper.MenuItem("4", "Xem thống kê");
            UIHelper.MenuItem("5", "Đổi mật khẩu");
            UIHelper.MenuItem("6", "Đăng xuất");
            UIHelper.MenuItem("0", "Thoát chương trình");
            UIHelper.Line();

            switch (UIHelper.PromptChoice("Chọn chức năng: "))
            {
                case "1": ViewMyInfo(); break;
                case "2": UpdateMyInfo(); break;
                case "3": ViewRideHistory(); break;
                case "4": ViewStatistics(); break;
                case "5": ChangePassword(); break;
                case "6":
                    authManager.Logout();
                    UIHelper.Success("Đã đăng xuất.");
                    back = true;
                    break;
                case "0":
                    Console.WriteLine("Tạm biệt!");
                    Environment.Exit(0);
                    break;
                default:
                    UIHelper.Error("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    private void ViewMyInfo()
    {
        var driver = authManager.GetCurrentDriver();
        if (driver == null) return;

        Console.WriteLine();
        UIHelper.Header("THÔNG TIN CÁ NHÂN");
        UIHelper.LabelValue("ID:", driver.Id.ToString());
        UIHelper.LabelValue("Tên:", UIHelper.Truncate(driver.Name, 30));
        UIHelper.LabelValue("Đánh giá:", $"{driver.Rating:F1} sao ({driver.RatingCount} đánh giá)");
        UIHelper.LabelValue("Vị trí:", $"({driver.Location.X:F1}, {driver.Location.Y:F1})");
        UIHelper.LabelValue("Tổng số chuyến:", driver.TotalRides.ToString());
        UIHelper.Line();
    }

    private void UpdateMyInfo()
    {
        var driver = authManager.GetCurrentDriver();
        if (driver == null) return;

        Console.WriteLine("\n--- CẬP NHẬT THÔNG TIN ---");
        ViewMyInfo();

        Console.WriteLine("Chọn thông tin cần cập nhật:");
        Console.WriteLine("1. Tên");
        Console.WriteLine("2. Vị trí (X, Y)");
        Console.WriteLine("3. Hủy");

        switch (UIHelper.PromptChoice("Lựa chọn: "))
        {
            case "1":
                string? newName = UIHelper.Prompt("Nhập tên mới: ");
                if (!string.IsNullOrEmpty(newName))
                {
                    driver.Name = newName;
                    UIHelper.Success("Đã cập nhật tên.");
                }
                break;
            case "2":
                if (double.TryParse(UIHelper.Prompt("Nhập tọa độ X mới: "), out double x) &&
                    double.TryParse(UIHelper.Prompt("Nhập tọa độ Y mới: "), out double y))
                {
                    driver.Location = (x, y);
                    UIHelper.Success("Đã cập nhật vị trí.");
                }
                break;
            case "3":
                Console.WriteLine("Đã hủy.");
                break;
            default:
                UIHelper.Error("Lựa chọn không hợp lệ.");
                break;
        }
    }

    private void ViewRideHistory()
    {
        var driver = authManager.GetCurrentDriver();
        if (driver == null) return;

        var rides = rideManager.GetRidesByDriver(driver.Id)
            .Where(r => r.Status == "COMPLETED")
            .OrderByDescending(r => r.Timestamp)
            .ToList();

        if (rides.Count == 0)
        {
            Console.WriteLine("\nBan chua co chuyen di nao hoan thanh.");
            return;
        }

        Console.WriteLine($"\n--- LỊCH SỬ CHUYẾN ĐI ({rides.Count} chuyến) ---");
        UIHelper.DriverRideTable.DrawHeader();

        double totalFare = 0;
        double totalDistance = 0;

        foreach (var ride in rides.Take(20))
        {
            string rating = ride.CustomerRating.HasValue ? $"{ride.CustomerRating} sao" : "---";
            UIHelper.DriverRideTable.DrawRow(
                ride.RideId,
                ride.Timestamp.ToString("dd/MM/yyyy"),
                $"{ride.Distance:F1} km",
                $"{ride.Fare:N0} đ",
                rating
            );
            totalFare += ride.Fare;
            totalDistance += ride.Distance;
        }

        UIHelper.DriverRideTable.DrawSeparator();
        UIHelper.DriverRideTable.DrawTotalRow($"{totalDistance:F1} km", $"{totalFare:N0} đ");
        UIHelper.DriverRideTable.DrawSeparator();
    }

    private void ViewStatistics()
    {
        var driver = authManager.GetCurrentDriver();
        if (driver == null) return;

        var (totalRides, completedCount, totalRevenue, avgDistance) = rideManager.GetDriverStats(driver.Id);

        Console.WriteLine();
        UIHelper.Header("THỐNG KÊ CỦA BẠN");
        UIHelper.LabelValue("Tổng số chuyến:", totalRides.ToString());
        UIHelper.LabelValue("Chuyến hoàn thành:", completedCount.ToString());
        UIHelper.LabelValue("Tổng doanh thu:", $"{totalRevenue:N0} VND");
        UIHelper.LabelValue("Quãng đường TB:", $"{avgDistance:F2} km");
        UIHelper.LabelValue("Đánh giá hiện tại:", $"{driver.Rating:F1} sao ({driver.RatingCount} đánh giá)");
        UIHelper.Line();
    }

    private void ChangePassword()
    {
        Console.WriteLine("\n--- ĐỔI MẬT KHẨU ---");
        string? oldPassword = UIHelper.Prompt("Nhập mật khẩu hiện tại: ");
        string? newPassword = UIHelper.Prompt("Nhập mật khẩu mới: ");
        string? confirmPassword = UIHelper.Prompt("Xác nhận mật khẩu mới: ");

        if (newPassword != confirmPassword)
        {
            UIHelper.Error("Mật khẩu xác nhận không khớp.");
            return;
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            UIHelper.Error("Mật khẩu mới không được để trống.");
            return;
        }

        if (authManager.ChangePassword(oldPassword ?? "", newPassword))
            UIHelper.Success("Đổi mật khẩu thành công!");
        else
            UIHelper.Error("Đổi mật khẩu thất bại. Mật khẩu hiện tại không đúng.");
    }
}
