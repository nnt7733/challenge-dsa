namespace MinRide.Models;

/// <summary>
/// Represents a driver in the MinRide system.
/// </summary>
public class Driver
{
    /// <summary>
    /// Gets or sets the unique identifier for the driver.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the driver.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the rating of the driver (0.0 to 5.0).
    /// </summary>
    public double Rating { get; private set; }

    /// <summary>
    /// Gets or sets the current location of the driver as (X, Y) coordinates.
    /// </summary>
    public (double X, double Y) Location { get; set; }

    /// <summary>
    /// Gets the total number of rides completed by the driver.
    /// </summary>
    public int TotalRides { get; private set; }

    /// <summary>
    /// Gets the total number of ratings received by the driver.
    /// </summary>
    public int RatingCount { get; private set; }

    /// <summary>
    /// Gets the sum of all ratings (used for calculating average).
    /// </summary>
    public double RatingSum { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the driver is marked as deleted (soft delete).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Driver"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the driver.</param>
    /// <param name="name">The name of the driver.</param>
    /// <param name="rating">The initial rating of the driver (must be between 0.0 and 5.0).</param>
    /// <param name="x">The X coordinate of the driver's location.</param>
    /// <param name="y">The Y coordinate of the driver's location.</param>
    /// <exception cref="ArgumentException">Thrown when rating is not between 0.0 and 5.0.</exception>
    public Driver(int id, string name, double rating, double x, double y)
    {
        if (rating < 0.0 || rating > 5.0)
        {
            throw new ArgumentException("Rating must be between 0.0 and 5.0.", nameof(rating));
        }

        Id = id;
        Name = name;
        Rating = rating;
        Location = (x, y);
        TotalRides = 0;
        RatingCount = 0;
        RatingSum = 0;
    }

    /// <summary>
    /// Sets the rating of the driver.
    /// </summary>
    /// <param name="rating">The new rating value (must be between 0.0 and 5.0).</param>
    /// <exception cref="ArgumentException">Thrown when rating is not between 0.0 and 5.0.</exception>
    public void SetRating(double rating)
    {
        if (rating < 0.0 || rating > 5.0)
        {
            throw new ArgumentException("Rating must be between 0.0 and 5.0.", nameof(rating));
        }

        Rating = rating;
    }

    /// <summary>
    /// Adds a new rating from a customer and recalculates the average.
    /// </summary>
    /// <param name="stars">The rating value (1-5 stars).</param>
    public void AddRating(int stars)
    {
        if (stars < 1 || stars > 5) return;

        RatingSum += stars;
        RatingCount++;
        Rating = Math.Round(RatingSum / RatingCount, 1);
    }

    /// <summary>
    /// Sets the rating data directly (used when loading from CSV).
    /// </summary>
    public void SetRatingData(double ratingSum, int ratingCount)
    {
        RatingSum = ratingSum;
        RatingCount = ratingCount;
        if (ratingCount > 0)
        {
            Rating = Math.Round(ratingSum / ratingCount, 1);
        }
        else
        {
            // If no ratings, set to default 5.0 (for new drivers)
            Rating = 5.0;
        }
    }

    /// <summary>
    /// Calculates the Euclidean distance from the driver's location to a specified point.
    /// </summary>
    /// <param name="point">The target point as (X, Y) coordinates.</param>
    /// <returns>The Euclidean distance to the specified point.</returns>
    public double DistanceTo((double X, double Y) point)
    {
        return Math.Sqrt(Math.Pow(Location.X - point.X, 2) + Math.Pow(Location.Y - point.Y, 2));
    }

    /// <summary>
    /// Increments the total number of rides completed by the driver by one.
    /// </summary>
    public void IncrementRides()
    {
        TotalRides++;
    }

    /// <summary>
    /// Decrements the total number of rides completed by the driver by one.
    /// </summary>
    public void DecrementRides()
    {
        if (TotalRides > 0)
        {
            TotalRides--;
        }
    }

    /// <summary>
    /// Sets the total number of rides directly.
    /// </summary>
    /// <param name="totalRides">The total rides count to set.</param>
    public void SetTotalRides(int totalRides)
    {
        TotalRides = Math.Max(0, totalRides);
    }

    /// <summary>
    /// Displays the driver's information to the console.
    /// </summary>
    public void Display()
    {
        Console.WriteLine($"ID: {Id} | Tên: {Name} | Đánh giá: {Rating:F1} sao | Vị trí: ({Location.X}, {Location.Y}) | Số chuyến: {TotalRides}");
    }

    /// <summary>
    /// Displays detailed driver information in a formatted box.
    /// </summary>
    public void DisplayDetailed()
    {
        Console.WriteLine("+--------------------------------------------+");
        Console.WriteLine("|          THONG TIN TAI XE                  |");
        Console.WriteLine("+--------------------------------------------+");
        Console.WriteLine($"| ID:               {Id,-24} |");
        Console.WriteLine($"| Ten:              {Name,-24} |");
        Console.WriteLine($"| Rating:           {Rating:F1} sao ({RatingCount} danh gia){"",-6} |");
        Console.WriteLine($"| Vi tri:           ({Location.X:F1}, {Location.Y:F1}){"",-14} |");
        Console.WriteLine($"| Tong so chuyen:   {TotalRides,-24} |");
        Console.WriteLine("+--------------------------------------------+");
    }

    /// <summary>
    /// Converts the driver's data to a CSV-formatted string.
    /// </summary>
    /// <returns>A comma-separated string containing the driver's data.</returns>
    public string ToCSV()
    {
        return $"{Id},{Name},{Rating},{Location.X},{Location.Y},{TotalRides},{RatingSum},{RatingCount}";
    }

    /// <summary>
    /// Creates a new <see cref="Driver"/> instance from a CSV-formatted string.
    /// </summary>
    /// <param name="csvLine">A comma-separated string containing driver data.</param>
    /// <returns>A new <see cref="Driver"/> instance populated with the parsed data.</returns>
    /// <exception cref="FormatException">Thrown when the CSV line cannot be parsed correctly.</exception>
    public static Driver FromCSV(string csvLine)
    {
        try
        {
            string[] parts = csvLine.Split(',');
            int id = int.Parse(parts[0]);
            string name = parts[1];
            double rating = double.Parse(parts[2]);
            double x = double.Parse(parts[3]);
            double y = double.Parse(parts[4]);
            int totalRides = int.Parse(parts[5]);

            Driver driver = new Driver(id, name, rating, x, y);
            driver.SetTotalRides(totalRides);

            // Load rating data if available (backward compatibility)
            if (parts.Length >= 8)
            {
                double ratingSum = double.Parse(parts[6]);
                int ratingCount = int.Parse(parts[7]);
                driver.SetRatingData(ratingSum, ratingCount);
            }

            return driver;
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException || ex is FormatException || ex is OverflowException)
        {
            throw new FormatException($"Failed to parse CSV line: {csvLine}", ex);
        }
    }
}
