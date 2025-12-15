using MinRide.Models;

namespace MinRide.Utils;

/// <summary>
/// Utility class to generate sample data for testing.
/// </summary>
public static class DataGenerator
{
    private static readonly Random random = new Random();

    private static readonly string[] FirstNames = {
        "Nguyen", "Tran", "Le", "Pham", "Hoang", "Vo", "Dang", "Bui", "Ngo", "Truong",
        "Huynh", "Dinh", "Do", "Ly", "Duong", "Vu", "Mai", "Lam", "Ha", "Tang"
    };

    private static readonly string[] MiddleNames = {
        "Van", "Thi", "Duc", "Minh", "Quang", "Thanh", "Manh", "Quoc", "Hong", "Tuan",
        "Mai", "Xuan", "Thu", "Hoai", "Khanh", "Anh", "Bich", "Cam", "Dieu", "Kim"
    };

    private static readonly string[] LastNames = {
        "An", "Binh", "Cuong", "Dung", "Em", "Phong", "Giang", "Hai", "Khoa", "Lam",
        "Minh", "Nam", "Oanh", "Phuc", "Quyen", "Son", "Trang", "Uyen", "Vy", "Xuan",
        "Hoa", "Lan", "Linh", "Nga", "Huong", "Tam", "Tuan", "Hung", "Duc", "Thao"
    };

    private static readonly string[] Districts = {
        "Quan 1", "Quan 3", "Quan 5", "Quan 7", "Quan 10",
        "Quan Binh Thanh", "Quan Go Vap", "Quan Thu Duc", "Quan Phu Nhuan", "Quan Tan Binh",
        "Quan Binh Tan", "Quan 2", "Quan 4", "Quan 6", "Quan 8"
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
    public static List<Driver> GenerateDrivers(int count)
    {
        var drivers = new List<Driver>();
        for (int i = 1; i <= count; i++)
        {
            var location = GenerateLocation();
            var driver = new Driver(i, GenerateName(), GenerateRating(), location.X, location.Y);
            // TotalRides starts at 0, will be synced with actual rides data
            drivers.Add(driver);
        }
        return drivers;
    }

    /// <summary>
    /// Generates a list of random customers.
    /// </summary>
    public static List<Customer> GenerateCustomers(int count)
    {
        var customers = new List<Customer>();
        for (int i = 1; i <= count; i++)
        {
            var location = GenerateLocation();
            var customer = new Customer(i, GenerateName(), GenerateDistrict(), location.X, location.Y);
            customers.Add(customer);
        }
        return customers;
    }

    /// <summary>
    /// Generates a list of random rides.
    /// </summary>
    public static List<Ride> GenerateRides(int count, int maxCustomerId, int maxDriverId)
    {
        var rides = new List<Ride>();
        var statuses = new[] { "CONFIRMED", "CONFIRMED", "CONFIRMED", "CANCELLED" }; // 75% confirmed

        for (int i = 1; i <= count; i++)
        {
            int customerId = random.Next(1, maxCustomerId + 1);
            int driverId = random.Next(1, maxDriverId + 1);
            double distance = Math.Round(1.0 + random.NextDouble() * 10.0, 1);
            double fare = distance * 12000;
            DateTime timestamp = DateTime.Now.AddDays(-random.Next(1, 30)).AddHours(-random.Next(0, 24));
            string status = statuses[random.Next(statuses.Length)];

            var ride = new Ride(i, customerId, driverId, distance, fare, timestamp, status);
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
        Console.WriteLine($"Generating {driverCount} drivers...");
        var drivers = GenerateDrivers(driverCount);
        
        Console.WriteLine($"Generating {customerCount} customers...");
        var customers = GenerateCustomers(customerCount);
        
        Console.WriteLine($"Generating {rideCount} rides...");
        var rides = GenerateRides(rideCount, customerCount, driverCount);

        // Sync driver's TotalRides with actual confirmed rides
        var driverDict = drivers.ToDictionary(d => d.Id);
        foreach (var ride in rides.Where(r => r.Status == "CONFIRMED"))
        {
            if (driverDict.TryGetValue(ride.DriverId, out var driver))
            {
                driver.IncrementRides();
            }
        }

        // Ensure data folder exists
        Directory.CreateDirectory(dataFolder);

        // Save to CSV
        FileHandler.SaveDrivers(Path.Combine(dataFolder, "drivers.csv"), drivers);
        FileHandler.SaveCustomers(Path.Combine(dataFolder, "customers.csv"), customers);
        FileHandler.SaveRides(Path.Combine(dataFolder, "rides.csv"), rides);

        Console.WriteLine("\n✓ Data generation complete!");
        Console.WriteLine($"  - Drivers: {driverCount}");
        Console.WriteLine($"  - Customers: {customerCount}");
        Console.WriteLine($"  - Rides: {rideCount}");
    }

    /// <summary>
    /// Interactive data generation menu.
    /// </summary>
    public static void GenerateDataInteractive()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════╗");
        Console.WriteLine("║        SINH DỮ LIỆU MẪU                  ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");

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

        Console.Write("Nhập số lượng chuyến đi (mặc định 5): ");
        if (!int.TryParse(Console.ReadLine(), out int rideCount) || rideCount <= 0)
        {
            rideCount = 5;
        }

        Console.WriteLine();
        GenerateAndSaveData(driverCount, customerCount, rideCount);
    }
}

