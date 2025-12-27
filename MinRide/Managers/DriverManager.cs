using MinRide.Models;
using MinRide.Utils;
using MinRide.Algorithms;

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
    /// Trie data structure for efficient prefix-based name searches.
    /// </summary>
    private NameTrie nameTrie;

    /// <summary>
    /// Suffix Tree data structure for efficient substring-based name searches.
    /// </summary>
    private SuffixTree suffixTree;

    /// <summary>
    /// Grid-based spatial index for efficient nearby driver searches.
    /// Key: Grid cell coordinates (int, int), Value: List of drivers in that cell.
    /// </summary>
    private Dictionary<(int, int), List<Driver>> gridIndex;

    /// <summary>
    /// Size of each grid cell in distance units (e.g., 2.0 km).
    /// </summary>
    private const double CellSize = 2.0;

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
        nameTrie = new NameTrie();
        suffixTree = new SuffixTree();
        gridIndex = new Dictionary<(int, int), List<Driver>>();
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
        // Check for duplicate ID to prevent inconsistency
        if (idToIndex.ContainsKey(driver.Id))
        {
            var existingDriver = FindDriverById(driver.Id);
            if (existingDriver != null && !existingDriver.IsDeleted)
            {
                if (!silent)
                {
                    Console.WriteLine($"[X] Lỗi: ID {driver.Id} đã tồn tại. Vui lòng sử dụng AddDriverWithValidation() để kiểm tra.");
                }
                return;
            }
        }
        
        idToIndex[driver.Id] = drivers.Count;
        driver.IsDeleted = false;
        drivers.Add(driver);
        nameTrie.Insert(driver.Name, driver.Id);
        suffixTree.Insert(driver.Name, driver.Id);
        
        // Add to grid index
        var cellKey = GetCellKey(driver.Location.X, driver.Location.Y);
        if (!gridIndex.ContainsKey(cellKey))
        {
            gridIndex[cellKey] = new List<Driver>();
        }
        gridIndex[cellKey].Add(driver);
        
        if (!silent)
        {
            Console.WriteLine($"[OK] Da them tai xe {driver.Name} (ID: {driver.Id})");
        }
    }

    /// <summary>
    /// Deletes a driver from the collection by ID using lazy deletion.
    /// </summary>
    /// <param name="id">The ID of the driver to delete.</param>
    /// <returns><c>true</c> if the driver was marked as deleted; <c>false</c> if not found.</returns>
    public bool DeleteDriver(int id)
    {
        if (!idToIndex.TryGetValue(id, out int index))
        {
            return false;
        }

        Driver driver = drivers[index];
        
        // Use lazy deletion - just mark as deleted
        driver.IsDeleted = true;
        nameTrie.Remove(driver.Name, id);
        suffixTree.Remove(driver.Name, id);
        
        // Remove from grid index
        var cellKey = GetCellKey(driver.Location.X, driver.Location.Y);
        if (gridIndex.TryGetValue(cellKey, out var driverList))
        {
            driverList.Remove(driver);
            if (driverList.Count == 0)
            {
                gridIndex.Remove(cellKey);
            }
        }

        return true;
    }

    /// <summary>
    /// Finds a driver by their ID using O(1) dictionary lookup.
    /// </summary>
    /// <param name="id">The ID of the driver to find.</param>
    /// <returns>The driver if found and not deleted; otherwise, <c>null</c>.</returns>
    public Driver? FindDriverById(int id)
    {
        if (idToIndex.TryGetValue(id, out int index))
        {
            Driver driver = drivers[index];
            // Return null if the driver is marked as deleted
            return driver.IsDeleted ? null : driver;
        }
        return null;
    }

    /// <summary>
    /// Finds all drivers whose name starts with the specified prefix using Trie.
    /// Time complexity: O(L + M) where L is prefix length and M is number of matching drivers.
    /// </summary>
    /// <param name="prefix">The name prefix to search for.</param>
    /// <returns>A list of drivers matching the prefix and not deleted.</returns>
    public List<Driver> FindDriversByNamePrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return new List<Driver>();

        List<int> matchingIds = nameTrie.SearchByPrefix(prefix);
        var result = new List<Driver>();

        foreach (int id in matchingIds)
        {
            if (idToIndex.TryGetValue(id, out int index))
            {
                Driver driver = drivers[index];
                if (!driver.IsDeleted)
                {
                    result.Add(driver);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Finds all drivers whose name contains the specified search string (substring search) using Suffix Tree.
    /// Optimized with O(L + M) complexity where L is substring length and M is number of matches.
    /// </summary>
    /// <param name="name">The name or partial name to search for.</param>
    /// <returns>A list of drivers matching the search criteria and not deleted.</returns>
    public List<Driver> FindDriversByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return new List<Driver>();

        List<int> matchingIds = suffixTree.SearchBySubstring(name);
        var result = new List<Driver>();

        foreach (int id in matchingIds)
        {
            if (idToIndex.TryGetValue(id, out int index))
            {
                Driver driver = drivers[index];
                if (!driver.IsDeleted)
                {
                    result.Add(driver);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the top K drivers sorted by rating using Min-Heap optimization.
    /// Optimized using PriorityQueue for O(n + k*log(k)) complexity instead of O(n*log(n)).
    /// </summary>
    /// <param name="k">The number of drivers to return.</param>
    /// <param name="highest">If <c>true</c>, returns highest rated; otherwise, lowest rated. Default is highest.</param>
    /// <returns>A list of top K drivers.</returns>
    public List<Driver> GetTopK(int k, bool fromStart = true)
    {
        // Validate k parameter
        if (k <= 0)
        {
            return new List<Driver>();
        }

        var activeDrivers = drivers.Where(d => !d.IsDeleted).ToList();
        if (activeDrivers.Count == 0)
        {
            return new List<Driver>();
        }

        // Limit k to actual driver count
        k = Math.Min(k, activeDrivers.Count);

        if (fromStart)
        {
            // Get Top K HIGHEST rated drivers using Min-Heap
            // Keep a min-heap of size K. When new element is larger than min, replace it.
            // This gives us the K highest elements with O(n + k*log(k)) complexity
            var minHeap = new PriorityQueue<Driver, double>();
            
            foreach (var driver in activeDrivers)
            {
                if (minHeap.Count < k)
                {
                    minHeap.Enqueue(driver, driver.Rating);
                }
                else if (driver.Rating > minHeap.Peek().Rating)
                {
                    // Remove min and add new driver with higher rating
                    minHeap.Dequeue();
                    minHeap.Enqueue(driver, driver.Rating);
                }
            }
            
            // Extract from heap and sort by rating descending
            var result = new List<Driver>();
            while (minHeap.Count > 0)
            {
                result.Add(minHeap.Dequeue());
            }
            result.Reverse(); // Reverse to get descending order
            return result;
        }
        else
        {
            // Get Top K LOWEST rated drivers using Max-Heap (inverted priority)
            // Keep a max-heap of size K. When new element is smaller than max, replace it.
            var maxHeap = new PriorityQueue<Driver, double>();
            
            foreach (var driver in activeDrivers)
            {
                if (maxHeap.Count < k)
                {
                    // Use negative rating for max-heap behavior
                    maxHeap.Enqueue(driver, -driver.Rating);
                }
                else if (driver.Rating < -maxHeap.Peek().Rating)
                {
                    // Remove max and add new driver with lower rating
                    maxHeap.Dequeue();
                    maxHeap.Enqueue(driver, -driver.Rating);
                }
            }
            
            // Extract from heap and sort by rating ascending
            var result = new List<Driver>();
            while (maxHeap.Count > 0)
            {
                result.Add(maxHeap.Dequeue());
            }
            result.Reverse(); // Reverse to get ascending order
            return result;
        }
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

        Driver? driverToUpdate;

        if (matchingDrivers.Count == 1)
        {
            driverToUpdate = matchingDrivers[0];
            Console.WriteLine("\nTìm thấy tài xế:");
            driverToUpdate.DisplayDetailed();
        }
        else
        {
            // Multiple drivers with same name
            Console.WriteLine($"\nTim thay {matchingDrivers.Count} tai xe trung ten:");
            Console.WriteLine("+-----+--------+------------------------+--------+-----------------+-----------+");
            Console.WriteLine("| STT |   ID   |          Ten           | Rating |     Vi tri      | So chuyen |");
            Console.WriteLine("+-----+--------+------------------------+--------+-----------------+-----------+");

            int stt = 1;
            foreach (var d in matchingDrivers)
            {
                string driverName = d.Name.Length > 22 ? d.Name.Substring(0, 19) + "..." : d.Name;
                Console.WriteLine($"| {stt,3} | {d.Id,6} | {driverName,-22} | {d.Rating,6:F1} | ({d.Location.X:F1}, {d.Location.Y:F1}){"",-5} | {d.TotalRides,9} |");
                stt++;
            }
            Console.WriteLine("+-----+--------+------------------------+--------+-----------------+-----------+");

            Console.Write("\nNhập ID tài xế cần cập nhật: ");
            if (!int.TryParse(Console.ReadLine(), out int selectedId))
            {
                Console.WriteLine("ID không hợp lệ.");
                return;
            }

            driverToUpdate = matchingDrivers.FirstOrDefault(d => d.Id == selectedId);
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
        if (newName != null)
        {
            nameTrie.Remove(driverToUpdate.Name, driverToUpdate.Id);
            suffixTree.Remove(driverToUpdate.Name, driverToUpdate.Id);
            driverToUpdate.Name = newName;
            nameTrie.Insert(newName, driverToUpdate.Id);
            suffixTree.Insert(newName, driverToUpdate.Id);
        }
        
        if (newRating.HasValue)
            driverToUpdate.SetRating(newRating.Value);
        
        if (newX.HasValue || newY.HasValue)
        {
            double newLocationX = newX ?? oldX;
            double newLocationY = newY ?? oldY;
            
            // Update grid index if location is changing
            if (newLocationX != oldX || newLocationY != oldY)
            {
                var oldCellKey = GetCellKey(oldX, oldY);
                var newCellKey = GetCellKey(newLocationX, newLocationY);
                
                // Remove from old cell
                if (oldCellKey != newCellKey)
                {
                    if (gridIndex.TryGetValue(oldCellKey, out var oldCellDrivers))
                    {
                        oldCellDrivers.Remove(driverToUpdate);
                        if (oldCellDrivers.Count == 0)
                        {
                            gridIndex.Remove(oldCellKey);
                        }
                    }
                    
                    // Add to new cell
                    if (!gridIndex.ContainsKey(newCellKey))
                    {
                        gridIndex[newCellKey] = new List<Driver>();
                    }
                    gridIndex[newCellKey].Add(driverToUpdate);
                }
            }
            
            driverToUpdate.Location = (newLocationX, newLocationY);
        }

        // Push undo action
        undoStack?.Push(() =>
        {
            if (newName != null && driverToUpdate.Name != oldName)
            {
                nameTrie.Remove(driverToUpdate.Name, driverToUpdate.Id);
                suffixTree.Remove(driverToUpdate.Name, driverToUpdate.Id);
                driverToUpdate.Name = oldName;
                nameTrie.Insert(oldName, driverToUpdate.Id);
                suffixTree.Insert(oldName, driverToUpdate.Id);
            }
            driverToUpdate.SetRating(oldRating);
            
            // Restore grid index if location was changed
            if (driverToUpdate.Location.X != oldX || driverToUpdate.Location.Y != oldY)
            {
                var currentCellKey = GetCellKey(driverToUpdate.Location.X, driverToUpdate.Location.Y);
                var oldCellKey = GetCellKey(oldX, oldY);
                
                if (currentCellKey != oldCellKey)
                {
                    if (gridIndex.TryGetValue(currentCellKey, out var currentCellDrivers))
                    {
                        currentCellDrivers.Remove(driverToUpdate);
                        if (currentCellDrivers.Count == 0)
                        {
                            gridIndex.Remove(currentCellKey);
                        }
                    }
                    
                    if (!gridIndex.ContainsKey(oldCellKey))
                    {
                        gridIndex[oldCellKey] = new List<Driver>();
                    }
                    gridIndex[oldCellKey].Add(driverToUpdate);
                }
            }
            
            driverToUpdate.Location = (oldX, oldY);
            Console.WriteLine($"Đã hoàn tác cập nhật tài xế ID {driverToUpdate.Id}");
        });

        Console.WriteLine("\n[OK] Cap nhat thanh cong!");
        Console.WriteLine("--- THÔNG TIN SAU CẬP NHẬT ---");
        driverToUpdate.DisplayDetailed();
    }

    /// <summary>
    /// Gets the total count of drivers (excluding deleted ones).
    /// </summary>
    /// <returns>The number of active drivers in the collection.</returns>
    public int GetCount()
    {
        return drivers.Count(d => !d.IsDeleted);
    }

    /// <summary>
    /// Finds the K nearest drivers to a location using grid-based spatial partitioning and Min-Heap optimization.
    /// More efficient than finding all drivers and then sorting, especially for large datasets.
    /// Time Complexity: O(m log k) where m = candidates found, k = results to return.
    /// </summary>
    /// <param name="location">The center location as (X, Y) coordinates.</param>
    /// <param name="k">The number of nearest drivers to return.</param>
    /// <returns>A list of (Distance, Driver) tuples for the K nearest drivers, sorted by distance.</returns>
    public List<(double Distance, Driver Driver)> FindTopNearestDrivers((double X, double Y) location, int k)
    {
        if (k <= 0)
        {
            return new List<(double Distance, Driver Driver)>();
        }

        // Use Max-Heap to keep track of K nearest drivers
        // Priority is inverted (negative distance) so we can use max-heap as min-heap
        var maxHeap = new PriorityQueue<(double Distance, Driver Driver), double>();
        double maxDistance = double.MaxValue;

        // Search in expanding radius using grid
        // Start with initial step and expand if needed
        for (int currentStep = 0; currentStep <= 10; currentStep++)
        {
            var centerCell = GetCellKey(location.X, location.Y);
            
            // Broad Phase: Collect drivers from grid cells at current step
            var candidateDrivers = new HashSet<Driver>();
            for (int dx = -currentStep; dx <= currentStep; dx++)
            {
                for (int dy = -currentStep; dy <= currentStep; dy++)
                {
                    var cellKey = (centerCell.Item1 + dx, centerCell.Item2 + dy);
                    if (gridIndex.TryGetValue(cellKey, out var driversInCell))
                    {
                        foreach (var driver in driversInCell)
                        {
                            if (!driver.IsDeleted)
                            {
                                candidateDrivers.Add(driver);
                            }
                        }
                    }
                }
            }

            // Narrow Phase: Calculate distances and maintain heap of K smallest
            foreach (var driver in candidateDrivers)
            {
                double distance = driver.DistanceTo(location);
                
                // If heap not full, add this driver
                if (maxHeap.Count < k)
                {
                    maxHeap.Enqueue((distance, driver), -distance); // Negative for min-heap behavior
                    maxDistance = Math.Max(maxDistance, distance);
                }
                // If distance is better (smaller) than worst in heap, replace it
                else if (distance < maxDistance)
                {
                    maxHeap.Dequeue();
                    maxHeap.Enqueue((distance, driver), -distance);
                    
                    // Update maxDistance - peek at the top element (largest distance in heap)
                    if (maxHeap.Count > 0)
                    {
                        maxDistance = maxHeap.Peek().Distance;
                    }
                }
            }

            // If we have K drivers, we can stop searching
            if (maxHeap.Count >= k)
            {
                break;
            }
        }

        // Extract from heap in reverse order to get sorted result
        var result = new List<(double Distance, Driver Driver)>();
        while (maxHeap.Count > 0)
        {
            result.Add(maxHeap.Dequeue());
        }
        result.Reverse(); // Reverse to get ascending distance order
        return result;
    }

    /// <summary>
    /// Finds all drivers within a specified radius of a location using grid-based spatial partitioning.
    /// Uses two-phase approach: Broad Phase (grid cells) and Narrow Phase (exact distance calculation).
    /// Time Complexity: O((2*step+1)^2 * k + m*log(m)) where step = ceil(radius/CellSize), k = avg drivers per cell, m = matches
    /// Much faster than O(n) linear scan for sparse data.
    /// </summary>
    /// <param name="location">The center location as (X, Y) coordinates.</param>
    /// <param name="radius">The maximum distance from the location.</param>
    /// <returns>A list of tuples containing (Distance, Driver), sorted by distance ascending then rating descending.</returns>
    public List<(double Distance, Driver Driver)> FindNearbyDrivers((double X, double Y) location, double radius)
    {
        var result = new List<(double Distance, Driver Driver)>();
        
        // Calculate grid cell and search range
        var centerCell = GetCellKey(location.X, location.Y);
        int step = (int)Math.Ceiling(radius / CellSize);
        
        // Broad Phase: Collect drivers from nearby grid cells
        var candidateDrivers = new HashSet<Driver>();
        for (int dx = -step; dx <= step; dx++)
        {
            for (int dy = -step; dy <= step; dy++)
            {
                var cellKey = (centerCell.Item1 + dx, centerCell.Item2 + dy);
                if (gridIndex.TryGetValue(cellKey, out var driversInCell))
                {
                    foreach (var driver in driversInCell)
                    {
                        candidateDrivers.Add(driver);
                    }
                }
            }
        }
        
        // Narrow Phase: Calculate exact distances and filter using Min-Heap for efficiency
        // Use heap to maintain sorted order incrementally
        var minHeap = new PriorityQueue<(double Distance, Driver Driver), (double, double)>();
        
        foreach (var driver in candidateDrivers)
        {
            if (driver.IsDeleted) continue;
            
            double distance = driver.DistanceTo(location);
            if (distance <= radius)
            {
                // Add to heap with composite priority: distance ascending, rating descending
                minHeap.Enqueue((distance, driver), (distance, -driver.Rating));
            }
        }
        
        // Extract from heap to get sorted result
        while (minHeap.Count > 0)
        {
            result.Add(minHeap.Dequeue());
        }
        
        return result;
    }

    /// <summary>
    /// Finds the top-rated driver within a specified radius using O(m) linear scan.
    /// Optimized for Top 1 selection - no need to sort all candidates.
    /// </summary>
    /// <param name="location">The center location as (X, Y) coordinates.</param>
    /// <param name="radius">The maximum distance from the location.</param>
    /// <returns>A tuple containing (Distance, Driver) for the highest-rated driver, or null if none found.</returns>
    public (double Distance, Driver Driver)? FindTopRatedDriverInRadius((double X, double Y) location, double radius)
    {
        var centerCell = GetCellKey(location.X, location.Y);
        int step = (int)Math.Ceiling(radius / CellSize);

        // Broad Phase: Collect candidates from grid
        var candidateDrivers = new HashSet<Driver>();
        for (int dx = -step; dx <= step; dx++)
        {
            for (int dy = -step; dy <= step; dy++)
            {
                var cellKey = (centerCell.Item1 + dx, centerCell.Item2 + dy);
                if (gridIndex.TryGetValue(cellKey, out var driversInCell))
                {
                    foreach (var driver in driversInCell)
                    {
                        if (!driver.IsDeleted)
                            candidateDrivers.Add(driver);
                    }
                }
            }
        }

        // Narrow Phase: Find max rating in single pass - O(m)
        (double Distance, Driver Driver)? best = null;
        double maxRating = -1;

        foreach (var driver in candidateDrivers)
        {
            double distance = driver.DistanceTo(location);
            if (distance <= radius && driver.Rating > maxRating)
            {
                maxRating = driver.Rating;
                best = (distance, driver);
            }
        }

        return best;
    }

    /// <summary>
    /// Finds the best balanced driver (distance + rating weighted score) within a specified radius.
    /// Uses O(m) linear scan optimized for Top 1 selection.
    /// </summary>
    /// <param name="location">The center location as (X, Y) coordinates.</param>
    /// <param name="radius">The maximum distance from the location.</param>
    /// <returns>A tuple containing (Distance, Driver) for the best balanced driver, or null if none found.</returns>
    public (double Distance, Driver Driver)? FindBestBalancedDriverInRadius((double X, double Y) location, double radius)
    {
        var centerCell = GetCellKey(location.X, location.Y);
        int step = (int)Math.Ceiling(radius / CellSize);

        // Broad Phase: Collect candidates
        var candidateDrivers = new HashSet<Driver>();
        for (int dx = -step; dx <= step; dx++)
        {
            for (int dy = -step; dy <= step; dy++)
            {
                var cellKey = (centerCell.Item1 + dx, centerCell.Item2 + dy);
                if (gridIndex.TryGetValue(cellKey, out var driversInCell))
                {
                    foreach (var driver in driversInCell)
                    {
                        if (!driver.IsDeleted)
                            candidateDrivers.Add(driver);
                    }
                }
            }
        }

        // Step 1: Find max distance (needed for score calculation)
        double maxDist = 0;
        var candidatesWithDistance = new List<(double Distance, Driver Driver)>();
        
        foreach (var driver in candidateDrivers)
        {
            double distance = driver.DistanceTo(location);
            if (distance <= radius)
            {
                candidatesWithDistance.Add((distance, driver));
                if (distance > maxDist)
                    maxDist = distance;
            }
        }

        if (maxDist == 0) maxDist = 1;

        // Step 2: Find max score in single pass - O(m)
        (double Distance, Driver Driver)? best = null;
        double maxScore = -1;

        foreach (var (distance, driver) in candidatesWithDistance)
        {
            double score = ((maxDist - distance) / maxDist) * 0.6 + (driver.Rating / 5.0) * 0.4;
            if (score > maxScore)
            {
                maxScore = score;
                best = (distance, driver);
            }
        }

        return best;
    }

    /// <summary>
    /// Calculates the grid cell key for a given coordinate.
    /// </summary>
    /// <param name="x">The X coordinate (latitude).</param>
    /// <param name="y">The Y coordinate (longitude).</param>
    /// <returns>The grid cell coordinates as (int, int).</returns>
    private (int, int) GetCellKey(double x, double y)
    {
        int cellX = (int)(x / CellSize);
        int cellY = (int)(y / CellSize);
        return (cellX, cellY);
    }

    /// <summary>
    /// Displays all drivers in the collection (excluding deleted ones).
    /// </summary>
    public void DisplayAll()
    {
        foreach (var driver in drivers.Where(d => !d.IsDeleted))
        {
            driver.Display();
        }
    }

    /// <summary>
    /// Gets all drivers in the collection (excluding deleted ones).
    /// </summary>
    /// <returns>The list of all active drivers.</returns>
    public List<Driver> GetAll()
    {
        return drivers.Where(d => !d.IsDeleted).ToList();
    }

    /// <summary>
    /// Gets the next available ID for a new driver.
    /// </summary>
    /// <returns>The next available driver ID.</returns>
    public int GetNextId()
    {
        if (drivers.Count == 0)
            return 1;
        return drivers.Max(d => d.Id) + 1;
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

        // Update Trie and SuffixTree if name is changing
        if (newName != null && newName != oldName)
        {
            nameTrie.Remove(oldName, id);
            suffixTree.Remove(oldName, id);
            driver.Name = newName;
            nameTrie.Insert(newName, id);
            suffixTree.Insert(newName, id);
        }

        if (newRating.HasValue)
        {
            driver.SetRating(newRating.Value);
        }

        if (newX.HasValue || newY.HasValue)
        {
            double x = newX ?? driver.Location.X;
            double y = newY ?? driver.Location.Y;
            
            // Update grid index if location is changing
            if (x != oldX || y != oldY)
            {
                var oldCellKey = GetCellKey(oldX, oldY);
                var newCellKey = GetCellKey(x, y);
                
                // Remove from old cell
                if (oldCellKey != newCellKey)
                {
                    if (gridIndex.TryGetValue(oldCellKey, out var oldCellDrivers))
                    {
                        oldCellDrivers.Remove(driver);
                        if (oldCellDrivers.Count == 0)
                        {
                            gridIndex.Remove(oldCellKey);
                        }
                    }
                    
                    // Add to new cell
                    if (!gridIndex.ContainsKey(newCellKey))
                    {
                        gridIndex[newCellKey] = new List<Driver>();
                    }
                    gridIndex[newCellKey].Add(driver);
                }
            }
            
            driver.Location = (x, y);
        }

        // Push undo action
        undoStack?.Push(() =>
        {
            if (oldName != driver.Name)
            {
                nameTrie.Remove(driver.Name, id);
                suffixTree.Remove(driver.Name, id);
                driver.Name = oldName;
                nameTrie.Insert(oldName, id);
                suffixTree.Insert(oldName, id);
            }
            driver.SetRating(oldRating);
            
            // Restore grid index if location was changed
            double currentX = driver.Location.X;
            double currentY = driver.Location.Y;
            if (currentX != oldX || currentY != oldY)
            {
                var currentCellKey = GetCellKey(currentX, currentY);
                var oldCellKey = GetCellKey(oldX, oldY);
                
                if (currentCellKey != oldCellKey)
                {
                    if (gridIndex.TryGetValue(currentCellKey, out var currentCellDrivers))
                    {
                        currentCellDrivers.Remove(driver);
                        if (currentCellDrivers.Count == 0)
                        {
                            gridIndex.Remove(currentCellKey);
                        }
                    }
                    
                    if (!gridIndex.ContainsKey(oldCellKey))
                    {
                        gridIndex[oldCellKey] = new List<Driver>();
                    }
                    gridIndex[oldCellKey].Add(driver);
                }
            }
            
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

