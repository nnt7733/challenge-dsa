using MinRide.Managers;
using MinRide.Models;
using MinRide.Utils;

namespace MinRide;

/// <summary>
/// Main system class that coordinates all MinRide operations.
/// </summary>
public class MinRideSystem
{
    private DriverManager driverManager;
    private CustomerManager customerManager;
    private RideManager rideManager;
    private UndoStack undoStack;

    private const string DriversFilePath = "Data/drivers.csv";
    private const string CustomersFilePath = "Data/customers.csv";
    private const string RidesFilePath = "Data/rides.csv";

    /// <summary>
    /// Initializes a new instance of the <see cref="MinRideSystem"/> class.
    /// </summary>
    public MinRideSystem()
    {
        driverManager = new DriverManager();
        customerManager = new CustomerManager();
        rideManager = new RideManager();
        undoStack = new UndoStack();

        LoadData();
    }

    /// <summary>
    /// Loads data from CSV files.
    /// Validates rides (driver and customer must exist) and syncs TotalRides.
    /// </summary>
    private void LoadData()
    {
        Console.WriteLine("Đang tải dữ liệu...");
        int driverCount = 0, customerCount = 0, rideCount = 0, invalidRideCount = 0;

        // Step 1: Load drivers (reset TotalRides to 0)
        try
        {
            if (File.Exists(DriversFilePath))
            {
                var drivers = FileHandler.LoadDrivers(DriversFilePath);
                foreach (var driver in drivers)
                {
                    // Reset TotalRides to 0, will be synced from actual rides
                    driver.SetTotalRides(0);
                    driverManager.AddDriver(driver, silent: true);
                    driverCount++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Lỗi tải tài xế: {ex.Message}");
        }

        // Step 2: Load customers
        try
        {
            if (File.Exists(CustomersFilePath))
            {
                var customers = FileHandler.LoadCustomers(CustomersFilePath);
                foreach (var customer in customers)
                {
                    customerManager.AddCustomer(customer, silent: true);
                    customerCount++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Lỗi tải khách hàng: {ex.Message}");
        }

        // Step 3: Load rides with validation
        try
        {
            if (File.Exists(RidesFilePath))
            {
                var rides = FileHandler.LoadRides(RidesFilePath);
                foreach (var ride in rides)
                {
                    // Validate: driver and customer must exist
                    var driver = driverManager.FindDriverById(ride.DriverId);
                    var customer = customerManager.FindCustomerById(ride.CustomerId);

                    if (driver == null || customer == null)
                    {
                        invalidRideCount++;
                        continue; // Skip invalid ride
                    }

                    // Add valid ride to RideManager
                    rideManager.AddRideFromHistory(ride);
                    rideCount++;

                    // Sync TotalRides: increment for confirmed rides
                    if (ride.Status == "CONFIRMED")
                    {
                        driver.IncrementRides();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Lỗi tải chuyến đi: {ex.Message}");
        }

        // Show summary
        Console.WriteLine($"✓ Đã tải: {driverCount} tài xế | {customerCount} khách hàng | {rideCount} chuyến đi");
        if (invalidRideCount > 0)
        {
            Console.WriteLine($"⚠ Bỏ qua {invalidRideCount} chuyến đi không hợp lệ (tài xế/khách hàng không tồn tại)");
        }
    }

    /// <summary>
    /// Runs the main application loop.
    /// </summary>
    public void Run()
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║           MINRIDE - MENU CHÍNH         ║");
            Console.WriteLine("╠════════════════════════════════════════╣");
            Console.WriteLine("║  1. Quản lý tài xế                     ║");
            Console.WriteLine("║  2. Quản lý khách hàng                 ║");
            Console.WriteLine("║  3. Quản lý chuyến đi                  ║");
            Console.WriteLine("║  4. Tìm tài xế phù hợp                 ║");
            Console.WriteLine("║  5. Đặt xe                             ║");
            Console.WriteLine("║  6. Tự động ghép cặp                   ║");
            Console.WriteLine("║  7. Undo                               ║");
            Console.WriteLine("║  8. Lưu dữ liệu                        ║");
            Console.WriteLine("║  9. Thoát                              ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.Write("Chọn chức năng: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ManageDrivers();
                    break;
                case "2":
                    ManageCustomers();
                    break;
                case "3":
                    ManageRides();
                    break;
                case "4":
                    FindSuitableDrivers();
                    break;
                case "5":
                    BookRide();
                    break;
                case "6":
                    AutoMatchRide();
                    break;
                case "7":
                    undoStack.Undo();
                    break;
                case "8":
                    SaveData();
                    break;
                case "9":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                    break;
            }
        }
    }

    /// <summary>
    /// Driver management sub-menu.
    /// </summary>
    private void ManageDrivers()
    {
        bool back = false;

        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("┌──────────────────────────────────────────────┐");
            Console.WriteLine("│            QUẢN LÝ TÀI XẾ                    │");
            Console.WriteLine("├──────────────────────────────────────────────┤");
            Console.WriteLine("│  1. Hiển thị tất cả tài xế                   │");
            Console.WriteLine("│  2. Hiển thị top K tài xế (rating cao nhất)  │");
            Console.WriteLine("│  3. Hiển thị top K tài xế (rating thấp nhất) │");
            Console.WriteLine("│  4. Thêm tài xế mới                          │");
            Console.WriteLine("│  5. Cập nhật tài xế theo tên                 │");
            Console.WriteLine("│  6. Cập nhật tài xế theo ID                  │");
            Console.WriteLine("│  7. Xóa tài xế theo ID                       │");
            Console.WriteLine("│  8. Tìm kiếm tài xế theo tên                 │");
            Console.WriteLine("│  9. Tìm kiếm tài xế theo ID                  │");
            Console.WriteLine("│ 10. Sắp xếp danh sách theo rating (tăng dần) │");
            Console.WriteLine("│ 11. Sắp xếp danh sách theo rating (giảm dần) │");
            Console.WriteLine("│ 12. Quay lại menu chính                      │");
            Console.WriteLine("└──────────────────────────────────────────────┘");
            Console.Write("Chọn chức năng (1-12): ");

            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    DisplayAllDrivers();
                    break;
                case "2":
                    DisplayTopKDrivers(highest: true);
                    break;
                case "3":
                    DisplayTopKDrivers(highest: false);
                    break;
                case "4":
                    AddDriverWithValidation();
                    break;
                case "5":
                    driverManager.UpdateDriverByName();
                    break;
                case "6":
                    UpdateDriverById();
                    break;
                case "7":
                    DeleteDriverWithConstraintCheck();
                    break;
                case "8":
                    SearchDriverByName();
                    break;
                case "9":
                    SearchDriverById();
                    break;
                case "10":
                    SortDriversAscending();
                    break;
                case "11":
                    SortDriversDescending();
                    break;
                case "12":
                    back = true;
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    private void DisplayAllDrivers()
    {
        Console.WriteLine("\n--- DANH SÁCH TẤT CẢ TÀI XẾ ---");
        var drivers = driverManager.GetAll();
        if (drivers.Count == 0)
        {
            Console.WriteLine("Chưa có tài xế nào trong hệ thống.");
            return;
        }
        Console.WriteLine($"Tổng số: {drivers.Count} tài xế\n");
        driverManager.DisplayAll();
    }

    private void DisplayTopKDrivers(bool highest)
    {
        string label = highest ? "RATING CAO NHẤT" : "RATING THẤP NHẤT";
        Console.Write($"Nhập số lượng K (Top K {label}): ");

        if (!TryReadInt(out int k) || k <= 0)
        {
            Console.WriteLine("K phải là số nguyên dương.");
            return;
        }

        var topDrivers = driverManager.GetTopK(k, highest);

        if (topDrivers.Count == 0)
        {
            Console.WriteLine("Không có tài xế nào.");
            return;
        }

        Console.WriteLine($"\n--- TOP {topDrivers.Count} TÀI XẾ {label} ---");
        Console.WriteLine("┌─────┬────────┬────────────────────────┬────────┬─────────────────┬───────────┐");
        Console.WriteLine("│ STT │   ID   │          Tên           │ Rating │     Vị trí      │ Số chuyến │");
        Console.WriteLine("├─────┼────────┼────────────────────────┼────────┼─────────────────┼───────────┤");

        int stt = 1;
        foreach (var d in topDrivers)
        {
            string name = d.Name.Length > 22 ? d.Name.Substring(0, 19) + "..." : d.Name;
            Console.WriteLine($"│ {stt,3} │ {d.Id,6} │ {name,-22} │ {d.Rating,6:F1} │ ({d.Location.X:F1}, {d.Location.Y:F1}){"",-5} │ {d.TotalRides,9} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────┴────────────────────────┴────────┴─────────────────┴───────────┘");
    }

    private void AddDriverWithValidation()
    {
        Console.WriteLine("\n--- THÊM TÀI XẾ MỚI ---");

        Console.Write("Nhập ID: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        if (driverManager.FindDriverById(id) != null)
        {
            Console.WriteLine("✗ Lỗi: ID đã tồn tại trong hệ thống.");
            return;
        }

        Console.Write("Nhập tên: ");
        string? name = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("✗ Lỗi: Tên không được để trống.");
            return;
        }

        Console.Write("Nhập rating (0.0 - 5.0): ");
        if (!TryReadDouble(out double rating) || rating < 0 || rating > 5)
        {
            Console.WriteLine("✗ Lỗi: Rating phải từ 0.0 đến 5.0.");
            return;
        }

        Console.Write("Nhập tọa độ X: ");
        if (!TryReadDouble(out double x))
        {
            Console.WriteLine("✗ Lỗi: Tọa độ X không hợp lệ.");
            return;
        }

        Console.Write("Nhập tọa độ Y: ");
        if (!TryReadDouble(out double y))
        {
            Console.WriteLine("✗ Lỗi: Tọa độ Y không hợp lệ.");
            return;
        }

        try
        {
            var driver = new Driver(id, name, rating, x, y);

            if (driverManager.AddDriverWithValidation(driver, out string? errorMessage))
            {
                // Push undo action
                undoStack.Push(() =>
                {
                    driverManager.DeleteDriver(id);
                    Console.WriteLine($"Đã hoàn tác thêm tài xế {name}");
                });

                Console.WriteLine($"\n✓ Thành công! Đã thêm tài xế:");
                driver.DisplayDetailed();
            }
            else
            {
                Console.WriteLine($"✗ Lỗi: {errorMessage}");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✗ Lỗi: {ex.Message}");
        }
    }

    private void UpdateDriverById()
    {
        Console.Write("Nhập ID tài xế cần cập nhật: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var driver = driverManager.FindDriverById(id);
        if (driver == null)
        {
            Console.WriteLine("Không tìm thấy tài xế với ID này.");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN HIỆN TẠI ---");
        driver.DisplayDetailed();

        // Store old values for undo
        string oldName = driver.Name;
        double oldRating = driver.Rating;
        double oldX = driver.Location.X;
        double oldY = driver.Location.Y;

        Console.WriteLine("\nChọn thông tin cần cập nhật:");
        Console.WriteLine("1. Tên");
        Console.WriteLine("2. Rating");
        Console.WriteLine("3. Vị trí (X, Y)");
        Console.WriteLine("4. Cập nhật tất cả");
        Console.WriteLine("5. Hủy");
        Console.Write("Lựa chọn: ");

        string? choice = Console.ReadLine()?.Trim();

        if (choice == "5")
        {
            Console.WriteLine("Đã hủy cập nhật.");
            return;
        }

        bool updated = false;

        switch (choice)
        {
            case "1":
                Console.Write("Nhập tên mới: ");
                string? newName = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(newName))
                {
                    driver.Name = newName;
                    updated = true;
                }
                break;

            case "2":
                Console.Write("Nhập rating mới (0-5): ");
                if (TryReadDouble(out double newRating) && newRating >= 0 && newRating <= 5)
                {
                    driver.SetRating(newRating);
                    updated = true;
                }
                else
                {
                    Console.WriteLine("Rating không hợp lệ.");
                    return;
                }
                break;

            case "3":
                Console.Write("Nhập tọa độ X mới: ");
                if (!TryReadDouble(out double newX))
                {
                    Console.WriteLine("Tọa độ X không hợp lệ.");
                    return;
                }
                Console.Write("Nhập tọa độ Y mới: ");
                if (!TryReadDouble(out double newY))
                {
                    Console.WriteLine("Tọa độ Y không hợp lệ.");
                    return;
                }
                driver.Location = (newX, newY);
                updated = true;
                break;

            case "4":
                Console.Write("Nhập tên mới (Enter để giữ nguyên): ");
                string? inputName = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputName))
                {
                    driver.Name = inputName;
                    updated = true;
                }

                Console.Write("Nhập rating mới (Enter để giữ nguyên): ");
                string? inputRating = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputRating) && double.TryParse(inputRating, out double r) && r >= 0 && r <= 5)
                {
                    driver.SetRating(r);
                    updated = true;
                }

                Console.Write("Nhập tọa độ X mới (Enter để giữ nguyên): ");
                string? inputX = Console.ReadLine()?.Trim();
                Console.Write("Nhập tọa độ Y mới (Enter để giữ nguyên): ");
                string? inputY = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(inputX) || !string.IsNullOrEmpty(inputY))
                {
                    double x = !string.IsNullOrEmpty(inputX) && double.TryParse(inputX, out double xv) ? xv : oldX;
                    double y = !string.IsNullOrEmpty(inputY) && double.TryParse(inputY, out double yv) ? yv : oldY;
                    driver.Location = (x, y);
                    updated = true;
                }
                break;

            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                return;
        }

        if (updated)
        {
            // Push undo action
            undoStack.Push(() =>
            {
                driver.Name = oldName;
                driver.SetRating(oldRating);
                driver.Location = (oldX, oldY);
                Console.WriteLine($"Đã hoàn tác cập nhật tài xế ID {id}");
            });

            Console.WriteLine("\n✓ Cập nhật thành công!");
            Console.WriteLine("--- THÔNG TIN SAU CẬP NHẬT ---");
            driver.DisplayDetailed();
        }
    }

    private void DeleteDriverWithConstraintCheck()
    {
        Console.Write("Nhập ID tài xế cần xóa: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var driver = driverManager.FindDriverById(id);
        if (driver == null)
        {
            Console.WriteLine("Không tìm thấy tài xế với ID này.");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN TÀI XẾ SẮP XÓA ---");
        driver.DisplayDetailed();

        // Check constraints
        bool hasConstraints = false;
        List<string> warnings = new List<string>();

        if (rideManager.HasPendingRidesForDriver(id))
        {
            warnings.Add("⚠ Tài xế đang có chuyến đi chờ xử lý");
            hasConstraints = true;
        }

        if (rideManager.HasRecentRidesForDriver(id, 24))
        {
            warnings.Add("⚠ Tài xế có chuyến đi trong 24h qua");
            hasConstraints = true;
        }

        if (hasConstraints)
        {
            Console.WriteLine("\n--- CẢNH BÁO ---");
            foreach (var warning in warnings)
            {
                Console.WriteLine(warning);
            }
        }

        Console.Write("\nBạn có chắc muốn xóa tài xế này? (Y/N): ");
        string? confirm = Console.ReadLine()?.Trim().ToUpper();

        if (confirm != "Y")
        {
            Console.WriteLine("Đã hủy xóa.");
            return;
        }

        // Store driver data for undo
        var driverCopy = new Driver(driver.Id, driver.Name, driver.Rating, driver.Location.X, driver.Location.Y);
        driverCopy.SetTotalRides(driver.TotalRides);

        if (driverManager.DeleteDriver(id))
        {
            // Push undo action
            undoStack.Push(() =>
            {
                driverManager.AddDriver(driverCopy);
                Console.WriteLine($"Đã khôi phục tài xế {driverCopy.Name}");
            });

            Console.WriteLine($"✓ Đã xóa tài xế ID: {id} ({driver.Name})");
        }
        else
        {
            Console.WriteLine("✗ Lỗi khi xóa tài xế.");
        }
    }

    private void SearchDriverByName()
    {
        Console.Write("Nhập tên tài xế cần tìm: ");
        string? name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Tên không được để trống.");
            return;
        }

        var drivers = driverManager.FindDriversByName(name);

        if (drivers.Count == 0)
        {
            Console.WriteLine($"Không tìm thấy tài xế nào có tên chứa \"{name}\".");
            return;
        }

        Console.WriteLine($"\n--- TÌM THẤY {drivers.Count} TÀI XẾ ---");
        Console.WriteLine("┌─────┬────────┬────────────────────────┬────────┬─────────────────┬───────────┐");
        Console.WriteLine("│ STT │   ID   │          Tên           │ Rating │     Vị trí      │ Số chuyến │");
        Console.WriteLine("├─────┼────────┼────────────────────────┼────────┼─────────────────┼───────────┤");

        int stt = 1;
        foreach (var d in drivers)
        {
            string driverName = d.Name.Length > 22 ? d.Name.Substring(0, 19) + "..." : d.Name;
            Console.WriteLine($"│ {stt,3} │ {d.Id,6} │ {driverName,-22} │ {d.Rating,6:F1} │ ({d.Location.X:F1}, {d.Location.Y:F1}){"",-5} │ {d.TotalRides,9} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────┴────────────────────────┴────────┴─────────────────┴───────────┘");
    }

    private void SearchDriverById()
    {
        Console.Write("Nhập ID tài xế cần tìm: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var driver = driverManager.FindDriverById(id);
        if (driver == null)
        {
            Console.WriteLine($"Không tìm thấy tài xế với ID: {id}");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN TÀI XẾ ---");
        driver.DisplayDetailed();
    }

    private void SortDriversAscending()
    {
        driverManager.SortByRating(ascending: true);
        Console.WriteLine("✓ Đã sắp xếp danh sách theo rating tăng dần.");

        var topDrivers = driverManager.GetTopK(10, fromStart: false);
        if (topDrivers.Count > 0)
        {
            Console.WriteLine($"\n--- {Math.Min(10, topDrivers.Count)} TÀI XẾ ĐẦU TIÊN (RATING THẤP NHẤT) ---");
            foreach (var d in topDrivers)
            {
                d.Display();
            }
        }
    }

    private void SortDriversDescending()
    {
        driverManager.SortByRating(ascending: false);
        Console.WriteLine("✓ Đã sắp xếp danh sách theo rating giảm dần.");

        var topDrivers = driverManager.GetTopK(10, fromStart: true);
        if (topDrivers.Count > 0)
        {
            Console.WriteLine($"\n--- {Math.Min(10, topDrivers.Count)} TÀI XẾ ĐẦU TIÊN (RATING CAO NHẤT) ---");
            foreach (var d in topDrivers)
            {
                d.Display();
            }
        }
    }

    /// <summary>
    /// Customer management sub-menu.
    /// </summary>
    private void ManageCustomers()
    {
        bool back = false;

        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("┌──────────────────────────────────────────────┐");
            Console.WriteLine("│           QUẢN LÝ KHÁCH HÀNG                 │");
            Console.WriteLine("├──────────────────────────────────────────────┤");
            Console.WriteLine("│  1. Hiển thị tất cả khách hàng               │");
            Console.WriteLine("│  2. Hiển thị top K khách hàng (ID cao nhất)  │");
            Console.WriteLine("│  3. Hiển thị top K khách hàng (ID thấp nhất) │");
            Console.WriteLine("│  4. Thêm khách hàng mới                      │");
            Console.WriteLine("│  5. Cập nhật khách hàng theo ID              │");
            Console.WriteLine("│  6. Xóa khách hàng theo ID                   │");
            Console.WriteLine("│  7. Tìm kiếm khách hàng theo tên             │");
            Console.WriteLine("│  8. Tìm kiếm khách hàng theo ID              │");
            Console.WriteLine("│  9. Xem khách hàng theo quận/huyện           │");
            Console.WriteLine("│ 10. Liệt kê tất cả quận/huyện                │");
            Console.WriteLine("│ 11. Quay lại menu chính                      │");
            Console.WriteLine("└──────────────────────────────────────────────┘");
            Console.Write("Chọn chức năng (1-11): ");

            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    DisplayAllCustomers();
                    break;
                case "2":
                    DisplayTopKCustomers(highest: true);
                    break;
                case "3":
                    DisplayTopKCustomers(highest: false);
                    break;
                case "4":
                    AddCustomerWithValidation();
                    break;
                case "5":
                    customerManager.UpdateCustomerInteractive();
                    break;
                case "6":
                    DeleteCustomerWithConfirmation();
                    break;
                case "7":
                    SearchCustomerByName();
                    break;
                case "8":
                    SearchCustomerById();
                    break;
                case "9":
                    ViewCustomersByDistrict();
                    break;
                case "10":
                    ListAllDistricts();
                    break;
                case "11":
                    back = true;
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    private void DisplayAllCustomers()
    {
        Console.WriteLine("\n--- DANH SÁCH TẤT CẢ KHÁCH HÀNG ---");
        var customers = customerManager.GetAll();
        if (customers.Count == 0)
        {
            Console.WriteLine("Chưa có khách hàng nào trong hệ thống.");
            return;
        }
        Console.WriteLine($"Tổng số: {customers.Count} khách hàng\n");

        Console.WriteLine("┌─────┬────────┬────────────────────────┬────────────────┬─────────────────┐");
        Console.WriteLine("│ STT │   ID   │          Tên           │   Quận/Huyện   │     Vị trí      │");
        Console.WriteLine("├─────┼────────┼────────────────────────┼────────────────┼─────────────────┤");

        int stt = 1;
        foreach (var c in customers)
        {
            string name = c.Name.Length > 22 ? c.Name.Substring(0, 19) + "..." : c.Name;
            string district = c.District.Length > 14 ? c.District.Substring(0, 11) + "..." : c.District;
            Console.WriteLine($"│ {stt,3} │ {c.Id,6} │ {name,-22} │ {district,-14} │ ({c.Location.X:F1}, {c.Location.Y:F1}){"",-5} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────┴────────────────────────┴────────────────┴─────────────────┘");
    }

    private void DisplayTopKCustomers(bool highest)
    {
        string label = highest ? "ID CAO NHẤT" : "ID THẤP NHẤT";
        Console.Write($"Nhập số lượng K (Top K {label}): ");

        if (!TryReadInt(out int k) || k <= 0)
        {
            Console.WriteLine("K phải là số nguyên dương.");
            return;
        }

        var topCustomers = customerManager.GetTopK(k, highest);

        if (topCustomers.Count == 0)
        {
            Console.WriteLine("Không có khách hàng nào.");
            return;
        }

        Console.WriteLine($"\n--- TOP {topCustomers.Count} KHÁCH HÀNG {label} ---");
        Console.WriteLine("┌─────┬────────┬────────────────────────┬────────────────┬─────────────────┐");
        Console.WriteLine("│ STT │   ID   │          Tên           │   Quận/Huyện   │     Vị trí      │");
        Console.WriteLine("├─────┼────────┼────────────────────────┼────────────────┼─────────────────┤");

        int stt = 1;
        foreach (var c in topCustomers)
        {
            string name = c.Name.Length > 22 ? c.Name.Substring(0, 19) + "..." : c.Name;
            string district = c.District.Length > 14 ? c.District.Substring(0, 11) + "..." : c.District;
            Console.WriteLine($"│ {stt,3} │ {c.Id,6} │ {name,-22} │ {district,-14} │ ({c.Location.X:F1}, {c.Location.Y:F1}){"",-5} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────┴────────────────────────┴────────────────┴─────────────────┘");
    }

    private void AddCustomerWithValidation()
    {
        Console.WriteLine("\n--- THÊM KHÁCH HÀNG MỚI ---");

        Console.Write("Nhập ID: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        if (customerManager.FindCustomerById(id) != null)
        {
            Console.WriteLine("✗ Lỗi: ID đã tồn tại trong hệ thống.");
            return;
        }

        Console.Write("Nhập tên: ");
        string? name = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("✗ Lỗi: Tên không được để trống.");
            return;
        }

        Console.Write("Nhập quận/huyện: ");
        string? district = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(district))
        {
            Console.WriteLine("✗ Lỗi: Quận/Huyện không được để trống.");
            return;
        }

        Console.Write("Nhập tọa độ X: ");
        if (!TryReadDouble(out double x))
        {
            Console.WriteLine("✗ Lỗi: Tọa độ X không hợp lệ.");
            return;
        }

        Console.Write("Nhập tọa độ Y: ");
        if (!TryReadDouble(out double y))
        {
            Console.WriteLine("✗ Lỗi: Tọa độ Y không hợp lệ.");
            return;
        }

        var customer = new Customer(id, name, district, x, y);

        if (customerManager.AddCustomerWithValidation(customer, out string? errorMessage))
        {
            // Push undo action
            undoStack.Push(() =>
            {
                customerManager.DeleteCustomer(id);
                Console.WriteLine($"Đã hoàn tác thêm khách hàng {name}");
            });

            Console.WriteLine($"\n✓ Thành công! Đã thêm khách hàng:");
            customer.DisplayDetailed();
        }
        else
        {
            Console.WriteLine($"✗ Lỗi: {errorMessage}");
        }
    }

    private void DeleteCustomerWithConfirmation()
    {
        Console.Write("Nhập ID khách hàng cần xóa: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(id);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng với ID này.");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN KHÁCH HÀNG SẮP XÓA ---");
        customer.DisplayDetailed();

        Console.Write("\nBạn có chắc muốn xóa khách hàng này? (Y/N): ");
        string? confirm = Console.ReadLine()?.Trim().ToUpper();

        if (confirm != "Y")
        {
            Console.WriteLine("Đã hủy xóa.");
            return;
        }

        // Store customer data for undo
        var customerCopy = new Customer(customer.Id, customer.Name, customer.District, customer.Location.X, customer.Location.Y);

        if (customerManager.DeleteCustomer(id))
        {
            // Push undo action
            undoStack.Push(() =>
            {
                customerManager.AddCustomer(customerCopy);
                Console.WriteLine($"Đã khôi phục khách hàng {customerCopy.Name}");
            });

            Console.WriteLine($"✓ Đã xóa khách hàng ID: {id} ({customer.Name})");
        }
        else
        {
            Console.WriteLine("✗ Lỗi khi xóa khách hàng.");
        }
    }

    private void SearchCustomerByName()
    {
        Console.Write("Nhập tên khách hàng cần tìm: ");
        string? name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Tên không được để trống.");
            return;
        }

        var customers = customerManager.FindCustomersByName(name);

        if (customers.Count == 0)
        {
            Console.WriteLine($"Không tìm thấy khách hàng nào có tên chứa \"{name}\".");
            return;
        }

        Console.WriteLine($"\n--- TÌM THẤY {customers.Count} KHÁCH HÀNG ---");
        Console.WriteLine("┌─────┬────────┬────────────────────────┬────────────────┬─────────────────┐");
        Console.WriteLine("│ STT │   ID   │          Tên           │   Quận/Huyện   │     Vị trí      │");
        Console.WriteLine("├─────┼────────┼────────────────────────┼────────────────┼─────────────────┤");

        int stt = 1;
        foreach (var c in customers)
        {
            string customerName = c.Name.Length > 22 ? c.Name.Substring(0, 19) + "..." : c.Name;
            string district = c.District.Length > 14 ? c.District.Substring(0, 11) + "..." : c.District;
            Console.WriteLine($"│ {stt,3} │ {c.Id,6} │ {customerName,-22} │ {district,-14} │ ({c.Location.X:F1}, {c.Location.Y:F1}){"",-5} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────┴────────────────────────┴────────────────┴─────────────────┘");
    }

    private void SearchCustomerById()
    {
        Console.Write("Nhập ID khách hàng cần tìm: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(id);
        if (customer == null)
        {
            Console.WriteLine($"Không tìm thấy khách hàng với ID: {id}");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN KHÁCH HÀNG ---");
        customer.DisplayDetailed();
    }

    private void ViewCustomersByDistrict()
    {
        Console.Write("Nhập tên quận/huyện: ");
        string? district = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(district))
        {
            Console.WriteLine("Tên quận/huyện không được để trống.");
            return;
        }

        customerManager.DisplayCustomersByDistrictPaginated(district);
    }

    private void ListAllDistricts()
    {
        var districts = customerManager.GetAllDistricts();

        if (districts.Count == 0)
        {
            Console.WriteLine("Chưa có dữ liệu quận/huyện.");
            return;
        }

        Console.WriteLine("\n--- DANH SÁCH QUẬN/HUYỆN ---");
        Console.WriteLine("┌─────┬────────────────────────┬─────────────────┐");
        Console.WriteLine("│ STT │      Quận/Huyện        │ Số khách hàng   │");
        Console.WriteLine("├─────┼────────────────────────┼─────────────────┤");

        int stt = 1;
        foreach (var district in districts.OrderBy(d => d))
        {
            int count = customerManager.GetDistrictCount(district);
            string districtName = district.Length > 22 ? district.Substring(0, 19) + "..." : district;
            Console.WriteLine($"│ {stt,3} │ {districtName,-22} │ {count,15} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────────────────────┴─────────────────┘");
        Console.WriteLine($"\nTổng: {districts.Count} quận/huyện");
    }

    /// <summary>
    /// Ride management - View driver's ride history sorted by time.
    /// </summary>
    private void ManageRides()
    {
        Console.WriteLine();
        Console.WriteLine("┌────────────────────────────────────────────────┐");
        Console.WriteLine("│    QUẢN LÝ CHUYẾN ĐI - XEM LỊCH SỬ TÀI XẾ     │");
        Console.WriteLine("└────────────────────────────────────────────────┘");
        
        ViewDriverRideHistory();
    }

    /// <summary>
    /// Finds suitable drivers near a customer location.
    /// </summary>
    private void FindSuitableDrivers()
    {
        Console.Write("Nhập ID khách hàng: ");
        if (!TryReadInt(out int customerId))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng.");
            return;
        }

        Console.Write("Nhập bán kính tìm kiếm (km): ");
        if (!TryReadDouble(out double radius))
        {
            Console.WriteLine("Bán kính không hợp lệ.");
            return;
        }

        var nearbyDrivers = driverManager.FindNearbyDrivers(customer.Location, radius);

        if (nearbyDrivers.Count == 0)
        {
            Console.WriteLine("Không tìm thấy tài xế trong bán kính.");
            return;
        }

        Console.WriteLine($"\n--- TÀI XẾ GẦN KHÁCH HÀNG {customer.Name} (bán kính {radius}km) ---");
        foreach (var (distance, driver) in nearbyDrivers)
        {
            Console.WriteLine($"Khoảng cách: {distance:F2}km | ID: {driver.Id} | Tên: {driver.Name} | Rating: {driver.Rating:F1}");
        }
    }

    /// <summary>
    /// Advanced driver search with sorting options and direct booking.
    /// </summary>
    private void FindSuitableDriversAdvanced()
    {
        Console.Write("Nhập ID khách hàng: ");
        if (!TryReadInt(out int customerId))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng.");
            return;
        }

        Console.Write("Nhập bán kính tìm kiếm R (km): ");
        if (!TryReadDouble(out double radius) || radius <= 0)
        {
            Console.WriteLine("Bán kính không hợp lệ.");
            return;
        }

        Console.WriteLine("\nChọn tiêu chí sắp xếp:");
        Console.WriteLine("1. Khoảng cách (mặc định)");
        Console.WriteLine("2. Rating cao nhất");
        Console.WriteLine("3. Số chuyến đi nhiều nhất");
        Console.Write("Lựa chọn (1-3): ");

        string? sortChoice = Console.ReadLine()?.Trim();
        int sortCriteria = sortChoice switch
        {
            "2" => 2,
            "3" => 3,
            _ => 1
        };

        var nearbyDrivers = driverManager.FindNearbyDrivers(customer.Location, radius);

        if (nearbyDrivers.Count == 0)
        {
            Console.WriteLine($"\nKhông tìm thấy tài xế trong bán kính {radius:F1}km.");
            Console.WriteLine("Gợi ý: Hãy thử tăng bán kính tìm kiếm.");
            return;
        }

        // Apply secondary sorting based on user choice
        IEnumerable<(double Distance, Driver Driver)> sortedDrivers = sortCriteria switch
        {
            2 => nearbyDrivers.OrderByDescending(d => d.Driver.Rating).ThenBy(d => d.Distance),
            3 => nearbyDrivers.OrderByDescending(d => d.Driver.TotalRides).ThenBy(d => d.Distance),
            _ => nearbyDrivers // Already sorted by distance
        };

        var driverList = sortedDrivers.ToList();

        // Display results in table format
        Console.WriteLine($"\n┌─────┬────────┬────────────────────────┬────────┬─────────────┬───────────┐");
        Console.WriteLine($"│ STT │   ID   │          Tên           │ Rating │ Khoảng cách │ Số chuyến │");
        Console.WriteLine($"├─────┼────────┼────────────────────────┼────────┼─────────────┼───────────┤");

        int stt = 1;
        foreach (var (distance, driver) in driverList)
        {
            string name = driver.Name.Length > 22 ? driver.Name.Substring(0, 19) + "..." : driver.Name;
            Console.WriteLine($"│ {stt,3} │ {driver.Id,6} │ {name,-22} │ {driver.Rating,6:F1} │ {distance,9:F2}km │ {driver.TotalRides,9} │");
            stt++;
        }

        Console.WriteLine($"└─────┴────────┴────────────────────────┴────────┴─────────────┴───────────┘");
        Console.WriteLine($"Tổng: {driverList.Count} tài xế trong bán kính {radius:F1}km");

        // Ask to select driver for booking
        Console.Write("\nChọn tài xế để đặt xe? (nhập ID hoặc 0 để bỏ qua): ");
        if (!TryReadInt(out int selectedDriverId))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        if (selectedDriverId == 0)
        {
            Console.WriteLine("Đã bỏ qua.");
            return;
        }

        // Check if selected driver is in the nearby list
        var selectedDriver = driverList.FirstOrDefault(d => d.Driver.Id == selectedDriverId);
        if (selectedDriver.Driver == null)
        {
            Console.WriteLine("Tài xế không nằm trong danh sách tìm kiếm.");
            return;
        }

        // Proceed to book ride with preselected driver
        BookRide(selectedDriverId);
    }

    /// <summary>
    /// Books a ride manually.
    /// </summary>
    private void BookRide(int? preselectedDriverId = null)
    {
        Console.Write("Nhập ID khách hàng: ");
        if (!TryReadInt(out int customerId))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng.");
            return;
        }

        Driver? driver;
        int driverId;

        if (preselectedDriverId.HasValue)
        {
            driverId = preselectedDriverId.Value;
            driver = driverManager.FindDriverById(driverId);
            if (driver == null)
            {
                Console.WriteLine("Không tìm thấy tài xế đã chọn.");
                return;
            }
        }
        else
        {
            Console.Write("Nhập ID tài xế: ");
            if (!TryReadInt(out driverId))
            {
                Console.WriteLine("ID không hợp lệ.");
                return;
            }

            driver = driverManager.FindDriverById(driverId);
            if (driver == null)
            {
                Console.WriteLine("Không tìm thấy tài xế.");
                return;
            }
        }

        // Calculate driver-to-customer distance automatically
        double driverToCustomerDistance = driver.DistanceTo(customer.Location);
        Console.WriteLine($"Khoảng cách tài xế đến điểm đón: {driverToCustomerDistance:F2} km");

        Console.Write("Nhập khoảng cách từ điểm đón đến điểm đích (km): ");
        if (!TryReadDouble(out double destinationDistance) || destinationDistance < 0)
        {
            Console.WriteLine("Khoảng cách không hợp lệ.");
            return;
        }

        // Calculate totals
        double totalDistance = driverToCustomerDistance + destinationDistance;
        double fare = totalDistance * 12000;

        // Display summary
        Console.WriteLine();
        Console.WriteLine("┌────────────────────────────────────────┐");
        Console.WriteLine("│           THÔNG TIN ĐẶT XE             │");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine($"│  Khách hàng: {customer.Name,-25} │");
        Console.WriteLine($"│  Tài xế: {driver.Name,-29} │");
        Console.WriteLine($"│  Khoảng cách tài xế → điểm đón: {driverToCustomerDistance,5:F2} km │");
        Console.WriteLine($"│  Khoảng cách điểm đón → đích: {destinationDistance,7:F2} km │");
        Console.WriteLine($"│  Tổng quãng đường: {totalDistance,18:F2} km │");
        Console.WriteLine($"│  Giá cước: {fare,22:N0} VND │");
        Console.WriteLine("└────────────────────────────────────────┘");

        Console.Write("Xác nhận đặt xe? (Y/N): ");
        string? confirm = Console.ReadLine()?.Trim().ToUpper();

        if (confirm == "Y")
        {
            rideManager.CreateRide(customerId, driverId, totalDistance);

            // Increment driver's TotalRides
            driver.IncrementRides();

            // Push undo action
            undoStack.Push(() =>
            {
                rideManager.CancelPendingRides();
                Console.WriteLine("Đã hoàn tác đặt xe.");
            });

            Console.WriteLine("Đặt xe thành công! Chuyến đi đã được thêm vào hàng đợi.");
        }
        else
        {
            Console.WriteLine("Đã hủy đặt xe.");
        }
    }

    /// <summary>
    /// Automatically matches a customer with the best available driver.
    /// </summary>
    private void AutoMatchRide()
    {
        Console.Write("Nhập ID khách hàng: ");
        if (!TryReadInt(out int customerId))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(customerId);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng.");
            return;
        }

        Console.Write("Nhập khoảng cách đến điểm đích (km): ");
        if (!TryReadDouble(out double destinationDistance) || destinationDistance < 0)
        {
            Console.WriteLine("Khoảng cách không hợp lệ.");
            return;
        }

        Console.WriteLine("\nChọn chiến lược tìm kiếm:");
        Console.WriteLine("1. Tài xế gần nhất (mặc định)");
        Console.WriteLine("2. Tài xế rating cao nhất trong bán kính 5km");
        Console.WriteLine("3. Cân bằng (khoảng cách + rating)");
        Console.Write("Lựa chọn (1-3): ");

        string? strategyChoice = Console.ReadLine()?.Trim();
        int strategy = strategyChoice switch
        {
            "2" => 2,
            "3" => 3,
            _ => 1
        };

        // Set initial radius based on strategy
        double radius = strategy switch
        {
            1 => 10.0,
            2 => 5.0,
            3 => 7.0,
            _ => 10.0
        };

        List<(double Distance, Driver Driver)> nearbyDrivers;
        int retryCount = 0;
        const int maxRetries = 3;

        while (true)
        {
            nearbyDrivers = driverManager.FindNearbyDrivers(customer.Location, radius);

            if (nearbyDrivers.Count > 0)
            {
                break;
            }

            Console.WriteLine($"Không tìm thấy tài xế trong bán kính {radius:F1}km.");

            if (retryCount >= maxRetries)
            {
                Console.WriteLine("Đã vượt quá số lần thử. Không thể tìm thấy tài xế.");
                return;
            }

            Console.Write("Mở rộng bán kính tìm kiếm? (Y/N): ");
            string? expandChoice = Console.ReadLine()?.Trim().ToUpper();

            if (expandChoice != "Y")
            {
                Console.WriteLine("Đã hủy tìm kiếm.");
                return;
            }

            radius += 5.0;
            retryCount++;
            Console.WriteLine($"Mở rộng bán kính lên {radius:F1}km...");
        }

        // Select best driver based on strategy
        (double Distance, Driver Driver) bestMatch;

        switch (strategy)
        {
            case 1:
                // Strategy 1: Nearest driver (first in sorted list)
                bestMatch = nearbyDrivers[0];
                break;

            case 2:
                // Strategy 2: Highest rating among nearby drivers
                bestMatch = nearbyDrivers.OrderByDescending(d => d.Driver.Rating).First();
                break;

            case 3:
                // Strategy 3: Balanced score = (maxDist - distance)/maxDist * 0.6 + rating/5.0 * 0.4
                double maxDist = nearbyDrivers.Max(d => d.Distance);
                if (maxDist == 0) maxDist = 1; // Avoid division by zero

                bestMatch = nearbyDrivers
                    .Select(d => new
                    {
                        d.Distance,
                        d.Driver,
                        Score = ((maxDist - d.Distance) / maxDist) * 0.6 + (d.Driver.Rating / 5.0) * 0.4
                    })
                    .OrderByDescending(d => d.Score)
                    .Select(d => (d.Distance, d.Driver))
                    .First();
                break;

            default:
                bestMatch = nearbyDrivers[0];
                break;
        }

        var (driverDistance, bestDriver) = bestMatch;

        // Display matched driver info
        Console.WriteLine();
        Console.WriteLine("┌────────────────────────────────────────┐");
        Console.WriteLine("│        TÀI XẾ ĐƯỢC GHÉP CẶP            │");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine($"│  ID: {bestDriver.Id,-33} │");
        Console.WriteLine($"│  Tên: {bestDriver.Name,-32} │");
        Console.WriteLine($"│  Rating: {bestDriver.Rating:F1,-29} │");
        Console.WriteLine($"│  Khoảng cách: {driverDistance:F2} km{"",-20} │");
        Console.WriteLine($"│  Số chuyến đã hoàn thành: {bestDriver.TotalRides,-12} │");
        Console.WriteLine("└────────────────────────────────────────┘");

        // Calculate totals
        double totalDistance = driverDistance + destinationDistance;
        double fare = totalDistance * 12000;

        Console.WriteLine($"\nTổng quãng đường: {totalDistance:F2} km");
        Console.WriteLine($"Giá cước ước tính: {fare:N0} VND");

        // Create ride automatically (no confirmation in auto mode)
        rideManager.CreateRide(customerId, bestDriver.Id, totalDistance);

        // Increment driver's TotalRides
        bestDriver.IncrementRides();

        // Push undo action
        undoStack.Push(() =>
        {
            rideManager.CancelPendingRides();
            Console.WriteLine("Đã hoàn tác ghép cặp tự động.");
        });

        Console.WriteLine("\nĐã tự động ghép cặp và tạo chuyến đi thành công!");
    }

    /// <summary>
    /// Saves all data to CSV files.
    /// </summary>
    private void SaveData()
    {
        // Ensure Data directory exists
        Directory.CreateDirectory("Data");

        FileHandler.SaveDrivers(DriversFilePath, driverManager.GetAll());
        FileHandler.SaveCustomers(CustomersFilePath, customerManager.GetAll());
        FileHandler.SaveRides(RidesFilePath, rideManager.GetAllRides());

        Console.WriteLine("Đã lưu tất cả dữ liệu thành công!");
    }

    #region Driver Helper Methods

    private void AddDriver()
    {
        Console.Write("Nhập ID: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        if (driverManager.FindDriverById(id) != null)
        {
            Console.WriteLine("ID đã tồn tại.");
            return;
        }

        Console.Write("Nhập tên: ");
        string? name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Tên không được để trống.");
            return;
        }

        Console.Write("Nhập rating (0-5): ");
        if (!TryReadDouble(out double rating) || rating < 0 || rating > 5)
        {
            Console.WriteLine("Rating không hợp lệ (0-5).");
            return;
        }

        Console.Write("Nhập tọa độ X: ");
        if (!TryReadDouble(out double x))
        {
            Console.WriteLine("Tọa độ X không hợp lệ.");
            return;
        }

        Console.Write("Nhập tọa độ Y: ");
        if (!TryReadDouble(out double y))
        {
            Console.WriteLine("Tọa độ Y không hợp lệ.");
            return;
        }

        var driver = new Driver(id, name, rating, x, y);
        driverManager.AddDriver(driver);

        // Push undo action
        undoStack.Push(() =>
        {
            driverManager.DeleteDriver(id);
            Console.WriteLine($"Đã hoàn tác thêm tài xế ID: {id}");
        });
    }

    private void DeleteDriver()
    {
        Console.Write("Nhập ID tài xế cần xóa: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var driver = driverManager.FindDriverById(id);
        if (driver == null)
        {
            Console.WriteLine("Không tìm thấy tài xế.");
            return;
        }

        // Store driver data for undo
        var driverBackup = new Driver(driver.Id, driver.Name, driver.Rating, driver.Location.X, driver.Location.Y);

        if (driverManager.DeleteDriver(id))
        {
            Console.WriteLine($"Đã xóa tài xế ID: {id}");

            // Push undo action
            undoStack.Push(() =>
            {
                driverManager.AddDriver(driverBackup);
                Console.WriteLine($"Đã hoàn tác xóa tài xế ID: {id}");
            });
        }
    }

    private void SearchDriver()
    {
        Console.Write("Tìm theo (1) ID hoặc (2) Tên: ");
        string? searchChoice = Console.ReadLine();

        if (searchChoice == "1")
        {
            Console.Write("Nhập ID: ");
            if (!TryReadInt(out int id))
            {
                Console.WriteLine("ID không hợp lệ.");
                return;
            }

            var driver = driverManager.FindDriverById(id);
            if (driver != null)
            {
                driver.Display();
            }
            else
            {
                Console.WriteLine("Không tìm thấy tài xế.");
            }
        }
        else if (searchChoice == "2")
        {
            Console.Write("Nhập tên: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Tên không được để trống.");
                return;
            }

            var drivers = driverManager.FindDriversByName(name);
            if (drivers.Count > 0)
            {
                Console.WriteLine($"Tìm thấy {drivers.Count} tài xế:");
                foreach (var driver in drivers)
                {
                    driver.Display();
                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy tài xế.");
            }
        }
    }

    private void SortDriversByRating()
    {
        Console.Write("Sắp xếp (1) Giảm dần hoặc (2) Tăng dần: ");
        string? sortChoice = Console.ReadLine();

        bool ascending = sortChoice == "2";
        driverManager.SortByRating(ascending);
        Console.WriteLine("Đã sắp xếp danh sách tài xế.");
        driverManager.DisplayAll();
    }

    private void GetTopKDrivers()
    {
        Console.Write("Nhập số lượng K: ");
        if (!TryReadInt(out int k) || k <= 0)
        {
            Console.WriteLine("K không hợp lệ.");
            return;
        }

        Console.Write("Lấy (1) Rating cao nhất hoặc (2) Rating thấp nhất: ");
        string? topChoice = Console.ReadLine();

        bool highest = topChoice != "2";
        var topDrivers = driverManager.GetTopK(k, highest);

        string label = highest ? "RATING CAO NHẤT" : "RATING THẤP NHẤT";
        Console.WriteLine($"\n--- TOP {topDrivers.Count} TÀI XẾ {label} ---");
        Console.WriteLine("┌─────┬────────┬────────────────────────┬────────┬─────────────────┬───────────┐");
        Console.WriteLine("│ STT │   ID   │          Tên           │ Rating │     Vị trí      │ Số chuyến │");
        Console.WriteLine("├─────┼────────┼────────────────────────┼────────┼─────────────────┼───────────┤");

        int stt = 1;
        foreach (var d in topDrivers)
        {
            string name = d.Name.Length > 22 ? d.Name.Substring(0, 19) + "..." : d.Name;
            Console.WriteLine($"│ {stt,3} │ {d.Id,6} │ {name,-22} │ {d.Rating,6:F1} │ ({d.Location.X:F1}, {d.Location.Y:F1}){"",-5} │ {d.TotalRides,9} │");
            stt++;
        }
        Console.WriteLine("└─────┴────────┴────────────────────────┴────────┴─────────────────┴───────────┘");
    }

    #endregion

    #region Customer Helper Methods

    private void AddCustomer()
    {
        Console.Write("Nhập ID: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        if (customerManager.FindCustomerById(id) != null)
        {
            Console.WriteLine("ID đã tồn tại.");
            return;
        }

        Console.Write("Nhập tên: ");
        string? name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Tên không được để trống.");
            return;
        }

        Console.Write("Nhập quận/huyện: ");
        string? district = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(district))
        {
            Console.WriteLine("Quận/huyện không được để trống.");
            return;
        }

        Console.Write("Nhập tọa độ X: ");
        if (!TryReadDouble(out double x))
        {
            Console.WriteLine("Tọa độ X không hợp lệ.");
            return;
        }

        Console.Write("Nhập tọa độ Y: ");
        if (!TryReadDouble(out double y))
        {
            Console.WriteLine("Tọa độ Y không hợp lệ.");
            return;
        }

        var customer = new Customer(id, name, district, x, y);
        customerManager.AddCustomer(customer);

        // Push undo action
        undoStack.Push(() =>
        {
            customerManager.DeleteCustomer(id);
            Console.WriteLine($"Đã hoàn tác thêm khách hàng ID: {id}");
        });
    }

    private void DeleteCustomer()
    {
        Console.Write("Nhập ID khách hàng cần xóa: ");
        if (!TryReadInt(out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = customerManager.FindCustomerById(id);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng.");
            return;
        }

        // Store customer data for undo
        var customerBackup = new Customer(customer.Id, customer.Name, customer.District, customer.Location.X, customer.Location.Y);

        if (customerManager.DeleteCustomer(id))
        {
            Console.WriteLine($"Đã xóa khách hàng ID: {id}");

            // Push undo action
            undoStack.Push(() =>
            {
                customerManager.AddCustomer(customerBackup);
                Console.WriteLine($"Đã hoàn tác xóa khách hàng ID: {id}");
            });
        }
    }

    private void SearchCustomer()
    {
        Console.Write("Tìm theo (1) ID hoặc (2) Tên: ");
        string? searchChoice = Console.ReadLine();

        if (searchChoice == "1")
        {
            Console.Write("Nhập ID: ");
            if (!TryReadInt(out int id))
            {
                Console.WriteLine("ID không hợp lệ.");
                return;
            }

            var customer = customerManager.FindCustomerById(id);
            if (customer != null)
            {
                customer.Display();
            }
            else
            {
                Console.WriteLine("Không tìm thấy khách hàng.");
            }
        }
        else if (searchChoice == "2")
        {
            Console.Write("Nhập tên: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Tên không được để trống.");
                return;
            }

            var customers = customerManager.FindCustomersByName(name);
            if (customers.Count > 0)
            {
                Console.WriteLine($"Tìm thấy {customers.Count} khách hàng:");
                foreach (var customer in customers)
                {
                    customer.Display();
                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy khách hàng.");
            }
        }
    }

    private void GetCustomersByDistrict()
    {
        Console.Write("Nhập tên quận/huyện: ");
        string? district = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(district))
        {
            Console.WriteLine("Tên quận/huyện không được để trống.");
            return;
        }

        int count = customerManager.GetDistrictCount(district);
        Console.WriteLine($"Số lượng khách hàng trong {district}: {count}");

        var customers = customerManager.GetCustomersByDistrict(district);
        foreach (var customer in customers)
        {
            customer.Display();
        }
    }

    private void GetTopKCustomers()
    {
        Console.Write("Nhập số lượng K: ");
        if (!TryReadInt(out int k) || k <= 0)
        {
            Console.WriteLine("K không hợp lệ.");
            return;
        }

        Console.Write("Lấy (1) ID cao nhất hoặc (2) ID thấp nhất: ");
        string? topChoice = Console.ReadLine();

        bool highest = topChoice != "2";
        var topCustomers = customerManager.GetTopK(k, highest);

        Console.WriteLine($"\n--- TOP {k} KHÁCH HÀNG ---");
        foreach (var customer in topCustomers)
        {
            customer.Display();
        }
    }

    #endregion

    #region Ride Helper Methods

    private void ViewDriverRideHistory()
    {
        Console.Write("Nhập ID tài xế: ");
        if (!TryReadInt(out int driverId))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var driver = driverManager.FindDriverById(driverId);
        if (driver == null)
        {
            Console.WriteLine("Không tìm thấy tài xế với ID này.");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN TÀI XẾ ---");
        driver.DisplayDetailed();

        // Get all rides sorted by timestamp (ascending - oldest first)
        var rides = rideManager.GetRidesByDriver(driverId);

        if (rides.Count == 0)
        {
            Console.WriteLine("\nTài xế này chưa có chuyến đi nào.");
            return;
        }

        // Display rides in table format, sorted by time
        Console.WriteLine($"\n--- DANH SÁCH CHUYẾN ĐI CỦA TÀI XẾ D{driverId} (SẮP XẾP THEO THỜI GIAN) ---");
        Console.WriteLine("┌─────┬─────────┬────────────┬─────────────┬───────────────┬────────────┬─────────────────────┐");
        Console.WriteLine("│ STT │ RideID  │ Khách hàng │ Quãng đường │    Giá cước   │ Trạng thái │     Thời gian       │");
        Console.WriteLine("├─────┼─────────┼────────────┼─────────────┼───────────────┼────────────┼─────────────────────┤");

        int stt = 1;
        foreach (var ride in rides)
        {
            string status = ride.Status.Length > 10 ? ride.Status.Substring(0, 7) + "..." : ride.Status;
            Console.WriteLine($"│ {stt,3} │ {ride.RideId,7} │ C{ride.CustomerId,-9} │ {ride.Distance,9:F1}km │ {ride.Fare,11:N0}đ │ {status,-10} │ {ride.Timestamp:dd/MM/yyyy HH:mm} │");
            stt++;
        }
        Console.WriteLine("└─────┴─────────┴────────────┴─────────────┴───────────────┴────────────┴─────────────────────┘");

        Console.WriteLine($"\nTổng số chuyến đi: {rides.Count}");
    }

    #endregion

    #region Input Validation Helpers

    private static bool TryReadInt(out int value)
    {
        return int.TryParse(Console.ReadLine(), out value);
    }

    private static bool TryReadDouble(out double value)
    {
        return double.TryParse(Console.ReadLine(), out value);
    }

    #endregion
}

