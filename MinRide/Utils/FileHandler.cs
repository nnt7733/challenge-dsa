using MinRide.Models;

namespace MinRide.Utils;

/// <summary>
/// Provides static methods for loading and saving data to CSV files.
/// </summary>
public static class FileHandler
{
    /// <summary>
    /// Loads drivers from a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <returns>A list of drivers loaded from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static List<Driver> LoadDrivers(string filePath)
    {
        List<Driver> drivers = new List<Driver>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Skip header line
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    drivers.Add(Driver.FromCSV(lines[i]));
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Lỗi: Không tìm thấy file - {filePath}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi tải danh sách tài xế: {ex.Message}");
            throw;
        }

        return drivers;
    }

    /// <summary>
    /// Saves drivers to a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="drivers">The list of drivers to save.</param>
    public static void SaveDrivers(string filePath, List<Driver> drivers)
    {
        try
        {
            List<string> lines = new List<string>
            {
                "ID,Name,Rating,X,Y,TotalRides,RatingSum,RatingCount"
            };

            foreach (var driver in drivers)
            {
                lines.Add(driver.ToCSV());
            }

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"Đã lưu {drivers.Count} tài xế vào {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu danh sách tài xế: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads customers from a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <returns>A list of customers loaded from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static List<Customer> LoadCustomers(string filePath)
    {
        List<Customer> customers = new List<Customer>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Skip header line
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    customers.Add(Customer.FromCSV(lines[i]));
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Lỗi: Không tìm thấy file - {filePath}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi tải danh sách khách hàng: {ex.Message}");
            throw;
        }

        return customers;
    }

    /// <summary>
    /// Saves customers to a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="customers">The list of customers to save.</param>
    public static void SaveCustomers(string filePath, List<Customer> customers)
    {
        try
        {
            List<string> lines = new List<string>
            {
                "ID,Name,District,X,Y"
            };

            foreach (var customer in customers)
            {
                lines.Add(customer.ToCSV());
            }

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"Đã lưu {customers.Count} khách hàng vào {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu danh sách khách hàng: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads rides from a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <returns>A list of rides loaded from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static List<Ride> LoadRides(string filePath)
    {
        List<Ride> rides = new List<Ride>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            // Skip header line
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    rides.Add(Ride.FromCSV(lines[i]));
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Lỗi: Không tìm thấy file - {filePath}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi tải danh sách chuyến đi: {ex.Message}");
            throw;
        }

        return rides;
    }

    /// <summary>
    /// Saves rides to a CSV file.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="rides">The list of rides to save.</param>
    public static void SaveRides(string filePath, List<Ride> rides)
    {
        try
        {
            List<string> lines = new List<string>
            {
                "RideId,CustomerId,DriverId,Distance,Fare,Timestamp,Status,CustomerRating"
            };

            foreach (var ride in rides)
            {
                lines.Add(ride.ToCSV());
            }

            File.WriteAllLines(filePath, lines);
            Console.WriteLine($"Đã lưu {rides.Count} chuyến đi vào {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu danh sách chuyến đi: {ex.Message}");
            throw;
        }
    }
}

