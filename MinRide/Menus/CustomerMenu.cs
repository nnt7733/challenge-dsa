namespace MinRide.Menus;

using MinRide.Auth;
using MinRide.Managers;
using MinRide.Models;
using MinRide.Utils;

/// <summary>
/// Menu for customer users.
/// </summary>
public class CustomerMenu
{
    private readonly AuthManager authManager;
    private readonly CustomerManager customerManager;
    private readonly DriverManager driverManager;
    private readonly RideManager rideManager;

    public CustomerMenu(AuthManager auth, CustomerManager cm, DriverManager dm, RideManager rm)
    {
        authManager = auth;
        customerManager = cm;
        driverManager = dm;
        rideManager = rm;
    }

    public void Show()
    {
        bool back = false;

        while (!back && authManager.Session.IsLoggedIn)
        {
            rideManager.ProcessRides(driverManager);

            var customer = authManager.GetCurrentCustomer();
            if (customer == null)
            {
                UIHelper.Error("Không tìm thấy thông tin khách hàng.");
                authManager.Logout();
                return;
            }

            var unratedRides = rideManager.GetUnratedCompletedRides(customer.Id);
            string ratingNotice = unratedRides.Count > 0 ? $" ({unratedRides.Count} chờ đánh giá)" : "";

            Console.WriteLine();
            UIHelper.Line();
            UIHelper.Title("MINRIDE - KHÁCH HÀNG");
            UIHelper.Title($"Xin chào, {UIHelper.Truncate(customer.Name, 35)}");
            UIHelper.Line();
            UIHelper.MenuItem("1", "Xem thông tin cá nhân");
            UIHelper.MenuItem("2", "Cập nhật thông tin");
            UIHelper.MenuItem("3", "Đặt xe");
            UIHelper.MenuItem("4", "Xem chuyến đi hiện tại");
            UIHelper.MenuItem("5", "Xem lịch sử chuyến đi");
            UIHelper.MenuItem("6", $"Đánh giá tài xế{ratingNotice}");
            UIHelper.MenuItem("7", "Đổi mật khẩu");
            UIHelper.MenuItem("8", "Đăng xuất");
            UIHelper.MenuItem("0", "Thoát chương trình");
            UIHelper.Line();

            switch (UIHelper.PromptChoice("Chọn chức năng: "))
            {
                case "1": ViewMyInfo(); break;
                case "2": UpdateMyInfo(); break;
                case "3": BookRide(); break;
                case "4": ViewCurrentRide(); break;
                case "5": ViewRideHistory(); break;
                case "6": RateDriver(); break;
                case "7": ChangePassword(); break;
                case "8":
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
        var customer = authManager.GetCurrentCustomer();
        if (customer == null) return;

        Console.WriteLine();
        UIHelper.Header("THONG TIN CA NHAN");
        UIHelper.LabelValue("ID:", customer.Id.ToString());
        UIHelper.LabelValue("Ten:", UIHelper.Truncate(customer.Name, 30));
        UIHelper.LabelValue("Quan/Huyen:", UIHelper.Truncate(customer.District, 30));
        UIHelper.LabelValue("Vị trí:", $"({customer.Location.X:F1}, {customer.Location.Y:F1})");
        UIHelper.Line();
    }

    private void UpdateMyInfo()
    {
        var customer = authManager.GetCurrentCustomer();
        if (customer == null) return;

        Console.WriteLine("\n--- CẬP NHẬT THÔNG TIN ---");
        ViewMyInfo();

        Console.WriteLine("Chọn thông tin cần cập nhật:");
        Console.WriteLine("1. Tên");
        Console.WriteLine("2. Quận/Huyện");
        Console.WriteLine("3. Vị trí (X, Y)");
        Console.WriteLine("4. Hủy");

        switch (UIHelper.PromptChoice("Lựa chọn: "))
        {
            case "1":
                string? newName = UIHelper.Prompt("Nhập tên mới: ");
                if (!string.IsNullOrEmpty(newName))
                {
                    customer.Name = newName;
                    UIHelper.Success("Đã cập nhật tên.");
                }
                break;
            case "2":
                string? newDistrict = UIHelper.Prompt("Nhập quận/huyện mới: ");
                if (!string.IsNullOrEmpty(newDistrict))
                {
                    customer.District = newDistrict;
                    UIHelper.Success("Đã cập nhật quận/huyện.");
                }
                break;
            case "3":
                if (double.TryParse(UIHelper.Prompt("Nhập tọa độ X mới: "), out double x) &&
                    double.TryParse(UIHelper.Prompt("Nhập tọa độ Y mới: "), out double y))
                {
                    customer.Location = (x, y);
                    UIHelper.Success("Đã cập nhật vị trí.");
                }
                break;
            case "4":
                Console.WriteLine("Đã hủy.");
                break;
            default:
                UIHelper.Error("Lựa chọn không hợp lệ.");
                break;
        }
    }

    private void BookRide()
    {
        var customer = authManager.GetCurrentCustomer();
        if (customer == null) return;

        if (rideManager.HasActiveRide(customer.Id))
        {
            var activeRide = rideManager.GetActiveRide(customer.Id);
            Console.WriteLine();
            UIHelper.Header("[!] BAN DANG CO CHUYEN DI");
            UIHelper.LabelValue("Ride ID:", activeRide!.RideId.ToString());
            UIHelper.LabelValue("Trang thai:", activeRide.Status);
            UIHelper.Row("");
            UIHelper.Row("Vui long doi chuyen hien tai hoan thanh!");
            UIHelper.Line();
            return;
        }

        Console.WriteLine();
        UIHelper.Header("ĐẶT XE");
        UIHelper.LabelValue("Vị trí hiện tại:", $"({customer.Location.X:F1}, {customer.Location.Y:F1})");
        UIHelper.Line();

        if (!double.TryParse(UIHelper.Prompt("\nNhập khoảng cách đến điểm đích (km): "), out double destDistance) || destDistance < 0)
        {
            UIHelper.Error("Khoảng cách không hợp lệ.");
            return;
        }

        Console.WriteLine();
        UIHelper.Header("CHỌN TIÊU CHÍ TÌM TÀI XẾ");
        UIHelper.MenuItem("1", "Tài xế gần nhất");
        UIHelper.MenuItem("2", "Tài xế đánh giá cao nhất (bán kính 5km)");
        UIHelper.MenuItem("3", "Cân bằng (khoảng cách + đánh giá)");
        UIHelper.MenuItem("0", "Hủy");
        UIHelper.Line();

        string? strategyChoice = UIHelper.PromptChoice("Chọn tiêu chí (0-3): ");
        if (strategyChoice == "0") { Console.WriteLine("Đã hủy."); return; }

        int strategy = strategyChoice switch { "2" => 2, "3" => 3, _ => 1 };
        double radius = strategy switch { 1 => 10.0, 2 => 5.0, 3 => 7.0, _ => 10.0 };

        (double Distance, Driver Driver)? bestMatch = null;
        string strategyName = "";
        int retryCount = 0;

        while (true)
        {
            switch (strategy)
            {
                case 1: // Gần nhất - Dùng FindTopNearestDrivers với k=1
                    var topNearest = driverManager.FindTopNearestDrivers(customer.Location, 1);
                    if (topNearest.Count > 0)
                    {
                        bestMatch = topNearest[0];
                        strategyName = "Gần nhất";
                    }
                    break;
                case 2: // Rating cao nhất - Dùng Heap-based O(m)
                    bestMatch = driverManager.FindTopRatedDriverInRadius(customer.Location, radius);
                    strategyName = "Đánh giá cao nhất";
                    break;
                case 3: // Cân bằng - Dùng Heap-based O(m)
                    bestMatch = driverManager.FindBestBalancedDriverInRadius(customer.Location, radius);
                    strategyName = "Cân bằng";
                    break;
            }

            if (bestMatch != null) break;

            Console.WriteLine($"Không tìm thấy tài xế trong bán kính {radius:F1}km.");
            if (retryCount >= 3) { UIHelper.Error("Không thể tìm thấy tài xế."); return; }
            if (UIHelper.Prompt("Mở rộng bán kính? (Y/N): ")?.ToUpper() != "Y") { Console.WriteLine("Đã hủy."); return; }

            radius += 5.0;
            retryCount++;
        }

        var (driverDistance, bestDriver) = bestMatch.Value;
        double totalDistance = driverDistance + destDistance;
        double fare = totalDistance * 12000;

        Console.WriteLine();
        UIHelper.Header("TÀI XẾ ĐƯỢC GHÉP CẶP");
        UIHelper.LabelValue("Tiêu chí:", strategyName);
        UIHelper.LabelValue("ID:", bestDriver.Id.ToString());
        UIHelper.LabelValue("Tên:", UIHelper.Truncate(bestDriver.Name, 30));
        UIHelper.LabelValue("Đánh giá:", $"{bestDriver.Rating:F1} sao ({bestDriver.RatingCount} đánh giá)");
        UIHelper.LabelValue("Khoảng cách:", $"{driverDistance:F2} km");
        UIHelper.LabelValue("Số chuyến:", bestDriver.TotalRides.ToString());
        UIHelper.Line();
        UIHelper.LabelValue("KC đến đích:", $"{destDistance:F2} km", 18);
        UIHelper.LabelValue("Tổng quãng đường:", $"{totalDistance:F2} km", 18);
        UIHelper.LabelValue("Giá cước:", $"{fare:N0} VND", 18);
        UIHelper.Line();

        if (UIHelper.Prompt("\nXác nhận đặt xe? (Y/N): ")?.ToUpper() == "Y")
        {
            rideManager.CreateRideAndStart(customer.Id, bestDriver.Id, totalDistance);
            Console.WriteLine();
            UIHelper.Success("Đặt xe thành công! Chuyến đi đã khởi hành.");
            UIHelper.Info("Bạn có thể hủy trong vòng 1 phút đầu.");
        }
        else
        {
            Console.WriteLine("Đã hủy đặt xe.");
        }
    }

    private void ViewCurrentRide()
    {
        var customer = authManager.GetCurrentCustomer();
        if (customer == null) return;

        var activeRide = rideManager.GetActiveRide(customer.Id);
        if (activeRide == null)
        {
            Console.WriteLine("\nBan khong co chuyen di nao dang hoat dong.");
            return;
        }

        var driver = driverManager.FindDriverById(activeRide.DriverId);

        Console.WriteLine();
        UIHelper.Header("CHUYEN DI HIEN TAI");
        UIHelper.LabelValue("Ride ID:", activeRide.RideId.ToString());
        UIHelper.LabelValue("Tài xế:", UIHelper.Truncate(driver?.Name ?? "N/A", 30));
        UIHelper.LabelValue("Trang thai:", activeRide.Status);
        UIHelper.LabelValue("Quãng đường:", $"{activeRide.Distance:F2} km");
        UIHelper.LabelValue("Giá cước:", $"{activeRide.Fare:N0} VND");

        bool canCancel = false;
        int cancelTime = 0;

        if (activeRide.Status == "IN_PROGRESS")
        {
            UIHelper.LabelValue("TG con lai:", $"{activeRide.GetRemainingTravelTime()} giay");
            cancelTime = rideManager.GetRemainingCancelTimeForInProgress(activeRide.RideId);
            canCancel = cancelTime > 0;
            if (canCancel) UIHelper.LabelValue("Con huy duoc:", $"{cancelTime} giay");
        }
        else if (activeRide.Status == "PENDING")
        {
            cancelTime = activeRide.GetRemainingCancelTime();
            canCancel = cancelTime > 0;
            UIHelper.LabelValue("Con huy duoc:", $"{cancelTime} giay");
        }

        if (canCancel)
        {
            UIHelper.Line();
            UIHelper.Row("Nhan H de huy chuyen");
        }
        UIHelper.Line();

        if (canCancel && UIHelper.Prompt("\nNhap H de huy, Enter de quay lai: ")?.ToUpper() == "H")
        {
            if (activeRide.Status == "IN_PROGRESS")
                rideManager.CancelInProgressRide(activeRide.RideId);
            else
                rideManager.CancelRideById(activeRide.RideId);
        }
    }

    private void ViewRideHistory()
    {
        var customer = authManager.GetCurrentCustomer();
        if (customer == null) return;

        var completedRides = rideManager.GetRidesByCustomer(customer.Id)
            .Where(r => r.Status == "COMPLETED")
            .OrderByDescending(r => r.Timestamp)
            .ToList();

        if (completedRides.Count == 0)
        {
            Console.WriteLine("\nBan chua co chuyen di nao hoan thanh.");
            return;
        }

        Console.WriteLine($"\n--- LỊCH SỬ CHUYẾN ĐI ({completedRides.Count} chuyến) ---");
        UIHelper.RideTable.DrawHeader();

        foreach (var ride in completedRides.Take(20))
        {
            var driver = driverManager.FindDriverById(ride.DriverId);
            UIHelper.RideTable.DrawRow(
                ride.RideId,
                UIHelper.Truncate(driver?.Name ?? "N/A", 16),
                $"{ride.Distance:F1} km",
                $"{ride.Fare:N0} đ",
                ride.Timestamp.ToString("dd/MM/yy HH:mm")
            );
        }
        UIHelper.RideTable.DrawSeparator();
    }

    private void RateDriver()
    {
        var customer = authManager.GetCurrentCustomer();
        if (customer == null) return;

        var unratedRides = rideManager.GetUnratedCompletedRides(customer.Id);

        if (unratedRides.Count == 0)
        {
            Console.WriteLine("\nKhong co chuyen di nao can danh gia.");
            return;
        }

        Console.WriteLine($"\n--- CHUYẾN ĐI CHƯA ĐÁNH GIÁ ({unratedRides.Count} chuyến) ---");
        UIHelper.UnratedRideTable.DrawHeader();

        foreach (var ride in unratedRides)
        {
            var driver = driverManager.FindDriverById(ride.DriverId);
            UIHelper.UnratedRideTable.DrawRow(
                ride.RideId,
                UIHelper.Truncate(driver?.Name ?? "N/A", 18),
                $"{ride.Distance:F1} km",
                ride.Timestamp.ToString("dd/MM/yy HH:mm")
            );
        }
        UIHelper.UnratedRideTable.DrawSeparator();

        if (!int.TryParse(UIHelper.Prompt("\nNhap ID chuyen de danh gia (0 de quay lai): "), out int rideId) || rideId == 0)
            return;

        var rideToRate = unratedRides.FirstOrDefault(r => r.RideId == rideId);
        if (rideToRate == null) { UIHelper.Error("Không tìm thấy chuyến đi này."); return; }

        var rideDriver = driverManager.FindDriverById(rideToRate.DriverId);

        Console.WriteLine();
        UIHelper.Header($"Đánh giá: {UIHelper.Truncate(rideDriver?.Name ?? "N/A", 30)}");
        UIHelper.Row("1 sao - Rất tệ");
        UIHelper.Row("2 sao - Tệ");
        UIHelper.Row("3 sao - Bình thường");
        UIHelper.Row("4 sao - Tốt");
        UIHelper.Row("5 sao - Xuất sắc");
        UIHelper.Line();

        if (!int.TryParse(UIHelper.Prompt("Nhập số sao (1-5): "), out int stars) || stars < 1 || stars > 5)
        {
            UIHelper.Error("Số sao không hợp lệ.");
            return;
        }

        if (rideManager.RateRide(rideId, stars, driverManager))
            UIHelper.Success($"Đã đánh giá {stars} sao. Cảm ơn bạn!");
        else
            UIHelper.Error("Không thể đánh giá. Vui lòng thử lại.");
    }

    private void ChangePassword()
    {
        Console.WriteLine("\n--- ĐỔI MẬT KHẨU ---");
        string? oldPassword = UIHelper.Prompt("Nhập mật khẩu hiện tại: ");
        string? newPassword = UIHelper.Prompt("Nhập mật khẩu mới: ");
        string? confirmPassword = UIHelper.Prompt("Xác nhận mật khẩu mới: ");

        if (newPassword != confirmPassword) { UIHelper.Error("Mật khẩu xác nhận không khớp."); return; }
        if (string.IsNullOrWhiteSpace(newPassword)) { UIHelper.Error("Mật khẩu mới không được để trống."); return; }

        if (authManager.ChangePassword(oldPassword ?? "", newPassword))
            UIHelper.Success("Đổi mật khẩu thành công!");
        else
            UIHelper.Error("Đổi mật khẩu thất bại. Mật khẩu hiện tại không đúng.");
    }
}
