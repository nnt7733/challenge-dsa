namespace MinRide.Utils;

/// <summary>
/// Provides validation helper methods for input data.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates a driver ID.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateDriverId(int id, out string? errorMessage)
    {
        if (id <= 0)
        {
            errorMessage = "ID phải là số nguyên dương";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a driver name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateDriverName(string? name, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errorMessage = "Tên không được để trống";
            return false;
        }
        if (name.Length < 2)
        {
            errorMessage = "Tên phải có ít nhất 2 ký tự";
            return false;
        }
        if (name.Length > 50)
        {
            errorMessage = "Tên không được quá 50 ký tự";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a driver rating.
    /// </summary>
    /// <param name="rating">The rating to validate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateRating(double rating, out string? errorMessage)
    {
        if (rating < 0.0 || rating > 5.0)
        {
            errorMessage = "Rating phải từ 0.0 đến 5.0";
            return false;
        }
        if (double.IsNaN(rating) || double.IsInfinity(rating))
        {
            errorMessage = "Rating không hợp lệ";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a location coordinate.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateLocation(double x, double y, out string? errorMessage)
    {
        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            errorMessage = "Tọa độ X không hợp lệ";
            return false;
        }
        if (double.IsNaN(y) || double.IsInfinity(y))
        {
            errorMessage = "Tọa độ Y không hợp lệ";
            return false;
        }
        // Optional: Add coordinate range validation if needed
        // if (x < -1000 || x > 1000 || y < -1000 || y > 1000)
        // {
        //     errorMessage = "Tọa độ nằm ngoài phạm vi cho phép";
        //     return false;
        // }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a customer ID.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateCustomerId(int id, out string? errorMessage)
    {
        if (id <= 0)
        {
            errorMessage = "ID khách hàng phải là số nguyên dương";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a customer name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateCustomerName(string? name, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errorMessage = "Tên khách hàng không được để trống";
            return false;
        }
        if (name.Length < 2)
        {
            errorMessage = "Tên phải có ít nhất 2 ký tự";
            return false;
        }
        if (name.Length > 50)
        {
            errorMessage = "Tên không được quá 50 ký tự";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a district name.
    /// </summary>
    /// <param name="district">The district name to validate.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateDistrict(string? district, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(district))
        {
            errorMessage = "Quận/Huyện không được để trống";
            return false;
        }
        if (district.Length > 30)
        {
            errorMessage = "Tên quận/huyện không được quá 30 ký tự";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a ride distance.
    /// </summary>
    /// <param name="distance">The distance in kilometers.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateDistance(double distance, out string? errorMessage)
    {
        if (distance <= 0)
        {
            errorMessage = "Khoảng cách phải lớn hơn 0";
            return false;
        }
        if (distance > 1000)
        {
            errorMessage = "Khoảng cách không được vượt quá 1000 km";
            return false;
        }
        if (double.IsNaN(distance) || double.IsInfinity(distance))
        {
            errorMessage = "Khoảng cách không hợp lệ";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a search radius.
    /// </summary>
    /// <param name="radius">The radius in kilometers.</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateRadius(double radius, out string? errorMessage)
    {
        if (radius <= 0)
        {
            errorMessage = "Bán kính phải lớn hơn 0";
            return false;
        }
        if (radius > 100)
        {
            errorMessage = "Bán kính không được vượt quá 100 km";
            return false;
        }
        if (double.IsNaN(radius) || double.IsInfinity(radius))
        {
            errorMessage = "Bán kính không hợp lệ";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates a K value for top-K queries.
    /// </summary>
    /// <param name="k">The K value.</param>
    /// <param name="maxCount">Maximum allowed value (default 1000).</param>
    /// <param name="errorMessage">Output error message if invalid.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool ValidateK(int k, out string? errorMessage, int maxCount = 1000)
    {
        if (k <= 0)
        {
            errorMessage = "K phải là số nguyên dương";
            return false;
        }
        if (k > maxCount)
        {
            errorMessage = $"K không được vượt quá {maxCount}";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates fare calculation (Distance × 12,000 VND).
    /// </summary>
    /// <param name="distance">The distance in kilometers.</param>
    /// <param name="expectedFare">The expected fare in VND.</param>
    /// <param name="errorMessage">Output error message if mismatch.</param>
    /// <returns><c>true</c> if fare matches; otherwise, <c>false</c>.</returns>
    public static bool ValidateFareCalculation(double distance, double expectedFare, out string? errorMessage)
    {
        const double FARE_PER_KM = 12000;
        double calculatedFare = distance * FARE_PER_KM;

        // Allow small floating point difference
        if (Math.Abs(calculatedFare - expectedFare) > 0.01)
        {
            errorMessage = $"Giá cước không khớp: Tính toán = {calculatedFare:N0}, Thực tế = {expectedFare:N0}";
            return false;
        }
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Calculates fare based on distance (Distance × 12,000 VND).
    /// </summary>
    /// <param name="distance">The distance in kilometers.</param>
    /// <returns>The calculated fare in VND.</returns>
    public static double CalculateFare(double distance)
    {
        const double FARE_PER_KM = 12000;
        return distance * FARE_PER_KM;
    }

    /// <summary>
    /// Validates all driver fields at once.
    /// </summary>
    public static bool ValidateDriver(int id, string? name, double rating, double x, double y, out string? errorMessage)
    {
        if (!ValidateDriverId(id, out errorMessage)) return false;
        if (!ValidateDriverName(name, out errorMessage)) return false;
        if (!ValidateRating(rating, out errorMessage)) return false;
        if (!ValidateLocation(x, y, out errorMessage)) return false;
        return true;
    }

    /// <summary>
    /// Validates all customer fields at once.
    /// </summary>
    public static bool ValidateCustomer(int id, string? name, string? district, double x, double y, out string? errorMessage)
    {
        if (!ValidateCustomerId(id, out errorMessage)) return false;
        if (!ValidateCustomerName(name, out errorMessage)) return false;
        if (!ValidateDistrict(district, out errorMessage)) return false;
        if (!ValidateLocation(x, y, out errorMessage)) return false;
        return true;
    }
}

