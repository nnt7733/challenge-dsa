using System;
using System.IO;
using MinRide.Models;

namespace MinRide.Utils;

/// <summary>
/// Utility class to generate sample data for testing.
/// </summary>
public static class DataGenerator
{
    // Deterministic seed so sample data is stable across machines (friends can clone & get same results)
    private static readonly Random random = new Random(20251218);

    public const int DefaultDriverStartId = 1;
    public const int DefaultCustomerStartId = 101;
    public const int DefaultRideStartId = 1001;

    private static readonly string[] FirstNames =
    {
        "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Võ", "Đặng", "Bùi", "Ngô", "Trương",
        "Huỳnh", "Đinh", "Đỗ", "Lý", "Dương", "Vũ", "Mai", "Lâm", "Hà", "Tăng"
    };

    private static readonly string[] MiddleNames =
    {
        "Văn", "Thị", "Đức", "Minh", "Quang", "Thanh", "Mạnh", "Quốc", "Hồng", "Tuấn",
        "Mai", "Xuân", "Thu", "Hoài", "Khánh", "Anh", "Bích", "Cẩm", "Diệu", "Kim"
    };

    private static readonly string[] LastNames =
    {
        "An", "Bình", "Cường", "Dũng", "Em", "Phong", "Giang", "Hải", "Khoa", "Lâm",
        "Minh", "Nam", "Oanh", "Phúc", "Quyên", "Sơn", "Trang", "Uyên", "Vy", "Xuân",
        "Hòa", "Lan", "Linh", "Nga", "Hương", "Tâm", "Tuấn", "Hùng", "Đức", "Thảo"
    };

    private static readonly string[] Districts =
    {
        "Quận 1", "Quận 3", "Quận 5", "Quận 7", "Quận 10",
        "Bình Thạnh", "Gò Vấp", "Thủ Đức", "Phú Nhuận", "Tân Bình",
        "Bình Tân", "Quận 2", "Quận 4", "Quận 6", "Quận 8"
    };

    /// <summary>
    /// Generates a random Vietnamese name.
    /// </summary>
    public static string GenerateName()
    {
        string firstName = FirstNames[random.Next(FirstNames.Length)];
        string middleName = MiddleNames[random.Next(MiddleNames.Length)];
        string lastName = LastNames[random.Next(LastNames.Length)];
        return $"{firstName} {middleName} {lastName}";
    }

    /// <summary>
    /// Generates a random rating between 3.0 and 5.0.
    /// </summary>
    public static double GenerateRating()
    {
        return Math.Round(3.0 + random.NextDouble() * 2.0, 1);
    }

    /// <summary>
    /// Generates a random location within a grid.
    /// </summary>
    public static (double X, double Y) GenerateLocation(double maxX = 10.0, double maxY = 10.0)
    {
        double x = Math.Round(random.NextDouble() * maxX, 1);
        double y = Math.Round(random.NextDouble() * maxY, 1);
        return (x, y);
    }

    /// <summary>
    /// Generates a random district name.
    /// </summary>
    public static string GenerateDistrict()
    {
        return Districts[random.Next(Districts.Length)];
    }

    /// <summary>
    /// Generates a list of random drivers.
    /// TotalRides will be set to 0 and synced later based on actual rides data.
    /// </summary>
    public static List<Driver> GenerateDrivers(int count, int startId = DefaultDriverStartId)
    {
        var drivers = new List<Driver>();
        for (int i = 0; i < count; i++)
        {
            var location = GenerateLocation();
            int id = startId + i;
            // Rating will be calculated from actual rides, use default 5.0 for new drivers
            var driver = new Driver(id, GenerateName(), 5.0, location.X, location.Y);

            // TotalRides and Rating will be synced with actual rides data
            drivers.Add(driver);
        }
        return drivers;
    }

    /// <summary>
    /// Generates a list of random customers.
    /// </summary>
    public static List<Customer> GenerateCustomers(int count, int startId = DefaultCustomerStartId)
    {
        var customers = new List<Customer>();
        for (int i = 0; i < count; i++)
        {
            var location = GenerateLocation();
            int id = startId + i;
            var customer = new Customer(id, GenerateName(), GenerateDistrict(), location.X, location.Y);
            customers.Add(customer);
        }
        return customers;
    }

    /// <summary>
    /// Generates a list of random rides.
    /// </summary>
    public static List<Ride> GenerateRides(
        int count,
        int customerStartId = DefaultCustomerStartId,
        int driverStartId = DefaultDriverStartId,
        int rideStartId = DefaultRideStartId,
        int customerCount = 10,
        int driverCount = 10)
    {
        var rides = new List<Ride>();
        // Make sample data deterministic, consistent, and easy to test:
        // - Each ride is COMPLETED so rating feature works immediately
        // - Cover all customers/drivers (round-robin)
        DateTime baseTime = new DateTime(2024, 12, 15, 8, 30, 0);

        for (int i = 0; i < count; i++)
        {
            int rideId = rideStartId + i;
            int safeCustomerCount = Math.Max(1, customerCount);
            int safeDriverCount = Math.Max(1, driverCount);
            int customerId = customerStartId + (i % safeCustomerCount);
            int driverId = driverStartId + (i % safeDriverCount);

            // 5.0..15.0 km, deterministic-ish
            double distance = Math.Round(5.0 + (random.NextDouble() * 10.0), 1);
            double fare = Math.Round(distance * 12000, 0);
            DateTime timestamp = baseTime.AddMinutes(i * 45);
            string status = "COMPLETED";

            // Alternate 5/4 stars for demo
            int? customerRating = (i % 2 == 0) ? 5 : 4;

            var ride = new Ride(rideId, customerId, driverId, distance, fare, timestamp, status, customerRating);
            rides.Add(ride);
        }
        return rides;
    }

    /// <summary>
    /// Generates and saves sample data to CSV files.
    /// Driver's TotalRides will be synced with actual confirmed rides.
    /// </summary>
    public static void GenerateAndSaveData(int driverCount, int customerCount, int rideCount, string dataFolder = "Data")
    {
        Console.WriteLine($"Đang sinh {driverCount} tài xế...");
        var drivers = GenerateDrivers(driverCount, DefaultDriverStartId);
        
        Console.WriteLine($"Đang sinh {customerCount} khách hàng...");
        var customers = GenerateCustomers(customerCount, DefaultCustomerStartId);
        
        Console.WriteLine($"Đang sinh {rideCount} chuyến đi...");
        var rides = GenerateRides(
            rideCount,
            customerStartId: DefaultCustomerStartId,
            driverStartId: DefaultDriverStartId,
            rideStartId: DefaultRideStartId,
            customerCount: customerCount,
            driverCount: driverCount);

        // Sync driver's TotalRides and Rating with actual confirmed rides
        // Reset rating data first to avoid double-counting
        var driverDict = drivers.ToDictionary(d => d.Id);
        foreach (var driver in drivers)
        {
            // Reset rating data - will be recalculated from actual rides
            // If no ratings from rides, driver keeps default 5.0 rating
            driver.SetRatingData(0, 0);
        }

        // Now sync from actual rides
        foreach (var ride in rides.Where(r => r.Status == "COMPLETED"))
        {
            if (driverDict.TryGetValue(ride.DriverId, out var driver))
            {
                driver.IncrementRides();
                if (ride.CustomerRating.HasValue)
                {
                    driver.AddRating(ride.CustomerRating.Value);
                }
            }
        }

        // Ensure data folder exists
        Directory.CreateDirectory(dataFolder);

        // Save to CSV
        FileHandler.SaveDrivers(Path.Combine(dataFolder, "drivers.csv"), drivers);
        FileHandler.SaveCustomers(Path.Combine(dataFolder, "customers.csv"), customers);
        FileHandler.SaveRides(Path.Combine(dataFolder, "rides.csv"), rides);
        SavePasswords(Path.Combine(dataFolder, "passwords.csv"), customers, drivers);

        Console.WriteLine("\n[OK] Sinh dữ liệu mẫu hoàn tất!");
        Console.WriteLine($"  - Tài xế: {driverCount}");
        Console.WriteLine($"  - Khách hàng: {customerCount}");
        Console.WriteLine($"  - Chuyến đi: {rideCount}");
    }

    /// <summary>
    /// Interactive data generation menu.
    /// </summary>
    public static void GenerateDataInteractive()
    {
        Console.WriteLine();
        Console.WriteLine("+--------------------------------------------+");
        Console.WriteLine("|          SINH DỮ LIỆU MẪU                  |");
        Console.WriteLine("+--------------------------------------------+");

        Console.Write("Nhập số lượng tài xế (mặc định 10): ");
        if (!int.TryParse(Console.ReadLine(), out int driverCount) || driverCount <= 0)
        {
            driverCount = 10;
        }

        Console.Write("Nhập số lượng khách hàng (mặc định 10): ");
        if (!int.TryParse(Console.ReadLine(), out int customerCount) || customerCount <= 0)
        {
            customerCount = 10;
        }

        Console.Write("Nhập số lượng chuyến đi (mặc định 10): ");
        if (!int.TryParse(Console.ReadLine(), out int rideCount) || rideCount <= 0)
        {
            rideCount = 10;
        }

        Console.WriteLine();
        GenerateAndSaveData(driverCount, customerCount, rideCount);
    }

    private static void SavePasswords(string filePath, List<Customer> customers, List<Driver> drivers)
    {
        var lines = new List<string> { "Username,Password" };

        // Customer: login with ID/ID (username input is numeric ID, but stored key is C{ID})
        foreach (var c in customers.OrderBy(c => c.Id))
        {
            lines.Add($"C{c.Id},{c.Id}");
        }

        // Driver: login with ID/ID (stored key is D{ID})
        foreach (var d in drivers.OrderBy(d => d.Id))
        {
            lines.Add($"D{d.Id},{d.Id}");
        }

        File.WriteAllLines(filePath, lines);
    }
}
