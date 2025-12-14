using MinRide.Models;
using MinRide.Utils;

namespace MinRide.Managers;

/// <summary>
/// Manages a collection of drivers with efficient lookup and manipulation operations.
/// </summary>
public class DriverManager
{
    /// <summary>
    /// The list of all drivers.
    /// </summary>
    private List<Driver> drivers;

    /// <summary>
    /// Maps driver ID to list index for O(1) lookup.
    /// </summary>
    private Dictionary<int, int> idToIndex;

    /// <summary>
    /// The undo stack for reversible operations.
    /// </summary>
    private UndoStack? undoStack;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriverManager"/> class.
    /// </summary>
    public DriverManager()
    {
        drivers = new List<Driver>();
        idToIndex = new Dictionary<int, int>();
    }

    /// <summary>
    /// Sets the undo stack for the manager.
    /// </summary>
    /// <param name="stack">The undo stack to use.</param>
    public void SetUndoStack(UndoStack stack)
    {
        undoStack = stack;
    }

    /// <summary>
    /// Adds a new driver to the collection.
    /// </summary>
    /// <param name="driver">The driver to add.</param>
    public void AddDriver(Driver driver, bool silent = false)
    {
        idToIndex[driver.Id] = drivers.Count;
        drivers.Add(driver);
        if (!silent)
        {
            Console.WriteLine($"✓ Đã thêm tài xế {driver.Name} (ID: {driver.Id})");
        }
    }

    /// <summary>
    /// Deletes a driver from the collection by ID.
    /// </summary>
    /// <param name="id">The ID of the driver to delete.</param>
    /// <returns><c>true</c> if the driver was deleted; <c>false</c> if not found.</returns>
    public bool DeleteDriver(int id)
    {
        if (!idToIndex.TryGetValue(id, out int index))
        {
            return false;
        }

        // Remove from dictionary
        idToIndex.Remove(id);

        // If not the last element, swap with last element
        int lastIndex = drivers.Count - 1;
        if (index != lastIndex)
        {
            Driver lastDriver = drivers[lastIndex];
            drivers[index] = lastDriver;
            idToIndex[lastDriver.Id] = index;
        }

        // Remove last element
        drivers.RemoveAt(lastIndex);

        return true;
    }

    /// <summary>
    /// Finds a driver by their ID using O(1) dictionary lookup.
    /// </summary>
    /// <param name="id">The ID of the driver to find.</param>
    /// <returns>The driver if found; otherwise, <c>null</c>.</returns>
    public Driver? FindDriverById(int id)
    {
        if (idToIndex.TryGetValue(id, out int index))
        {
            return drivers[index];
        }
        return null;
    }

    /// <summary>
    /// Finds all drivers whose name contains the specified search string.
    /// </summary>
    /// <param name="name">The name or partial name to search for.</param>
    /// <returns>A list of drivers matching the search criteria.</returns>
    public List<Driver> FindDriversByName(string name)
    {
        return drivers.Where(d => d.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Sorts all drivers by their rating.
    /// </summary>
    /// <param name="ascending">If <c>true</c>, sorts in ascending order; otherwise, descending. Default is descending.</param>
    public void SortByRating(bool ascending = false)
    {
        drivers = ascending
            ? drivers.OrderBy(d => d.Rating).ToList()
            : drivers.OrderByDescending(d => d.Rating).ToList();

        // Rebuild idToIndex dictionary
        RebuildIndex();
    }

    /// <summary>
    /// Gets the top K drivers sorted by rating.
    /// </summary>
    /// <param name="k">The number of drivers to return.</param>
    /// <param name="highest">If <c>true</c>, returns highest rated; otherwise, lowest rated. Default is highest.</param>
    /// <returns>A list of top K drivers.</returns>
    public List<Driver> GetTopK(int k, bool fromStart = true)
    {
        // Validate k parameter
        if (k <= 0 || drivers.Count == 0)
        {
            return new List<Driver>();
        }

        // Limit k to actual driver count
        k = Math.Min(k, drivers.Count);

        // fromStart = true: Top K with HIGHEST rating (đầu danh sách = tốt nhất)
        // fromStart = false: Top K with LOWEST rating (cuối danh sách = kém nhất)
        return fromStart
            ? drivers.OrderByDescending(d => d.Rating).Take(k).ToList()
            : drivers.OrderBy(d => d.Rating).Take(k).ToList();
    }

    /// <summary>
    /// Adds a driver with full validation.
    /// </summary>
    /// <param name="driver">The driver to add.</param>
    /// <param name="errorMessage">Output error message if validation fails.</param>
    /// <returns><c>true</c> if driver was added successfully; otherwise, <c>false</c>.</returns>
    public bool AddDriverWithValidation(Driver driver, out string? errorMessage)
    {
        // Check if ID already exists
        if (idToIndex.ContainsKey(driver.Id))
        {
            errorMessage = "ID đã tồn tại";
            return false;
        }

        // Check if Name is empty
        if (string.IsNullOrWhiteSpace(driver.Name))
        {
            errorMessage = "Tên không được để trống";
            return false;
        }

        // Check if Rating is valid (already validated in constructor, but double-check)
        if (driver.Rating < 0 || driver.Rating > 5)
        {
            errorMessage = "Rating phải từ 0.0 đến 5.0";
            return false;
        }

        // Check if Location is valid
        if (double.IsNaN(driver.Location.X) || double.IsInfinity(driver.Location.X) ||
            double.IsNaN(driver.Location.Y) || double.IsInfinity(driver.Location.Y))
        {
            errorMessage = "Tọa độ không hợp lệ";
            return false;
        }

        // All validations passed
        AddDriver(driver);
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Interactive method to update a driver by name with handling for duplicate names.
    /// </summary>
    public void UpdateDriverByName()
    {
        Console.Write("Nhập tên tài xế cần cập nhật: ");
        string? name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Tên không được để trống.");
            return;
        }

        var matchingDrivers = FindDriversByName(name);

        if (matchingDrivers.Count == 0)
        {
            Console.WriteLine("Không tìm thấy tài xế với tên này.");
            return;
        }

        Driver driverToUpdate;

        if (matchingDrivers.Count == 1)
        {
            driverToUpdate = matchingDrivers[0];
            Console.WriteLine("\nTìm thấy tài xế:");
            driverToUpdate.DisplayDetailed();
        }
        else
        {
            // Multiple drivers with same name
            Console.WriteLine($"\nTìm thấy {matchingDrivers.Count} tài xế trùng tên:");
            Console.WriteLine("┌─────┬────────┬────────────────────────┬────────┬─────────────────┬───────────┐");
            Console.WriteLine("│ STT │   ID   │          Tên           │ Rating │     Vị trí      │ Số chuyến │");
            Console.WriteLine("├─────┼────────┼────────────────────────┼────────┼─────────────────┼───────────┤");

            int stt = 1;
            foreach (var d in matchingDrivers)
            {
                string driverName = d.Name.Length > 22 ? d.Name.Substring(0, 19) + "..." : d.Name;
                Console.WriteLine($"│ {stt,3} │ {d.Id,6} │ {driverName,-22} │ {d.Rating,6:F1} │ ({d.Location.X:F1}, {d.Location.Y:F1}){"",-5} │ {d.TotalRides,9} │");
                stt++;
            }
            Console.WriteLine("└─────┴────────┴────────────────────────┴────────┴─────────────────┴───────────┘");

            Console.Write("\nNhập ID tài xế cần cập nhật: ");
            if (!int.TryParse(Console.ReadLine(), out int selectedId))
            {
                Console.WriteLine("ID không hợp lệ.");
                return;
            }

            driverToUpdate = matchingDrivers.FirstOrDefault(d => d.Id == selectedId)!;
            if (driverToUpdate == null)
            {
                Console.WriteLine("ID không nằm trong danh sách tìm kiếm.");
                return;
            }
        }

        // Display current info
        Console.WriteLine("\n--- THÔNG TIN HIỆN TẠI ---");
        driverToUpdate.DisplayDetailed();

        // Ask which field to update
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

        // Store old values for undo
        string oldName = driverToUpdate.Name;
        double oldRating = driverToUpdate.Rating;
        double oldX = driverToUpdate.Location.X;
        double oldY = driverToUpdate.Location.Y;

        string? newName = null;
        double? newRating = null;
        double? newX = null;
        double? newY = null;

        switch (choice)
        {
            case "1":
                Console.Write("Nhập tên mới: ");
                newName = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(newName))
                {
                    Console.WriteLine("Tên không được để trống.");
                    return;
                }
                break;

            case "2":
                Console.Write("Nhập rating mới (0-5): ");
                if (!double.TryParse(Console.ReadLine(), out double rating) || rating < 0 || rating > 5)
                {
                    Console.WriteLine("Rating không hợp lệ (0-5).");
                    return;
                }
                newRating = rating;
                break;

            case "3":
                Console.Write("Nhập tọa độ X mới: ");
                if (!double.TryParse(Console.ReadLine(), out double x))
                {
                    Console.WriteLine("Tọa độ X không hợp lệ.");
                    return;
                }
                Console.Write("Nhập tọa độ Y mới: ");
                if (!double.TryParse(Console.ReadLine(), out double y))
                {
                    Console.WriteLine("Tọa độ Y không hợp lệ.");
                    return;
                }
                newX = x;
                newY = y;
                break;

            case "4":
                Console.Write("Nhập tên mới (Enter để giữ nguyên): ");
                string? inputName = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputName)) newName = inputName;

                Console.Write("Nhập rating mới (Enter để giữ nguyên): ");
                string? inputRating = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputRating))
                {
                    if (double.TryParse(inputRating, out double r) && r >= 0 && r <= 5)
                        newRating = r;
                    else
                    {
                        Console.WriteLine("Rating không hợp lệ.");
                        return;
                    }
                }

                Console.Write("Nhập tọa độ X mới (Enter để giữ nguyên): ");
                string? inputX = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputX))
                {
                    if (double.TryParse(inputX, out double xVal))
                        newX = xVal;
                    else
                    {
                        Console.WriteLine("Tọa độ X không hợp lệ.");
                        return;
                    }
                }

                Console.Write("Nhập tọa độ Y mới (Enter để giữ nguyên): ");
                string? inputY = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputY))
                {
                    if (double.TryParse(inputY, out double yVal))
                        newY = yVal;
                    else
                    {
                        Console.WriteLine("Tọa độ Y không hợp lệ.");
                        return;
                    }
                }
                break;

            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                return;
        }

        // Apply updates
        if (newName != null) driverToUpdate.Name = newName;
        if (newRating.HasValue) driverToUpdate.SetRating(newRating.Value);
        if (newX.HasValue || newY.HasValue)
        {
            driverToUpdate.Location = (newX ?? oldX, newY ?? oldY);
        }

        // Push undo action
        undoStack?.Push(() =>
        {
            driverToUpdate.Name = oldName;
            driverToUpdate.SetRating(oldRating);
            driverToUpdate.Location = (oldX, oldY);
            Console.WriteLine($"Đã hoàn tác cập nhật tài xế ID {driverToUpdate.Id}");
        });

        Console.WriteLine("\n✓ Cập nhật thành công!");
        Console.WriteLine("--- THÔNG TIN SAU CẬP NHẬT ---");
        driverToUpdate.DisplayDetailed();
    }

    /// <summary>
    /// Gets the total count of drivers.
    /// </summary>
    /// <returns>The number of drivers in the collection.</returns>
    public int GetCount()
    {
        return drivers.Count;
    }

    /// <summary>
    /// Finds all drivers within a specified radius of a location, sorted by distance and rating.
    /// </summary>
    /// <param name="location">The center location as (X, Y) coordinates.</param>
    /// <param name="radius">The maximum distance from the location.</param>
    /// <returns>A list of tuples containing (Distance, Driver), sorted by distance ascending then rating descending.</returns>
    public List<(double Distance, Driver Driver)> FindNearbyDrivers((double X, double Y) location, double radius)
    {
        return drivers
            .Select(d => (Distance: d.DistanceTo(location), Driver: d))
            .Where(t => t.Distance <= radius)
            .OrderBy(t => t.Distance)
            .ThenByDescending(t => t.Driver.Rating)
            .ToList();
    }

    /// <summary>
    /// Displays all drivers in the collection.
    /// </summary>
    public void DisplayAll()
    {
        foreach (var driver in drivers)
        {
            driver.Display();
        }
    }

    /// <summary>
    /// Gets all drivers in the collection.
    /// </summary>
    /// <returns>The list of all drivers.</returns>
    public List<Driver> GetAll()
    {
        return drivers;
    }

    /// <summary>
    /// Rebuilds the idToIndex dictionary after list reordering.
    /// </summary>
    private void RebuildIndex()
    {
        idToIndex.Clear();
        for (int i = 0; i < drivers.Count; i++)
        {
            idToIndex[drivers[i].Id] = i;
        }
    }

    /// <summary>
    /// Updates a driver's information with optional parameters.
    /// </summary>
    /// <param name="id">The ID of the driver to update.</param>
    /// <param name="newName">The new name (optional).</param>
    /// <param name="newRating">The new rating (optional, must be 0.0 to 5.0).</param>
    /// <param name="newX">The new X coordinate (optional).</param>
    /// <param name="newY">The new Y coordinate (optional).</param>
    /// <returns><c>true</c> if the driver was updated; <c>false</c> if not found or validation failed.</returns>
    public bool UpdateDriver(int id, string? newName = null, double? newRating = null, double? newX = null, double? newY = null)
    {
        var driver = FindDriverById(id);
        if (driver == null)
        {
            return false;
        }

        // Validate rating if provided
        if (newRating.HasValue && (newRating.Value < 0.0 || newRating.Value > 5.0))
        {
            Console.WriteLine("Error: Rating must be between 0.0 and 5.0.");
            return false;
        }

        // Store old values for undo
        string oldName = driver.Name;
        double oldRating = driver.Rating;
        double oldX = driver.Location.X;
        double oldY = driver.Location.Y;

        // Update only non-null parameters
        if (newName != null)
        {
            driver.Name = newName;
        }

        if (newRating.HasValue)
        {
            driver.SetRating(newRating.Value);
        }

        if (newX.HasValue || newY.HasValue)
        {
            double x = newX ?? driver.Location.X;
            double y = newY ?? driver.Location.Y;
            driver.Location = (x, y);
        }

        // Push undo action
        undoStack?.Push(() =>
        {
            driver.Name = oldName;
            driver.SetRating(oldRating);
            driver.Location = (oldX, oldY);
            Console.WriteLine($"Đã hoàn tác cập nhật tài xế ID: {id}");
        });

        return true;
    }

    /// <summary>
    /// Interactively prompts user to add a new driver.
    /// </summary>
    public void AddDriverInteractive()
    {
        int id;
        while (true)
        {
            Console.Write("Nhập ID tài xế: ");
            if (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Lỗi: ID phải là số nguyên. Vui lòng thử lại.");
                continue;
            }

            if (FindDriverById(id) != null)
            {
                Console.WriteLine("Lỗi: ID đã tồn tại. Vui lòng nhập ID khác.");
                continue;
            }

            break;
        }

        string name;
        while (true)
        {
            Console.Write("Nhập tên tài xế: ");
            name = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Lỗi: Tên không được để trống. Vui lòng thử lại.");
                continue;
            }

            break;
        }

        double rating;
        while (true)
        {
            Console.Write("Nhập rating (0.0 - 5.0): ");
            if (!double.TryParse(Console.ReadLine(), out rating))
            {
                Console.WriteLine("Lỗi: Rating phải là số. Vui lòng thử lại.");
                continue;
            }

            if (rating < 0.0 || rating > 5.0)
            {
                Console.WriteLine("Lỗi: Rating phải từ 0.0 đến 5.0. Vui lòng thử lại.");
                continue;
            }

            break;
        }

        double x;
        while (true)
        {
            Console.Write("Nhập tọa độ X: ");
            if (!double.TryParse(Console.ReadLine(), out x))
            {
                Console.WriteLine("Lỗi: Tọa độ X phải là số. Vui lòng thử lại.");
                continue;
            }

            break;
        }

        double y;
        while (true)
        {
            Console.Write("Nhập tọa độ Y: ");
            if (!double.TryParse(Console.ReadLine(), out y))
            {
                Console.WriteLine("Lỗi: Tọa độ Y phải là số. Vui lòng thử lại.");
                continue;
            }

            break;
        }

        var driver = new Driver(id, name, rating, x, y);
        AddDriver(driver);

        // Push undo action
        undoStack?.Push(() =>
        {
            DeleteDriver(id);
            Console.WriteLine($"Đã hoàn tác thêm tài xế ID: {id}");
        });

        Console.WriteLine($"Thành công! Đã thêm tài xế: ID={id}, Tên={name}, Rating={rating:F1}, Vị trí=({x}, {y})");
    }

    /// <summary>
    /// Interactively prompts user to update a driver's information.
    /// </summary>
    public void UpdateDriverInteractive()
    {
        Console.Write("Nhập ID tài xế cần cập nhật: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Lỗi: ID không hợp lệ.");
            return;
        }

        var driver = FindDriverById(id);
        if (driver == null)
        {
            Console.WriteLine("Lỗi: Không tìm thấy tài xế với ID này.");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN TÀI XẾ HIỆN TẠI ---");
        driver.Display();

        Console.WriteLine("\nChọn thông tin cần cập nhật:");
        Console.WriteLine("1. Tên");
        Console.WriteLine("2. Rating");
        Console.WriteLine("3. Vị trí (X, Y)");
        Console.WriteLine("4. Tất cả");
        Console.Write("Lựa chọn: ");

        string? choice = Console.ReadLine();

        string? newName = null;
        double? newRating = null;
        double? newX = null;
        double? newY = null;

        switch (choice)
        {
            case "1":
                Console.Write("Nhập tên mới: ");
                newName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newName))
                {
                    Console.WriteLine("Lỗi: Tên không được để trống.");
                    return;
                }
                break;

            case "2":
                Console.Write("Nhập rating mới (0.0 - 5.0): ");
                if (!double.TryParse(Console.ReadLine(), out double ratingValue))
                {
                    Console.WriteLine("Lỗi: Rating phải là số.");
                    return;
                }
                newRating = ratingValue;
                break;

            case "3":
                Console.Write("Nhập tọa độ X mới: ");
                if (!double.TryParse(Console.ReadLine(), out double xValue))
                {
                    Console.WriteLine("Lỗi: Tọa độ X phải là số.");
                    return;
                }
                newX = xValue;

                Console.Write("Nhập tọa độ Y mới: ");
                if (!double.TryParse(Console.ReadLine(), out double yValue))
                {
                    Console.WriteLine("Lỗi: Tọa độ Y phải là số.");
                    return;
                }
                newY = yValue;
                break;

            case "4":
                Console.Write("Nhập tên mới (Enter để giữ nguyên): ");
                string? inputName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputName))
                {
                    newName = inputName;
                }

                Console.Write("Nhập rating mới (Enter để giữ nguyên): ");
                string? inputRating = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputRating))
                {
                    if (double.TryParse(inputRating, out double r))
                    {
                        newRating = r;
                    }
                    else
                    {
                        Console.WriteLine("Lỗi: Rating phải là số.");
                        return;
                    }
                }

                Console.Write("Nhập tọa độ X mới (Enter để giữ nguyên): ");
                string? inputX = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputX))
                {
                    if (double.TryParse(inputX, out double xVal))
                    {
                        newX = xVal;
                    }
                    else
                    {
                        Console.WriteLine("Lỗi: Tọa độ X phải là số.");
                        return;
                    }
                }

                Console.Write("Nhập tọa độ Y mới (Enter để giữ nguyên): ");
                string? inputY = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputY))
                {
                    if (double.TryParse(inputY, out double yVal))
                    {
                        newY = yVal;
                    }
                    else
                    {
                        Console.WriteLine("Lỗi: Tọa độ Y phải là số.");
                        return;
                    }
                }
                break;

            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                return;
        }

        if (UpdateDriver(id, newName, newRating, newX, newY))
        {
            Console.WriteLine("Cập nhật thành công!");
            Console.WriteLine("--- THÔNG TIN TÀI XẾ SAU CẬP NHẬT ---");
            driver.Display();
        }
        else
        {
            Console.WriteLine("Cập nhật thất bại.");
        }
    }
}

