namespace MinRide.Menus;

using MinRide.Auth;
using MinRide.Managers;
using MinRide.Models;
using MinRide.Utils;

/// <summary>
/// Menu for admin users with full system access.
/// </summary>
public class AdminMenu
{
    private readonly AuthManager authManager;
    private readonly CustomerManager customerManager;
    private readonly DriverManager driverManager;
    private readonly RideManager rideManager;
    private readonly UndoStack undoStack;

    public AdminMenu(AuthManager auth, CustomerManager cm, DriverManager dm, RideManager rm)
    {
        authManager = auth;
        customerManager = cm;
        driverManager = dm;
        rideManager = rm;
        undoStack = new UndoStack();
    }

    public void Show()
    {
        bool back = false;

        while (!back && authManager.Session.IsLoggedIn)
        {
            Console.WriteLine();
            UIHelper.Line();
            UIHelper.Title("MINRIDE - ADMIN");
            UIHelper.Title("Xin chào, Administrator");
            UIHelper.Line();
            UIHelper.MenuItem("1", "Quản lý tài xế");
            UIHelper.MenuItem("2", "Quản lý khách hàng");
            UIHelper.MenuItem("3", "Quản lý chuyến đi");
            UIHelper.MenuItem("4", "Tìm tài xế phù hợp");
            UIHelper.MenuItem("5", "Đặt xe");
            UIHelper.MenuItem("6", "Tự động ghép cặp");
            UIHelper.MenuItem("7", "Undo");
            UIHelper.MenuItem("8", "Lưu dữ liệu");
            UIHelper.MenuItem("9", "Đổi mật khẩu");
            UIHelper.MenuItem("10", "Đăng xuất");
            UIHelper.MenuItem("0", "Thoát chương trình");
            UIHelper.Line();

            switch (UIHelper.PromptChoice("Chọn chức năng: "))
            {
                case "1": ManageDrivers(); break;
                case "2": ManageCustomers(); break;
                case "3": ManageRides(); break;
                case "4": FindSuitableDrivers(); break;
                case "5": BookRide(); break;
                case "6": AutoMatchRide(); break;
                case "7": undoStack.Undo(); break;
                case "8": SaveData(); break;
                case "9": ChangePassword(); break;
                case "10":
                    authManager.Logout();
                    UIHelper.Success("Đã đăng xuất.");
                    back = true;
                    break;
                case "0":
                    SaveData();
                    Console.WriteLine("Tạm biệt!");
                    Environment.Exit(0);
                    break;
                default:
                    UIHelper.Error("Lựa chọn không hợp lệ.");
                    break;
            }
        }
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
            UIHelper.Error("Đổi mật khẩu thất bại.");
    }

    private void ManageDrivers()
    {
        bool back = false;

        while (!back)
        {
            Console.WriteLine();
            UIHelper.Header("QUẢN LÝ TÀI XẾ");
            UIHelper.MenuItem("1", "Hiển thị tất cả tài xế");
            UIHelper.MenuItem("2", "Thêm tài xế mới");
            UIHelper.MenuItem("3", "Cập nhật tài xế theo ID");
            UIHelper.MenuItem("4", "Xóa tài xế theo ID");
            UIHelper.MenuItem("5", "Tìm kiếm tài xế theo tên");
            UIHelper.MenuItem("6", "Tìm kiếm tài xế theo ID");
            UIHelper.MenuItem("7", "Quay lại");
            UIHelper.Line();

            switch (UIHelper.PromptChoice("Chọn: "))
            {
                case "1": DisplayAllDrivers(); break;
                case "2": AddDriver(); break;
                case "3": UpdateDriver(); break;
                case "4": DeleteDriver(); break;
                case "5": SearchDriverByName(); break;
                case "6": SearchDriverById(); break;
                case "7": back = true; break;
                default: UIHelper.Error("Lựa chọn không hợp lệ."); break;
            }
        }
    }

    private void DisplayAllDrivers()
    {
        var drivers = driverManager.GetAll();
        if (drivers.Count == 0)
        {
            Console.WriteLine("\nChua co tai xe nao trong he thong.");
            return;
        }

        Console.WriteLine($"\n--- DANH SACH TAI XE ({drivers.Count}) ---");
        UIHelper.DriverTable.DrawHeader();

        int stt = 1;
        foreach (var d in drivers)
        {
            UIHelper.DriverTable.DrawRow(
                stt++, d.Id,
                UIHelper.Truncate(d.Name, 18),
                d.Rating,
                $"({d.Location.X:F1},{d.Location.Y:F1})",
                d.TotalRides
            );
        }
        UIHelper.DriverTable.DrawSeparator();
    }

    private void AddDriver()
    {
            Console.WriteLine("\n--- THÊM TÀI XẾ MỚI ---");

        if (!int.TryParse(UIHelper.Prompt("Nhập ID: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }
        if (driverManager.FindDriverById(id) != null) { UIHelper.Error("ID đã tồn tại."); return; }

        string? name = UIHelper.Prompt("Nhập tên: ");
        if (string.IsNullOrEmpty(name)) { UIHelper.Error("Tên không được để trống."); return; }

        if (!double.TryParse(UIHelper.Prompt("Nhập đánh giá (0-5): "), out double rating) || rating < 0 || rating > 5)
        { UIHelper.Error("Đánh giá phải từ 0 đến 5."); return; }

        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ X: "), out double x)) { UIHelper.Error("Tọa độ X không hợp lệ."); return; }
        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ Y: "), out double y)) { UIHelper.Error("Tọa độ Y không hợp lệ."); return; }

        var driver = new Driver(id, name, rating, x, y);
        driverManager.AddDriver(driver);
    }

    private void UpdateDriver()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID tài xế cần cập nhật: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }

        var driver = driverManager.FindDriverById(id);
        if (driver == null) { UIHelper.Error("Không tìm thấy tài xế."); return; }

        Console.WriteLine();
        UIHelper.Header("THÔNG TIN TÀI XẾ");
        UIHelper.LabelValue("ID:", driver.Id.ToString());
        UIHelper.LabelValue("Tên:", UIHelper.Truncate(driver.Name, 30));
        UIHelper.LabelValue("Đánh giá:", $"{driver.Rating:F1} sao");
        UIHelper.LabelValue("Vị trí:", $"({driver.Location.X:F1}, {driver.Location.Y:F1})");
        UIHelper.LabelValue("Số chuyến:", driver.TotalRides.ToString());
        UIHelper.Line();

        Console.WriteLine("\nChọn thông tin cần cập nhật:");
        Console.WriteLine("1. Tên");
        Console.WriteLine("2. Vị trí (X, Y)");
        Console.WriteLine("3. Hủy");

        switch (UIHelper.PromptChoice("Lựa chọn: "))
        {
            case "1":
                string? newName = UIHelper.Prompt("Nhập tên mới: ");
                if (!string.IsNullOrEmpty(newName)) { driver.Name = newName; UIHelper.Success("Đã cập nhật tên."); }
                break;
            case "2":
                if (double.TryParse(UIHelper.Prompt("Nhập tọa độ X mới: "), out double newX) &&
                    double.TryParse(UIHelper.Prompt("Nhập tọa độ Y mới: "), out double newY))
                {
                    driver.Location = (newX, newY);
                    UIHelper.Success("Đã cập nhật vị trí.");
                }
                break;
            case "3": Console.WriteLine("Đã hủy."); break;
            default: UIHelper.Error("Lựa chọn không hợp lệ."); break;
        }
    }

    private void DeleteDriver()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID tài xế cần xóa: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }

        if (driverManager.DeleteDriver(id))
            UIHelper.Success($"Đã xóa tài xế ID: {id}");
        else
            UIHelper.Error("Không tìm thấy tài xế.");
    }

    private void SearchDriverByName()
    {
        string? name = UIHelper.Prompt("Nhập tên tài xế cần tìm: ");
        if (string.IsNullOrEmpty(name)) { UIHelper.Error("Tên không được để trống."); return; }

        var drivers = driverManager.FindDriversByName(name);
        if (drivers.Count == 0) { Console.WriteLine($"Không tìm thấy tài xế có tên chứa \"{name}\"."); return; }

        Console.WriteLine($"\n--- TIM THAY {drivers.Count} TAI XE ---");
        foreach (var d in drivers)
            Console.WriteLine($"  ID: {d.Id,4} | {UIHelper.Truncate(d.Name, 20),-20} | Rating: {d.Rating:F1} | Vi tri: ({d.Location.X:F1}, {d.Location.Y:F1})");
    }

    private void SearchDriverById()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID tài xế: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }

        var driver = driverManager.FindDriverById(id);
        if (driver == null) { UIHelper.Error("Không tìm thấy tài xế."); return; }

        Console.WriteLine();
        UIHelper.Header("THÔNG TIN TÀI XẾ");
        UIHelper.LabelValue("ID:", driver.Id.ToString());
        UIHelper.LabelValue("Tên:", UIHelper.Truncate(driver.Name, 30));
        UIHelper.LabelValue("Đánh giá:", $"{driver.Rating:F1} sao ({driver.RatingCount} đánh giá)");
        UIHelper.LabelValue("Vị trí:", $"({driver.Location.X:F1}, {driver.Location.Y:F1})");
        UIHelper.LabelValue("Số chuyến:", driver.TotalRides.ToString());
        UIHelper.Line();
    }

    private void ManageCustomers()
    {
        bool back = false;

        while (!back)
        {
            Console.WriteLine();
            UIHelper.Header("QUẢN LÝ KHÁCH HÀNG");
            UIHelper.MenuItem("1", "Hiển thị tất cả khách hàng");
            UIHelper.MenuItem("2", "Thêm khách hàng mới");
            UIHelper.MenuItem("3", "Cập nhật khách hàng theo ID");
            UIHelper.MenuItem("4", "Xóa khách hàng theo ID");
            UIHelper.MenuItem("5", "Tìm kiếm khách hàng theo tên");
            UIHelper.MenuItem("6", "Tìm kiếm khách hàng theo ID");
            UIHelper.MenuItem("7", "Quay lại");
            UIHelper.Line();

            switch (UIHelper.PromptChoice("Chọn: "))
            {
                case "1": DisplayAllCustomers(); break;
                case "2": AddCustomer(); break;
                case "3": customerManager.UpdateCustomerInteractive(); break;
                case "4": DeleteCustomer(); break;
                case "5": SearchCustomerByName(); break;
                case "6": SearchCustomerById(); break;
                case "7": back = true; break;
                default: UIHelper.Error("Lựa chọn không hợp lệ."); break;
            }
        }
    }

    private void DisplayAllCustomers()
    {
        var customers = customerManager.GetAll();
        if (customers.Count == 0)
        {
            Console.WriteLine("\nChưa có khách hàng nào trong hệ thống.");
            return;
        }

        Console.WriteLine($"\n--- DANH SÁCH KHÁCH HÀNG ({customers.Count}) ---");
        UIHelper.CustomerTable.DrawHeader();

        int stt = 1;
        foreach (var c in customers)
        {
            UIHelper.CustomerTable.DrawRow(
                stt++, c.Id,
                UIHelper.Truncate(c.Name, 18),
                UIHelper.Truncate(c.District, 14),
                $"({c.Location.X:F1},{c.Location.Y:F1})"
            );
        }
        UIHelper.CustomerTable.DrawSeparator();
    }

    private void AddCustomer()
    {
        Console.WriteLine("\n--- THÊM KHÁCH HÀNG MỚI ---");

        if (!int.TryParse(UIHelper.Prompt("Nhập ID: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }

        string? name = UIHelper.Prompt("Nhập tên: ");
        string? district = UIHelper.Prompt("Nhập quận/huyện: ");

        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ X: "), out double x)) { UIHelper.Error("Tọa độ X không hợp lệ."); return; }
        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ Y: "), out double y)) { UIHelper.Error("Tọa độ Y không hợp lệ."); return; }

        var customer = new Customer(id, name ?? "", district ?? "", x, y);
        if (customerManager.AddCustomerWithValidation(customer, out string? error))
            UIHelper.Success("Đã thêm khách hàng.");
        else
            UIHelper.Error($"Lỗi: {error}");
    }

    private void DeleteCustomer()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID khách hàng cần xóa: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }

        if (customerManager.DeleteCustomer(id))
            UIHelper.Success($"Đã xóa khách hàng ID: {id}");
        else
            UIHelper.Error("Không tìm thấy khách hàng.");
    }

    private void SearchCustomerByName()
    {
        string? name = UIHelper.Prompt("Nhập tên khách hàng cần tìm: ");
        if (string.IsNullOrEmpty(name)) { UIHelper.Error("Tên không được để trống."); return; }

        var customers = customerManager.FindCustomersByName(name);
        if (customers.Count == 0) { Console.WriteLine($"Không tìm thấy khách hàng có tên chứa \"{name}\"."); return; }

        Console.WriteLine($"\n--- TÌM THẤY {customers.Count} KHÁCH HÀNG ---");
        foreach (var c in customers)
            Console.WriteLine($"  ID: {c.Id,4} | {UIHelper.Truncate(c.Name, 20),-20} | {UIHelper.Truncate(c.District, 15),-15} | ({c.Location.X:F1}, {c.Location.Y:F1})");
    }

    private void SearchCustomerById()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID khách hàng: "), out int id)) { UIHelper.Error("ID không hợp lệ."); return; }

        var customer = customerManager.FindCustomerById(id);
        if (customer == null) { UIHelper.Error("Không tìm thấy khách hàng."); return; }

        Console.WriteLine();
        UIHelper.Header("THÔNG TIN KHÁCH HÀNG");
        UIHelper.LabelValue("ID:", customer.Id.ToString());
        UIHelper.LabelValue("Tên:", UIHelper.Truncate(customer.Name, 30));
        UIHelper.LabelValue("Quận/Huyện:", UIHelper.Truncate(customer.District, 30));
        UIHelper.LabelValue("Vị trí:", $"({customer.Location.X:F1}, {customer.Location.Y:F1})");
        UIHelper.Line();
    }

    private void ManageRides()
    {
        bool back = false;

        while (!back)
        {
            rideManager.ProcessRides(driverManager);
            var (pending, inProgress, completed) = rideManager.GetRideCounts();

            Console.WriteLine();
            UIHelper.Header("QUẢN LÝ CHUYẾN ĐI");
            UIHelper.Row($"Trạng thái: {pending} chờ | {inProgress} đang chạy | {completed} xong");
            UIHelper.Line();
            UIHelper.MenuItem("1", "Xem chuyến đi đang chờ");
            UIHelper.MenuItem("2", "Xem chuyến đi đang chạy");
            UIHelper.MenuItem("3", "Xem lịch sử chuyến đi");
            UIHelper.MenuItem("4", "Hủy chuyến đi (trong 2 phút)");
            UIHelper.MenuItem("5", "Quay lại");
            UIHelper.Line();

            switch (UIHelper.PromptChoice("Chọn: "))
            {
                case "1": rideManager.DisplayPendingRides(); break;
                case "2": rideManager.DisplayInProgressRides(); break;
                case "3": DisplayRideHistory(); break;
                case "4": CancelRide(); break;
                case "5": back = true; break;
                default: UIHelper.Error("Lựa chọn không hợp lệ."); break;
            }
        }
    }

    private void DisplayRideHistory()
    {
        var rides = rideManager.GetAllRides();
        if (rides.Count == 0) { Console.WriteLine("\nChưa có lịch sử chuyến đi."); return; }

        Console.WriteLine($"\n--- LỊCH SỬ CHUYẾN ĐI ({rides.Count}) ---");
        foreach (var r in rides.OrderByDescending(r => r.Timestamp).Take(20))
        {
            string rating = r.CustomerRating.HasValue ? $"{r.CustomerRating} sao" : "Chưa";
            Console.WriteLine($"  #{r.RideId,-4} | C{r.CustomerId,-3} -> D{r.DriverId,-3} | {r.Distance,5:F1}km | {r.Fare,10:N0}đ | {UIHelper.FormatStatus(r.Status),-10} | {rating}");
        }
    }

    private void CancelRide()
    {
        rideManager.DisplayPendingRides();
        if (!int.TryParse(UIHelper.Prompt("\nNhập ID chuyến cần hủy (0 để quay lại): "), out int rideId) || rideId == 0) return;
        rideManager.CancelRideById(rideId);
    }

    private void FindSuitableDrivers()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID khách hàng: "), out int customerId)) { UIHelper.Error("ID không hợp lệ."); return; }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null) { UIHelper.Error("Không tìm thấy khách hàng."); return; }

        if (!double.TryParse(UIHelper.Prompt("Nhập bán kính tìm kiếm (km): "), out double radius) || radius <= 0)
        { UIHelper.Error("Bán kính không hợp lệ."); return; }

        var nearbyDrivers = driverManager.FindNearbyDrivers(customer.Location, radius);
        if (nearbyDrivers.Count == 0) { Console.WriteLine("Không tìm thấy tài xế trong bán kính."); return; }

        Console.WriteLine($"\n--- TÀI XẾ GẦN {customer.Name} (bán kính {radius}km) ---");
        foreach (var (distance, driver) in nearbyDrivers)
            Console.WriteLine($"  {distance,5:F2}km - ID: {driver.Id,4} | {UIHelper.Truncate(driver.Name, 20),-20} | Đánh giá: {driver.Rating:F1} sao");
    }

    private void BookRide()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID khách hàng: "), out int customerId)) { UIHelper.Error("ID không hợp lệ."); return; }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null) { UIHelper.Error("Không tìm thấy khách hàng."); return; }

        if (!int.TryParse(UIHelper.Prompt("Nhập ID tài xế: "), out int driverId)) { UIHelper.Error("ID không hợp lệ."); return; }

        var driver = driverManager.FindDriverById(driverId);
        if (driver == null) { UIHelper.Error("Không tìm thấy tài xế."); return; }

        double driverToCustomer = driver.DistanceTo(customer.Location);
        Console.WriteLine($"Khoảng cách tài xế đến điểm đón: {driverToCustomer:F2} km");

        if (!double.TryParse(UIHelper.Prompt("Nhập khoảng cách đến điểm đích (km): "), out double destDistance) || destDistance < 0)
        { UIHelper.Error("Khoảng cách không hợp lệ."); return; }

        double totalDistance = driverToCustomer + destDistance;
        double fare = totalDistance * 12000;

        Console.WriteLine($"\nTổng quãng đường: {totalDistance:F2} km");
        Console.WriteLine($"Giá cước: {fare:N0} VND");

        if (UIHelper.Prompt("Xác nhận đặt xe? (Y/N): ")?.ToUpper() == "Y")
        {
            rideManager.CreateRide(customerId, driverId, totalDistance);
            UIHelper.Success("Đặt xe thành công!");
        }
    }

    private void AutoMatchRide()
    {
        if (!int.TryParse(UIHelper.Prompt("Nhập ID khách hàng: "), out int customerId)) { UIHelper.Error("ID không hợp lệ."); return; }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null) { UIHelper.Error("Không tìm thấy khách hàng."); return; }

        if (!double.TryParse(UIHelper.Prompt("Nhập khoảng cách đến điểm đích (km): "), out double destDistance) || destDistance < 0)
        { UIHelper.Error("Khoảng cách không hợp lệ."); return; }

        Console.WriteLine();
        UIHelper.Header("CHỌN TIÊU CHÍ TÌM TÀI XẾ");
        UIHelper.MenuItem("1", "Tài xế gần nhất");
        UIHelper.MenuItem("2", "Tài xế đánh giá cao nhất (bán kính 5km)");
        UIHelper.MenuItem("3", "Cân bằng (khoảng cách + đánh giá)");
        UIHelper.Line();

        int strategy = UIHelper.PromptChoice("Chọn tiêu chí (1-3): ") switch { "2" => 2, "3" => 3, _ => 1 };
        double radius = strategy switch { 1 => 10.0, 2 => 5.0, 3 => 7.0, _ => 10.0 };

        var nearbyDrivers = driverManager.FindNearbyDrivers(customer.Location, radius);
        if (nearbyDrivers.Count == 0) { Console.WriteLine("Không tìm thấy tài xế gần đây."); return; }

        (double Distance, Driver Driver) bestMatch;
        string strategyName;

        switch (strategy)
        {
            case 2:
                bestMatch = nearbyDrivers.OrderByDescending(d => d.Driver.Rating).First();
                strategyName = "Đánh giá cao nhất";
                break;
            case 3:
                double maxDist = nearbyDrivers.Max(d => d.Distance);
                if (maxDist == 0) maxDist = 1;
                bestMatch = nearbyDrivers
                    .Select(d => new { d.Distance, d.Driver, Score = ((maxDist - d.Distance) / maxDist) * 0.6 + (d.Driver.Rating / 5.0) * 0.4 })
                    .OrderByDescending(d => d.Score)
                    .Select(d => (d.Distance, d.Driver))
                    .First();
                strategyName = "Cân bằng";
                break;
            default:
                bestMatch = nearbyDrivers[0];
                strategyName = "Gần nhất";
                break;
        }

        var (distance, bestDriver) = bestMatch;
        double totalDistance = distance + destDistance;

        Console.WriteLine();
        UIHelper.Header("KẾT QUẢ GHÉP CẶP");
        UIHelper.LabelValue("Tiêu chí:", strategyName);
        UIHelper.LabelValue("Tài xế:", UIHelper.Truncate(bestDriver.Name, 30));
        UIHelper.LabelValue("Đánh giá:", $"{bestDriver.Rating:F1} sao");
        UIHelper.LabelValue("Khoảng cách:", $"{distance:F2} km");
        UIHelper.LabelValue("Tổng quãng đường:", $"{totalDistance:F2} km");
        UIHelper.LabelValue("Giá cước:", $"{(totalDistance * 12000):N0} VND");
        UIHelper.Line();

        rideManager.CreateRide(customerId, bestDriver.Id, totalDistance);
        UIHelper.Success("Đã tự động ghép cặp thành công!");
    }

    private void SaveData()
    {
        Directory.CreateDirectory("Data");
        FileHandler.SaveDrivers("Data/drivers.csv", driverManager.GetAll());
        FileHandler.SaveCustomers("Data/customers.csv", customerManager.GetAll());
        FileHandler.SaveRides("Data/rides.csv", rideManager.GetAllRides());
        authManager.SavePasswords();
        UIHelper.Success("Đã lưu tất cả dữ liệu!");
    }
}
