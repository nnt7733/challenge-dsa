namespace MinRide.Models;

/// <summary>
/// Represents a ride in the MinRide system.
/// Status flow: PENDING → IN_PROGRESS → COMPLETED (or CANCELLED)
/// </summary>
public class Ride
{
    /// <summary>
    /// Speed constant: 1km = 15 seconds of travel time.
    /// </summary>
    public const int SECONDS_PER_KM = 15;

    /// <summary>
    /// Gets or sets the unique identifier for the ride.
    /// </summary>
    public int RideId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who requested the ride.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the driver assigned to the ride.
    /// </summary>
    public int DriverId { get; set; }

    /// <summary>
    /// Gets or sets the distance of the ride in kilometers.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// Gets or sets the fare for the ride in VND.
    /// </summary>
    public double Fare { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the ride was created/booked.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the ride started (IN_PROGRESS).
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the expected completion time based on distance.
    /// </summary>
    public DateTime? ExpectedCompletionTime { get; set; }

    /// <summary>
    /// Gets or sets the current status of the ride (PENDING, IN_PROGRESS, COMPLETED, CANCELLED).
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ride"/> class for a new ride.
    /// </summary>
    /// <param name="rideId">The unique identifier for the ride.</param>
    /// <param name="customerId">The identifier of the customer requesting the ride.</param>
    /// <param name="driverId">The identifier of the assigned driver.</param>
    /// <param name="distance">The distance of the ride in kilometers.</param>
    /// <remarks>
    /// Fare is automatically calculated as distance * 12000 VND.
    /// Status is set to "PENDING" and Timestamp is set to the current time.
    /// </remarks>
    public Ride(int rideId, int customerId, int driverId, double distance)
    {
        RideId = rideId;
        CustomerId = customerId;
        DriverId = driverId;
        Distance = distance;
        Fare = distance * 12000;
        Timestamp = DateTime.Now;
        Status = "PENDING";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ride"/> class from existing data (e.g., from CSV).
    /// </summary>
    /// <param name="rideId">The unique identifier for the ride.</param>
    /// <param name="customerId">The identifier of the customer.</param>
    /// <param name="driverId">The identifier of the driver.</param>
    /// <param name="distance">The distance of the ride in kilometers.</param>
    /// <param name="fare">The fare for the ride in VND.</param>
    /// <param name="timestamp">The timestamp when the ride was created.</param>
    /// <param name="status">The current status of the ride.</param>
    public Ride(int rideId, int customerId, int driverId, double distance, double fare, DateTime timestamp, string status)
    {
        RideId = rideId;
        CustomerId = customerId;
        DriverId = driverId;
        Distance = distance;
        Fare = fare;
        Timestamp = timestamp;
        Status = status;
    }

    /// <summary>
    /// Starts the ride - sets status to IN_PROGRESS and calculates completion time.
    /// </summary>
    public void Start()
    {
        Status = "IN_PROGRESS";
        StartTime = DateTime.Now;
        // Calculate expected completion time: distance * 15 seconds per km
        int travelSeconds = (int)(Distance * SECONDS_PER_KM);
        ExpectedCompletionTime = StartTime.Value.AddSeconds(travelSeconds);
    }

    /// <summary>
    /// Completes the ride by setting the status to "COMPLETED".
    /// </summary>
    public void Complete()
    {
        Status = "COMPLETED";
    }

    /// <summary>
    /// Confirms the ride (legacy - now starts the ride).
    /// </summary>
    public void Confirm()
    {
        Start();
    }

    /// <summary>
    /// Cancels the ride by setting the status to "CANCELLED".
    /// </summary>
    public void Cancel()
    {
        Status = "CANCELLED";
    }

    /// <summary>
    /// Checks if the ride is still pending.
    /// </summary>
    /// <returns><c>true</c> if the ride status is "PENDING"; otherwise, <c>false</c>.</returns>
    public bool IsPending()
    {
        return Status == "PENDING";
    }

    /// <summary>
    /// Checks if the ride is in progress (driver is traveling).
    /// </summary>
    /// <returns><c>true</c> if the ride status is "IN_PROGRESS"; otherwise, <c>false</c>.</returns>
    public bool IsInProgress()
    {
        return Status == "IN_PROGRESS";
    }

    /// <summary>
    /// Checks if the ride has finished traveling (ready to complete).
    /// </summary>
    /// <returns><c>true</c> if ride is in progress and travel time has elapsed; otherwise, <c>false</c>.</returns>
    public bool HasFinishedTraveling()
    {
        if (Status != "IN_PROGRESS" || !ExpectedCompletionTime.HasValue)
            return false;
        return DateTime.Now >= ExpectedCompletionTime.Value;
    }

    /// <summary>
    /// Gets remaining travel time in seconds.
    /// </summary>
    /// <returns>Remaining seconds, or 0 if completed/not started.</returns>
    public int GetRemainingTravelTime()
    {
        if (Status != "IN_PROGRESS" || !ExpectedCompletionTime.HasValue)
            return 0;
        TimeSpan remaining = ExpectedCompletionTime.Value - DateTime.Now;
        return remaining.TotalSeconds > 0 ? (int)remaining.TotalSeconds : 0;
    }

    /// <summary>
    /// Gets total travel time in seconds for this ride.
    /// </summary>
    /// <returns>Total seconds based on distance.</returns>
    public int GetTotalTravelTime()
    {
        return (int)(Distance * SECONDS_PER_KM);
    }

    /// <summary>
    /// Checks if the ride can be cancelled (within 2 minutes of creation and still PENDING).
    /// </summary>
    /// <returns><c>true</c> if the ride can be cancelled; otherwise, <c>false</c>.</returns>
    public bool CanBeCancelled()
    {
        if (Status != "PENDING") return false;
        TimeSpan elapsed = DateTime.Now - Timestamp;
        return elapsed.TotalMinutes < 2;
    }

    /// <summary>
    /// Gets the remaining time in seconds before the ride auto-starts.
    /// </summary>
    /// <returns>Remaining seconds, or 0 if already expired.</returns>
    public int GetRemainingCancelTime()
    {
        TimeSpan elapsed = DateTime.Now - Timestamp;
        double remaining = 120 - elapsed.TotalSeconds; // 2 minutes = 120 seconds
        return remaining > 0 ? (int)remaining : 0;
    }

    /// <summary>
    /// Displays the ride's information to the console.
    /// </summary>
    public void Display()
    {
        string statusInfo = Status;
        if (Status == "IN_PROGRESS")
        {
            int remaining = GetRemainingTravelTime();
            statusInfo = $"IN_PROGRESS ({remaining}s còn lại)";
        }
        Console.WriteLine($"RideID: {RideId} | Customer: C{CustomerId} | Driver: D{DriverId} | Distance: {Distance:F1}km | Fare: {Fare:N0} VND | Status: {statusInfo}");
    }

    /// <summary>
    /// Converts the ride's data to a CSV-formatted string.
    /// Only saves COMPLETED rides to CSV.
    /// </summary>
    /// <returns>A comma-separated string containing the ride's data.</returns>
    public string ToCSV()
    {
        // Save as CONFIRMED for backward compatibility (COMPLETED rides)
        string saveStatus = Status == "COMPLETED" ? "CONFIRMED" : Status;
        return $"{RideId},{CustomerId},{DriverId},{Distance},{Fare},{Timestamp:O},{saveStatus}";
    }

    /// <summary>
    /// Creates a new <see cref="Ride"/> instance from a CSV-formatted string.
    /// </summary>
    /// <param name="csvLine">A comma-separated string containing ride data.</param>
    /// <returns>A new <see cref="Ride"/> instance populated with the parsed data.</returns>
    /// <exception cref="FormatException">Thrown when the CSV line cannot be parsed correctly.</exception>
    public static Ride FromCSV(string csvLine)
    {
        try
        {
            string[] parts = csvLine.Split(',');
            int rideId = int.Parse(parts[0]);
            int customerId = int.Parse(parts[1]);
            int driverId = int.Parse(parts[2]);
            double distance = double.Parse(parts[3]);
            double fare = double.Parse(parts[4]);
            DateTime timestamp = DateTime.Parse(parts[5]);
            string status = parts[6];

            // Convert CONFIRMED to COMPLETED for new logic
            if (status == "CONFIRMED") status = "COMPLETED";

            return new Ride(rideId, customerId, driverId, distance, fare, timestamp, status);
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException || ex is FormatException || ex is OverflowException)
        {
            throw new FormatException($"Failed to parse CSV line: {csvLine}", ex);
        }
    }
}

