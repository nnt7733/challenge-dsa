using MinRide.Models;

namespace MinRide.Managers;

/// <summary>
/// Manages rides including pending rides queue and ride history.
/// </summary>
public class RideManager
{
    /// <summary>
    /// The history of all confirmed rides stored as a linked list.
    /// </summary>
    private LinkedList<Ride> rideHistory;

    /// <summary>
    /// Maps driver ID to their list of rides.
    /// </summary>
    private Dictionary<int, List<Ride>> driverRides;

    /// <summary>
    /// Queue of pending rides waiting to be confirmed.
    /// </summary>
    private Queue<Ride> pendingRides;

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
        pendingRides = new Queue<Ride>();
        nextRideId = 1;
    }

    /// <summary>
    /// Creates a new ride and adds it to the pending queue.
    /// </summary>
    /// <param name="customerId">The ID of the customer requesting the ride.</param>
    /// <param name="driverId">The ID of the assigned driver.</param>
    /// <param name="distance">The distance of the ride in kilometers.</param>
    public void CreateRide(int customerId, int driverId, double distance)
    {
        Ride ride = new Ride(nextRideId, customerId, driverId, distance);
        nextRideId++;
        pendingRides.Enqueue(ride);

        Console.WriteLine($"Ride created - ID: {ride.RideId} | Customer: C{customerId} | Driver: D{driverId} | Distance: {distance:F1}km | Fare: {ride.Fare:N0} VND");
    }

    /// <summary>
    /// Confirms all pending rides and moves them to ride history.
    /// </summary>
    /// <param name="driverManager">Optional driver manager to update TotalRides.</param>
    public void ConfirmAllRides(DriverManager? driverManager = null)
    {
        int confirmedCount = 0;

        while (pendingRides.Count > 0)
        {
            Ride ride = pendingRides.Dequeue();
            ride.Confirm();

            // Add to ride history
            rideHistory.AddLast(ride);

            // Add to driver rides
            if (!driverRides.ContainsKey(ride.DriverId))
            {
                driverRides[ride.DriverId] = new List<Ride>();
            }
            driverRides[ride.DriverId].Add(ride);

            // Update driver's TotalRides
            if (driverManager != null)
            {
                var driver = driverManager.FindDriverById(ride.DriverId);
                driver?.IncrementRides();
            }

            confirmedCount++;
        }

        if (confirmedCount > 0)
        {
            Console.WriteLine($"✓ Đã xác nhận {confirmedCount} chuyến đi.");
        }
        else
        {
            Console.WriteLine("Không có chuyến đi nào để xác nhận.");
        }
    }

    /// <summary>
    /// Cancels and clears all pending rides.
    /// </summary>
    public void CancelPendingRides()
    {
        int cancelledCount = pendingRides.Count;
        pendingRides.Clear();
        Console.WriteLine($"Cancelled {cancelledCount} pending ride(s).");
    }

    /// <summary>
    /// Gets all rides for a specific driver, sorted by timestamp.
    /// </summary>
    /// <param name="driverId">The ID of the driver.</param>
    /// <returns>A list of rides for the driver, sorted by timestamp ascending.</returns>
    public List<Ride> GetRidesByDriver(int driverId)
    {
        if (!driverRides.TryGetValue(driverId, out var rides))
        {
            return new List<Ride>();
        }

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
            Console.WriteLine("Không có chuyến đi nào đang chờ.");
            return;
        }

        Console.WriteLine($"\n--- DANH SÁCH CHUYẾN ĐI ĐANG CHỜ ({pendingRides.Count} chuyến) ---");
        Console.WriteLine("┌─────┬─────────┬────────────┬────────────┬─────────────┬───────────────┬──────────────────┐");
        Console.WriteLine("│ STT │ RideID  │ Khách hàng │  Tài xế    │ Quãng đường │    Giá cước   │ Còn lại (hủy)    │");
        Console.WriteLine("├─────┼─────────┼────────────┼────────────┼─────────────┼───────────────┼──────────────────┤");

        int stt = 1;
        foreach (var ride in pendingRides)
        {
            int remaining = ride.GetRemainingCancelTime();
            string remainingStr = remaining > 0 ? $"{remaining}s" : "Hết hạn";
            Console.WriteLine($"│ {stt,3} │ {ride.RideId,7} │ C{ride.CustomerId,-9} │ D{ride.DriverId,-9} │ {ride.Distance,9:F1}km │ {ride.Fare,11:N0}đ │ {remainingStr,-16} │");
            stt++;
        }
        Console.WriteLine("└─────┴─────────┴────────────┴────────────┴─────────────┴───────────────┴──────────────────┘");
        Console.WriteLine("\n⚠ Chuyến đi sẽ tự động xác nhận sau 2 phút nếu không bị hủy.");
    }

    /// <summary>
    /// Auto-confirms rides that have been pending for more than 2 minutes.
    /// </summary>
    /// <param name="driverManager">Optional driver manager to update TotalRides.</param>
    /// <returns>Number of rides auto-confirmed.</returns>
    public int AutoConfirmExpiredRides(DriverManager? driverManager = null)
    {
        if (pendingRides.Count == 0) return 0;

        var stillPending = new Queue<Ride>();
        int autoConfirmedCount = 0;

        while (pendingRides.Count > 0)
        {
            var ride = pendingRides.Dequeue();
            
            if (!ride.CanBeCancelled())
            {
                // Expired - auto confirm
                ride.Confirm();
                rideHistory.AddLast(ride);
                
                if (!driverRides.ContainsKey(ride.DriverId))
                {
                    driverRides[ride.DriverId] = new List<Ride>();
                }
                driverRides[ride.DriverId].Add(ride);

                // Update driver's TotalRides
                if (driverManager != null)
                {
                    var driver = driverManager.FindDriverById(ride.DriverId);
                    driver?.IncrementRides();
                }

                autoConfirmedCount++;
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

        if (autoConfirmedCount > 0)
        {
            Console.WriteLine($"✓ Tự động xác nhận {autoConfirmedCount} chuyến đi đã hết thời gian hủy.");
        }

        return autoConfirmedCount;
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
            Console.WriteLine($"✗ Không tìm thấy chuyến đi ID {rideId} trong danh sách chờ.");
            return false;
        }

        if (!rideToCancel.CanBeCancelled())
        {
            Console.WriteLine($"✗ Chuyến đi ID {rideId} đã quá 2 phút, không thể hủy.");
            return false;
        }

        // Remove from list and rebuild queue
        ridesList.Remove(rideToCancel);
        pendingRides.Clear();
        foreach (var ride in ridesList)
        {
            pendingRides.Enqueue(ride);
        }

        Console.WriteLine($"✓ Đã hủy chuyến đi ID: {rideId}");
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
        rideHistory.AddLast(ride);

        // Add to driver rides dictionary
        if (!driverRides.ContainsKey(ride.DriverId))
        {
            driverRides[ride.DriverId] = new List<Ride>();
        }
        driverRides[ride.DriverId].Add(ride);

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
    /// Checks if a driver has confirmed rides within the specified hours.
    /// </summary>
    /// <param name="driverId">The driver ID to check.</param>
    /// <param name="hours">Number of hours to look back. Default is 24.</param>
    /// <returns><c>true</c> if driver has recent confirmed rides; otherwise, <c>false</c>.</returns>
    public bool HasRecentRidesForDriver(int driverId, int hours = 24)
    {
        if (!driverRides.TryGetValue(driverId, out var rides))
        {
            return false;
        }

        DateTime cutoff = DateTime.Now.AddHours(-hours);
        return rides.Any(r => r.Status == "CONFIRMED" && r.Timestamp >= cutoff);
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
    /// Gets the total revenue for a driver (only confirmed rides).
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <returns>Total fare from confirmed rides.</returns>
    public double GetDriverRevenue(int driverId)
    {
        if (!driverRides.TryGetValue(driverId, out var rides))
        {
            return 0;
        }

        return rides.Where(r => r.Status == "CONFIRMED").Sum(r => r.Fare);
    }

    /// <summary>
    /// Gets statistics for a driver's rides.
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <returns>Tuple of (total rides, confirmed count, total revenue, average distance).</returns>
    public (int TotalRides, int ConfirmedCount, double TotalRevenue, double AverageDistance) GetDriverStats(int driverId)
    {
        if (!driverRides.TryGetValue(driverId, out var rides) || rides.Count == 0)
        {
            return (0, 0, 0, 0);
        }

        var confirmedRides = rides.Where(r => r.Status == "CONFIRMED").ToList();
        int totalRides = rides.Count;
        int confirmedCount = confirmedRides.Count;
        double totalRevenue = confirmedRides.Sum(r => r.Fare);
        double avgDistance = confirmedRides.Count > 0 ? confirmedRides.Average(r => r.Distance) : 0;

        return (totalRides, confirmedCount, totalRevenue, avgDistance);
    }
}

