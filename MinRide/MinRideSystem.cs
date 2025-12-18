using MinRide.Auth;
using MinRide.Managers;
using MinRide.Menus;
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
    private AuthManager authManager;
    private MainMenu mainMenu;

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

        LoadData();

        // Initialize auth manager after data is loaded
        authManager = new AuthManager(customerManager, driverManager);
        mainMenu = new MainMenu(authManager, customerManager, driverManager, rideManager);
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
            Console.WriteLine($"[X] Loi tai tai xe: {ex.Message}");
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
            Console.WriteLine($"[X] Loi tai khach hang: {ex.Message}");
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
                    if (ride.Status == "COMPLETED")
                    {
                        driver.IncrementRides();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[X] Loi tai chuyen di: {ex.Message}");
        }

        // Show summary
        Console.WriteLine($"[OK] Da tai: {driverCount} tai xe | {customerCount} khach hang | {rideCount} chuyen di");
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
        mainMenu.Run();

        // Save data before exiting
                    SaveData();
    }

    /// <summary>
    /// Saves all data to CSV files.
    /// </summary>
    private void SaveData()
    {
        try
    {
        // Ensure Data directory exists
        Directory.CreateDirectory("Data");

        FileHandler.SaveDrivers(DriversFilePath, driverManager.GetAll());
        FileHandler.SaveCustomers(CustomersFilePath, customerManager.GetAll());
        FileHandler.SaveRides(RidesFilePath, rideManager.GetAllRides());
            authManager.SavePasswords();

            Console.WriteLine("[OK] Đã lưu dữ liệu tự động.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Lỗi lưu dữ liệu: {ex.Message}");
        }
    }
}
