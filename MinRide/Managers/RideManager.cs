using MinRide.Models;
using MinRide.Utils;

namespace MinRide.Managers;

/// <summary>
/// Manages rides including pending rides queue, in-progress rides, and ride history.
/// Ride flow: PENDING → IN_PROGRESS → COMPLETED
/// </summary>
public class RideManager
{
    /// <summary>
    /// The history of all completed rides stored as a linked list.
    /// </summary>
    private LinkedList<Ride> rideHistory;

    /// <summary>
    /// Maps driver ID to their list of completed rides.
    /// </summary>
    private Dictionary<int, List<Ride>> driverRides;

    /// <summary>
    /// Index for driver rides optimization: Maps DriverID to list of LinkedListNode references.
    /// Allows O(1) access to driver's rides instead of O(N) LinkedList traversal.
    /// </summary>
    private Dictionary<int, List<LinkedListNode<Ride>>> driverRideIndex;

    /// <summary>
    /// Queue of pending rides waiting to start (can be cancelled within 2 min).
    /// </summary>
    private Queue<Ride> pendingRides;

    /// <summary>
    /// List of in-progress rides (driver is traveling).
    /// </summary>
    private List<Ride> inProgressRides;

    /// <summary>
    /// The next ride ID to be assigned.
    /// </summary>
    private int nextRideId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RideManager"/> class.
    /// </summary>
    public RideManager()
    {
        rideHistory = new LinkedList<Ride>();
        driverRides = new Dictionary<int, List<Ride>>();
        driverRideIndex = new Dictionary<int, List<LinkedListNode<Ride>>>();
        pendingRides = new Queue<Ride>();
        inProgressRides = new List<Ride>();
        nextRideId = 1;
    }

    /// <summary>
    /// Creates a new ride and adds it to the pending queue (Admin mode - 2 min wait).
    /// </summary>
    /// <param name="customerId">The ID of the customer requesting the ride.</param>
    /// <param name="driverId">The ID of the assigned driver.</param>
    /// <param name="distance">The distance of the ride in kilometers.</param>
    public void CreateRide(int customerId, int driverId, double distance)
    {
        Ride ride = new Ride(nextRideId, customerId, driverId, distance);
        nextRideId++;
        pendingRides.Enqueue(ride);

        Console.WriteLine($"Đã tạo chuyến đi - Mã: {ride.RideId} | Khách: C{customerId} | Tài xế: D{driverId} | Quãng đường: {distance:F1}km | Giá cước: {ride.Fare:N0} VND");
    }

    /// <summary>
    /// Creates a new ride and starts it immediately (Customer mode - 1 min cancel window).
    /// </summary>
    /// <param name="customerId">The ID of the customer requesting the ride.</param>
    /// <param name="driverId">The ID of the assigned driver.</param>
    /// <param name="distance">The distance of the ride in kilometers.</param>
    /// <returns>The created ride.</returns>
    public Ride CreateRideAndStart(int customerId, int driverId, double distance)
    {
        Ride ride = new Ride(nextRideId, customerId, driverId, distance);
        nextRideId++;
        
        // Start immediately
        ride.Start();
        inProgressRides.Add(ride);

        Console.WriteLine($"Chuyến đi #{ride.RideId} đã khởi hành!");
        Console.WriteLine($"  Tài xế D{driverId} đang đến đón khách C{customerId}");
        Console.WriteLine($"  Quãng đường: {distance:F1}km | Giá cước: {ride.Fare:N0} VND");
        Console.WriteLine($"  Thời gian di chuyển: {ride.GetTotalTravelTime()} giây");

        return ride;
    }

    /// <summary>
    /// Cancels an in-progress ride within the first minute (Customer mode).
    /// </summary>
    /// <param name="rideId">The ride ID to cancel.</param>
    /// <returns>True if cancelled successfully.</returns>
    public bool CancelInProgressRide(int rideId)
    {
        var ride = inProgressRides.FirstOrDefault(r => r.RideId == rideId);
        if (ride == null)
        {
            Console.WriteLine($"[X] Không tìm thấy chuyến đi ID {rideId}.");
            return false;
        }

        // Check if within 1 minute
        if (ride.StartTime.HasValue)
        {
            TimeSpan elapsed = DateTime.Now - ride.StartTime.Value;
            if (elapsed.TotalSeconds > 60)
            {
                Console.WriteLine($"[X] Chuyến đi đã quá 1 phút, không thể hủy.");
                return false;
            }
        }

        ride.Cancel();
        inProgressRides.Remove(ride);
        Console.WriteLine($"[OK] Đã hủy chuyến đi ID: {rideId}");
        return true;
    }

    /// <summary>
    /// Gets the remaining cancel time for an in-progress ride (1 minute window).
    /// </summary>
    public int GetRemainingCancelTimeForInProgress(int rideId)
    {
        var ride = inProgressRides.FirstOrDefault(r => r.RideId == rideId);
        if (ride == null || !ride.StartTime.HasValue) return 0;

        TimeSpan elapsed = DateTime.Now - ride.StartTime.Value;
        double remaining = 60 - elapsed.TotalSeconds; // 1 minute = 60 seconds
        return remaining > 0 ? (int)remaining : 0;
    }

    /// <summary>
    /// Checks if an in-progress ride can be cancelled (within 1 minute).
    /// </summary>
    public bool CanCancelInProgressRide(int rideId)
    {
        return GetRemainingCancelTimeForInProgress(rideId) > 0;
    }

    /// <summary>
    /// Starts all pending rides - moves them to IN_PROGRESS.
    /// </summary>
    public void StartAllPendingRides()
    {
        int startedCount = 0;

        while (pendingRides.Count > 0)
        {
            Ride ride = pendingRides.Dequeue();
            ride.Start();
            inProgressRides.Add(ride);
            startedCount++;

            Console.WriteLine($"  → Chuyến {ride.RideId}: Bắt đầu di chuyển, thời gian: {ride.GetTotalTravelTime()} giây ({ride.Distance:F1}km)");
        }

        if (startedCount > 0)
        {
            Console.WriteLine($"\n[OK] Đã bắt đầu {startedCount} chuyến đi.");
        }
        else
        {
            Console.WriteLine("Không có chuyến đi nào để bắt đầu.");
        }
    }

    /// <summary>
    /// Completes rides that have finished traveling and updates driver stats.
    /// </summary>
    /// <param name="driverManager">Driver manager to update TotalRides.</param>
    /// <returns>Number of rides completed.</returns>
    public int CompleteFinishedRides(DriverManager? driverManager = null)
    {
        var completedRides = inProgressRides.Where(r => r.HasFinishedTraveling()).ToList();
        
        foreach (var ride in completedRides)
        {
            ride.Complete();
            inProgressRides.Remove(ride);
            
            // Add to ride history
            var node = rideHistory.AddLast(ride);

            // Add to driver rides dictionary
            if (!driverRides.ContainsKey(ride.DriverId))
            {
                driverRides[ride.DriverId] = new List<Ride>();
            }
            driverRides[ride.DriverId].Add(ride);

            // Add node reference to driver ride index for O(1) access
            if (!driverRideIndex.ContainsKey(ride.DriverId))
            {
                driverRideIndex[ride.DriverId] = new List<LinkedListNode<Ride>>();
            }
            driverRideIndex[ride.DriverId].Add(node);

            // Update driver's TotalRides
            if (driverManager != null)
            {
                var driver = driverManager.FindDriverById(ride.DriverId);
                driver?.IncrementRides();
            }

            Console.WriteLine($"[OK] Chuyến {ride.RideId} đã hoàn thành! (Tài xế D{ride.DriverId}, Giá: {ride.Fare:N0}đ)");
        }

        return completedRides.Count;
    }

    /// <summary>
    /// Gets list of in-progress rides.
    /// </summary>
    public List<Ride> GetInProgressRides()
    {
        return inProgressRides;
    }

    /// <summary>
    /// Displays in-progress rides with remaining time.
    /// </summary>
    public void DisplayInProgressRides()
    {
        if (inProgressRides.Count == 0)
        {
            Console.WriteLine("\nKhông có chuyến đi nào đang di chuyển.");
            return;
        }

        // Define column widths
        const int colSTT = 5, colRideID = 8, colCustomer = 12, colDriver = 10, colDistance = 11, colFare = 13, colRemain = 10;
        int[] widths = { colSTT, colRideID, colCustomer, colDriver, colDistance, colFare, colRemain };
        string separator = TableHelper.DrawSeparator(widths);
        int totalWidth = widths.Sum() + (widths.Length * 3) - 1;
        string title = $"CHUYẾN ĐI ĐANG DI CHUYỂN ({inProgressRides.Count} chuyến)";

        Console.WriteLine();
        Console.WriteLine(separator);
        Console.WriteLine($"|{title.PadLeft((totalWidth + title.Length) / 2).PadRight(totalWidth)}|");
        Console.WriteLine(separator);
        Console.WriteLine($"| {"STT",colSTT} | {"Mã",colRideID} | {"Khách hàng",-colCustomer} | {"Tài xế",-colDriver} | {"Quãng đường",colDistance} | {"Giá cước",colFare} | {"Còn lại",colRemain} |");
        Console.WriteLine(separator);

        int stt = 1;
        foreach (var ride in inProgressRides)
        {
            int remaining = ride.GetRemainingTravelTime();
            string remainingStr = remaining > 0 ? $"{remaining}s" : "Sắp xong";
            string customer = $"C{ride.CustomerId}";
            string driver = $"D{ride.DriverId}";
            string distance = $"{ride.Distance:F1} km";
            string fare = $"{ride.Fare:N0} đ";
            Console.WriteLine($"| {stt,colSTT} | {ride.RideId,colRideID} | {customer,-colCustomer} | {driver,-colDriver} | {distance,colDistance} | {fare,colFare} | {remainingStr,colRemain} |");
            stt++;
        }
        Console.WriteLine(separator);
    }

    /// <summary>
    /// Cancels and clears all pending rides.
    /// </summary>
    public void CancelPendingRides()
    {
        int cancelledCount = pendingRides.Count;
        pendingRides.Clear();
        Console.WriteLine($"Đã hủy {cancelledCount} chuyến đi đang chờ.");
    }

    /// <summary>
    /// Gets all rides for a specific driver, sorted by timestamp.
    /// Optimized using driver ride index: O(1) index lookup + O(k log k) sorting where k = driver's rides.
    /// Previous implementation was O(N) LinkedList traversal where N = total rides.
    /// </summary>
    /// <param name="driverId">The ID of the driver.</param>
    /// <returns>A list of rides for the driver, sorted by timestamp ascending.</returns>
    public List<Ride> GetRidesByDriver(int driverId)
    {
        // O(1) index lookup instead of O(N) LinkedList traversal
        if (!driverRideIndex.TryGetValue(driverId, out var nodeList))
        {
            return new List<Ride>();
        }

        // Extract rides from nodes and sort by timestamp
        var rides = nodeList.Select(node => node.Value).ToList();
        return rides.OrderBy(r => r.Timestamp).ToList();
    }

    /// <summary>
    /// Displays all pending rides in the queue with detailed info.
    /// </summary>
    public void DisplayPendingRides()
    {
        // Auto-confirm expired rides first
        AutoConfirmExpiredRides();

        if (pendingRides.Count == 0)
        {
            Console.WriteLine("\nKhông có chuyến đi nào đang chờ.");
            return;
        }

        // Define column widths
        const int colSTT = 5, colRideID = 8, colCustomer = 12, colDriver = 10, colDistance = 11, colFare = 13, colRemain = 10;
        int[] widths = { colSTT, colRideID, colCustomer, colDriver, colDistance, colFare, colRemain };
        string separator = TableHelper.DrawSeparator(widths);
        int totalWidth = widths.Sum() + (widths.Length * 3) - 1;
        string title = $"CHUYẾN ĐI ĐANG CHỜ ({pendingRides.Count} chuyến)";

        Console.WriteLine();
        Console.WriteLine(separator);
        Console.WriteLine($"|{title.PadLeft((totalWidth + title.Length) / 2).PadRight(totalWidth)}|");
        Console.WriteLine(separator);
        Console.WriteLine($"| {"STT",colSTT} | {"Mã",colRideID} | {"Khách hàng",-colCustomer} | {"Tài xế",-colDriver} | {"Quãng đường",colDistance} | {"Giá cước",colFare} | {"Còn lại",colRemain} |");
        Console.WriteLine(separator);

        int stt = 1;
        foreach (var ride in pendingRides)
        {
            int remaining = ride.GetRemainingCancelTime();
            string remainingStr = remaining > 0 ? $"{remaining}s" : "Hết hạn";
            string customer = $"C{ride.CustomerId}";
            string driver = $"D{ride.DriverId}";
            string distance = $"{ride.Distance:F1} km";
            string fare = $"{ride.Fare:N0} đ";
            Console.WriteLine($"| {stt,colSTT} | {ride.RideId,colRideID} | {customer,-colCustomer} | {driver,-colDriver} | {distance,colDistance} | {fare,colFare} | {remainingStr,colRemain} |");
            stt++;
        }
        Console.WriteLine(separator);
        Console.WriteLine("\n  * Chuyến đi sẽ tự động bắt đầu sau 2 phút nếu không bị hủy.");
    }

    /// <summary>
    /// Auto-starts rides that have been pending for more than 2 minutes.
    /// Also completes any finished in-progress rides.
    /// </summary>
    /// <param name="driverManager">Optional driver manager to update TotalRides.</param>
    /// <returns>Number of rides auto-started.</returns>
    public int ProcessRides(DriverManager? driverManager = null)
    {
        int autoStartedCount = 0;

        // Step 1: Complete any finished rides first
        CompleteFinishedRides(driverManager);

        // Step 2: Auto-start expired pending rides
        if (pendingRides.Count > 0)
        {
            var stillPending = new Queue<Ride>();

            while (pendingRides.Count > 0)
            {
                var ride = pendingRides.Dequeue();
                
                if (!ride.CanBeCancelled())
                {
                    // Expired - auto start the ride
                    ride.Start();
                    inProgressRides.Add(ride);
                    autoStartedCount++;
                    Console.WriteLine($"→ Chuyến {ride.RideId} tự động bắt đầu (hết thời gian hủy). Thời gian di chuyển: {ride.GetTotalTravelTime()}s");
                }
                else
                {
                    stillPending.Enqueue(ride);
                }
            }

            // Restore still pending rides
            while (stillPending.Count > 0)
            {
                pendingRides.Enqueue(stillPending.Dequeue());
            }
        }

        return autoStartedCount;
    }

    /// <summary>
    /// Legacy method - now calls ProcessRides.
    /// </summary>
    public int AutoConfirmExpiredRides(DriverManager? driverManager = null)
    {
        return ProcessRides(driverManager);
    }

    /// <summary>
    /// Cancels a specific pending ride by ID (only if within 2 minutes).
    /// </summary>
    /// <param name="rideId">The ride ID to cancel.</param>
    /// <returns>True if cancelled, false otherwise.</returns>
    public bool CancelRideById(int rideId)
    {
        // Auto-confirm expired rides first
        AutoConfirmExpiredRides();

        var ridesList = pendingRides.ToList();
        var rideToCancel = ridesList.FirstOrDefault(r => r.RideId == rideId);

        if (rideToCancel == null)
        {
            Console.WriteLine($"[X] Khong tim thay chuyen di ID {rideId} trong danh sach cho.");
            return false;
        }

        if (!rideToCancel.CanBeCancelled())
        {
            Console.WriteLine($"[X] Chuyen di ID {rideId} da qua 2 phut, khong the huy.");
            return false;
        }

        // Remove from list and rebuild queue
        ridesList.Remove(rideToCancel);
        pendingRides.Clear();
        foreach (var ride in ridesList)
        {
            pendingRides.Enqueue(ride);
        }

        Console.WriteLine($"[OK] Da huy chuyen di ID: {rideId}");
        return true;
    }

    /// <summary>
    /// Gets all rides from the ride history.
    /// </summary>
    /// <returns>A list of all confirmed rides.</returns>
    public List<Ride> GetAllRides()
    {
        return rideHistory.ToList();
    }

    /// <summary>
    /// Adds a ride from history (loaded from CSV file).
    /// </summary>
    /// <param name="ride">The ride to add.</param>
    public void AddRideFromHistory(Ride ride)
    {
        // Add to ride history
        var node = rideHistory.AddLast(ride);

        // Add to driver rides dictionary
        if (!driverRides.ContainsKey(ride.DriverId))
        {
            driverRides[ride.DriverId] = new List<Ride>();
        }
        driverRides[ride.DriverId].Add(ride);

        // Add node reference to driver ride index for O(1) access
        if (!driverRideIndex.ContainsKey(ride.DriverId))
        {
            driverRideIndex[ride.DriverId] = new List<LinkedListNode<Ride>>();
        }
        driverRideIndex[ride.DriverId].Add(node);

        // Update nextRideId if needed
        if (ride.RideId >= nextRideId)
        {
            nextRideId = ride.RideId + 1;
        }
    }

    /// <summary>
    /// Gets the queue of pending rides.
    /// </summary>
    /// <returns>The pending rides queue.</returns>
    public Queue<Ride> GetPendingRides()
    {
        return pendingRides;
    }

    /// <summary>
    /// Cancels the last pending ride (most recently added).
    /// </summary>
    /// <returns><c>true</c> if a ride was cancelled; otherwise, <c>false</c>.</returns>
    public bool CancelLastPendingRide()
    {
        if (pendingRides.Count == 0)
        {
            return false;
        }

        // Convert to list, remove last, rebuild queue
        var ridesList = pendingRides.ToList();
        var cancelledRide = ridesList[ridesList.Count - 1];
        ridesList.RemoveAt(ridesList.Count - 1);

        pendingRides.Clear();
        foreach (var ride in ridesList)
        {
            pendingRides.Enqueue(ride);
        }

        Console.WriteLine($"Đã hủy chuyến đi ID: {cancelledRide.RideId}");
        return true;
    }

    /// <summary>
    /// Checks if a driver has any pending rides.
    /// </summary>
    /// <param name="driverId">The driver ID to check.</param>
    /// <returns><c>true</c> if driver has pending rides; otherwise, <c>false</c>.</returns>
    public bool HasPendingRidesForDriver(int driverId)
    {
        return pendingRides.Any(r => r.DriverId == driverId);
    }

    /// <summary>
    /// Checks if a driver has completed rides within the specified hours.
    /// </summary>
    /// <param name="driverId">The driver ID to check.</param>
    /// <param name="hours">Number of hours to look back. Default is 24.</param>
    /// <returns><c>true</c> if driver has recent completed rides; otherwise, <c>false</c>.</returns>
    public bool HasRecentRidesForDriver(int driverId, int hours = 24)
    {
        if (!driverRides.TryGetValue(driverId, out var rides))
        {
            return false;
        }

        DateTime cutoff = DateTime.Now.AddHours(-hours);
        return rides.Any(r => r.Status == "COMPLETED" && r.Timestamp >= cutoff);
    }

    /// <summary>
    /// Checks if a driver has any in-progress rides.
    /// </summary>
    /// <param name="driverId">The driver ID to check.</param>
    /// <returns><c>true</c> if driver has in-progress rides; otherwise, <c>false</c>.</returns>
    public bool HasInProgressRidesForDriver(int driverId)
    {
        return inProgressRides.Any(r => r.DriverId == driverId);
    }

    /// <summary>
    /// Gets rides for a driver with optional filters.
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <param name="statusFilter">Optional status filter (PENDING, CONFIRMED, CANCELLED).</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <returns>Filtered list of rides sorted by timestamp descending.</returns>
    public List<Ride> GetRidesByDriverFiltered(int driverId, string? statusFilter = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        if (!driverRides.TryGetValue(driverId, out var rides))
        {
            return new List<Ride>();
        }

        IEnumerable<Ride> filtered = rides;

        // Apply status filter
        if (!string.IsNullOrEmpty(statusFilter))
        {
            filtered = filtered.Where(r => r.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Apply date range filters
        if (fromDate.HasValue)
        {
            filtered = filtered.Where(r => r.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            filtered = filtered.Where(r => r.Timestamp <= toDate.Value);
        }

        // Sort by timestamp descending (newest first)
        return filtered.OrderByDescending(r => r.Timestamp).ToList();
    }

    /// <summary>
    /// Gets the total revenue for a driver (only completed rides).
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <returns>Total fare from completed rides.</returns>
    public double GetDriverRevenue(int driverId)
    {
        if (!driverRides.TryGetValue(driverId, out var rides))
        {
            return 0;
        }

        return rides.Where(r => r.Status == "COMPLETED").Sum(r => r.Fare);
    }

    /// <summary>
    /// Gets statistics for a driver's rides.
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <returns>Tuple of (total rides, completed count, total revenue, average distance).</returns>
    public (int TotalRides, int CompletedCount, double TotalRevenue, double AverageDistance) GetDriverStats(int driverId)
    {
        if (!driverRides.TryGetValue(driverId, out var rides) || rides.Count == 0)
        {
            return (0, 0, 0, 0);
        }

        var completedRides = rides.Where(r => r.Status == "COMPLETED").ToList();
        int totalRides = rides.Count;
        int completedCount = completedRides.Count;
        double totalRevenue = completedRides.Sum(r => r.Fare);
        double avgDistance = completedRides.Count > 0 ? completedRides.Average(r => r.Distance) : 0;

        return (totalRides, completedCount, totalRevenue, avgDistance);
    }

    /// <summary>
    /// Gets a summary of all ride statuses.
    /// </summary>
    public (int Pending, int InProgress, int Completed) GetRideCounts()
    {
        return (pendingRides.Count, inProgressRides.Count, rideHistory.Count);
    }

    /// <summary>
    /// Checks if a customer has an active ride (PENDING or IN_PROGRESS).
    /// </summary>
    /// <param name="customerId">The customer ID to check.</param>
    /// <returns>True if customer has an active ride.</returns>
    public bool HasActiveRide(int customerId)
    {
        return pendingRides.Any(r => r.CustomerId == customerId) ||
               inProgressRides.Any(r => r.CustomerId == customerId);
    }

    /// <summary>
    /// Gets the active ride for a customer (PENDING or IN_PROGRESS).
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>The active ride, or null if none.</returns>
    public Ride? GetActiveRide(int customerId)
    {
        var pending = pendingRides.FirstOrDefault(r => r.CustomerId == customerId);
        if (pending != null) return pending;

        return inProgressRides.FirstOrDefault(r => r.CustomerId == customerId);
    }

    /// <summary>
    /// Gets all rides for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>List of rides for the customer.</returns>
    public List<Ride> GetRidesByCustomer(int customerId)
    {
        var rides = new List<Ride>();

        // Add from pending
        rides.AddRange(pendingRides.Where(r => r.CustomerId == customerId));

        // Add from in-progress
        rides.AddRange(inProgressRides.Where(r => r.CustomerId == customerId));

        // Add from history
        rides.AddRange(rideHistory.Where(r => r.CustomerId == customerId));

        return rides.OrderByDescending(r => r.Timestamp).ToList();
    }

    /// <summary>
    /// Gets completed rides that haven't been rated by the customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>List of unrated completed rides.</returns>
    public List<Ride> GetUnratedCompletedRides(int customerId)
    {
        return rideHistory
            .Where(r => r.CustomerId == customerId &&
                        r.Status == "COMPLETED" &&
                        !r.CustomerRating.HasValue)
            .OrderByDescending(r => r.Timestamp)
            .ToList();
    }

    /// <summary>
    /// Rates a completed ride and updates the driver's rating.
    /// </summary>
    /// <param name="rideId">The ride ID to rate.</param>
    /// <param name="stars">The rating (1-5 stars).</param>
    /// <param name="driverManager">The driver manager to update driver rating.</param>
    /// <returns>True if rating was successful.</returns>
    public bool RateRide(int rideId, int stars, DriverManager driverManager)
    {
        if (stars < 1 || stars > 5) return false;

        var ride = rideHistory.FirstOrDefault(r => r.RideId == rideId);
        if (ride == null || ride.Status != "COMPLETED" || ride.CustomerRating.HasValue)
            return false;

        ride.CustomerRating = stars;

        // Update driver's rating
        var driver = driverManager.FindDriverById(ride.DriverId);
        driver?.AddRating(stars);

        return true;
    }

    /// <summary>
    /// Gets a ride by ID from all sources (pending, in-progress, history).
    /// </summary>
    /// <param name="rideId">The ride ID.</param>
    /// <returns>The ride, or null if not found.</returns>
    public Ride? GetRideById(int rideId)
    {
        var pending = pendingRides.FirstOrDefault(r => r.RideId == rideId);
        if (pending != null) return pending;

        var inProgress = inProgressRides.FirstOrDefault(r => r.RideId == rideId);
        if (inProgress != null) return inProgress;

        return rideHistory.FirstOrDefault(r => r.RideId == rideId);
    }
}

