namespace MinRide.Algorithms;

/// <summary>
/// Provides spatial search algorithms for finding nearby items based on location.
/// </summary>
public static class SpatialSearch
{
    /// <summary>
    /// Finds all items within a specified radius of a target location.
    /// </summary>
    /// <typeparam name="T">The type of items to search.</typeparam>
    /// <param name="items">The list of items to search through.</param>
    /// <param name="getLocation">A function that extracts the (X, Y) location from an item.</param>
    /// <param name="targetLocation">The center point to search from.</param>
    /// <param name="radius">The maximum distance from the target location.</param>
    /// <returns>A list of tuples containing (Distance, Item) sorted by distance ascending.</returns>
    public static List<(double Distance, T Item)> FindNearby<T>(
        List<T> items,
        Func<T, (double X, double Y)> getLocation,
        (double X, double Y) targetLocation,
        double radius)
    {
        return items
            .Select(item => (Distance: CalculateDistance(getLocation(item), targetLocation), Item: item))
            .Where(t => t.Distance <= radius)
            .OrderBy(t => t.Distance)
            .ToList();
    }

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    /// <param name="point1">The first point as (X, Y) coordinates.</param>
    /// <param name="point2">The second point as (X, Y) coordinates.</param>
    /// <returns>The Euclidean distance between the two points.</returns>
    public static double CalculateDistance((double X, double Y) point1, (double X, double Y) point2)
    {
        return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
    }
}

