namespace MinRide.Models;

/// <summary>
/// Represents a customer in the MinRide system.
/// </summary>
public class Customer
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the customer.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the district where the customer is located.
    /// </summary>
    public string District { get; set; }

    /// <summary>
    /// Gets or sets the current location of the customer as (X, Y) coordinates.
    /// </summary>
    public (double X, double Y) Location { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the customer.</param>
    /// <param name="name">The name of the customer.</param>
    /// <param name="district">The district where the customer is located.</param>
    /// <param name="x">The X coordinate of the customer's location.</param>
    /// <param name="y">The Y coordinate of the customer's location.</param>
    public Customer(int id, string name, string district, double x, double y)
    {
        Id = id;
        Name = name;
        District = district;
        Location = (x, y);
    }

    /// <summary>
    /// Displays the customer's information to the console.
    /// </summary>
    public void Display()
    {
        Console.WriteLine($"ID: {Id} | Name: {Name} | District: {District} | Location: ({Location.X}, {Location.Y})");
    }

    /// <summary>
    /// Displays detailed customer information in a formatted box.
    /// </summary>
    public void DisplayDetailed()
    {
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║         THÔNG TIN KHÁCH HÀNG             ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine($"║ ID:              {Id,-24} ║");
        Console.WriteLine($"║ Tên:             {Name,-24} ║");
        Console.WriteLine($"║ Quận/Huyện:      {District,-24} ║");
        Console.WriteLine($"║ Vị trí:          ({Location.X:F1}, {Location.Y:F1}){"",-14} ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
    }

    /// <summary>
    /// Converts the customer's data to a CSV-formatted string.
    /// </summary>
    /// <returns>A comma-separated string containing the customer's data.</returns>
    public string ToCSV()
    {
        return $"{Id},{Name},{District},{Location.X},{Location.Y}";
    }

    /// <summary>
    /// Creates a new <see cref="Customer"/> instance from a CSV-formatted string.
    /// </summary>
    /// <param name="csvLine">A comma-separated string containing customer data.</param>
    /// <returns>A new <see cref="Customer"/> instance populated with the parsed data.</returns>
    /// <exception cref="FormatException">Thrown when the CSV line cannot be parsed correctly.</exception>
    public static Customer FromCSV(string csvLine)
    {
        try
        {
            string[] parts = csvLine.Split(',');
            int id = int.Parse(parts[0]);
            string name = parts[1];
            string district = parts[2];
            double x = double.Parse(parts[3]);
            double y = double.Parse(parts[4]);

            return new Customer(id, name, district, x, y);
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException || ex is FormatException || ex is OverflowException)
        {
            throw new FormatException($"Failed to parse CSV line: {csvLine}", ex);
        }
    }
}

