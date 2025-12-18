namespace MinRide.Auth;

using MinRide.Models;
using MinRide.Managers;

/// <summary>
/// Manages authentication, registration, and password operations.
/// </summary>
public class AuthManager
{
    private readonly CustomerManager customerManager;
    private readonly DriverManager driverManager;
    private Dictionary<string, string> passwords; // username -> password
    private const string PasswordsFilePath = "Data/passwords.csv";

    /// <summary>
    /// Gets the current user session.
    /// </summary>
    public UserSession Session { get; } = new UserSession();

    /// <summary>
    /// Initializes a new instance of the AuthManager class.
    /// </summary>
    public AuthManager(CustomerManager cm, DriverManager dm)
    {
        customerManager = cm;
        driverManager = dm;
        passwords = new Dictionary<string, string>();
        LoadPasswords();
    }

    /// <summary>
    /// Loads passwords from file.
    /// </summary>
    private void LoadPasswords()
    {
        // Default admin password
        passwords["admin"] = "admin";

        if (File.Exists(PasswordsFilePath))
        {
            try
            {
                var lines = File.ReadAllLines(PasswordsFilePath);
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        passwords[parts[0]] = parts[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải mật khẩu: {ex.Message}");
            }
        }

        // Sync passwords with existing customers and drivers (use ID as default password)
        foreach (var customer in customerManager.GetAll())
        {
            string username = $"C{customer.Id}";
            if (!passwords.ContainsKey(username))
            {
                passwords[username] = customer.Id.ToString();
            }
        }

        foreach (var driver in driverManager.GetAll())
        {
            string username = $"D{driver.Id}";
            if (!passwords.ContainsKey(username))
            {
                passwords[username] = driver.Id.ToString();
            }
        }
    }

    /// <summary>
    /// Saves passwords to file.
    /// </summary>
    public void SavePasswords()
    {
        try
        {
            Directory.CreateDirectory("Data");
            var lines = new List<string> { "Username,Password" };
            foreach (var kvp in passwords)
            {
                if (kvp.Key != "admin") // Don't save admin password
                {
                    lines.Add($"{kvp.Key},{kvp.Value}");
                }
            }
            File.WriteAllLines(PasswordsFilePath, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu mật khẩu: {ex.Message}");
        }
    }

    /// <summary>
    /// Authenticates a user login.
    /// </summary>
    /// <param name="role">The role to login as.</param>
    /// <param name="username">The username (ID for customers/drivers).</param>
    /// <param name="password">The password.</param>
    /// <returns>True if login successful, false otherwise.</returns>
    public bool Login(UserRole role, string username, string password)
    {
        switch (role)
        {
            case UserRole.ADMIN:
                if (username == "admin" && passwords.TryGetValue("admin", out string? adminPwd) && adminPwd == password)
                {
                    Session.Login(UserRole.ADMIN, "admin");
                    return true;
                }
                break;

            case UserRole.CUSTOMER:
                if (int.TryParse(username, out int customerId))
                {
                    string customerUsername = $"C{customerId}";
                    if (passwords.TryGetValue(customerUsername, out string? customerPwd) &&
                        customerPwd == password &&
                        customerManager.FindCustomerById(customerId) != null)
                    {
                        Session.Login(UserRole.CUSTOMER, customerUsername, customerId);
                        return true;
                    }
                }
                break;

            case UserRole.DRIVER:
                if (int.TryParse(username, out int driverId))
                {
                    string driverUsername = $"D{driverId}";
                    if (passwords.TryGetValue(driverUsername, out string? driverPwd) &&
                        driverPwd == password &&
                        driverManager.FindDriverById(driverId) != null)
                    {
                        Session.Login(UserRole.DRIVER, driverUsername, driverId);
                        return true;
                    }
                }
                break;
        }
        return false;
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    public void Logout()
    {
        Session.Logout();
    }

    /// <summary>
    /// Registers a new customer.
    /// </summary>
    /// <returns>The newly created customer, or null if failed.</returns>
    public Customer? RegisterCustomer(string name, string district, double x, double y)
    {
        int newId = customerManager.GetNextId();
        var customer = new Customer(newId, name, district, x, y);

        if (customerManager.AddCustomerWithValidation(customer, out string? error))
        {
            // Set default password as ID
            string username = $"C{newId}";
            passwords[username] = newId.ToString();
            SavePasswords();

            Session.Login(UserRole.CUSTOMER, username, newId);
            return customer;
        }

        Console.WriteLine($"[X] Loi dang ky: {error}");
        return null;
    }

    /// <summary>
    /// Registers a new driver.
    /// </summary>
    /// <returns>The newly created driver, or null if failed.</returns>
    public Driver? RegisterDriver(string name, double x, double y)
    {
        int newId = driverManager.GetNextId();
        var driver = new Driver(newId, name, 5.0, x, y); // Default rating 5.0 for new drivers

        if (driverManager.AddDriverWithValidation(driver, out string? error))
        {
            // Set default password as ID
            string username = $"D{newId}";
            passwords[username] = newId.ToString();
            SavePasswords();

            Session.Login(UserRole.DRIVER, username, newId);
            return driver;
        }

        Console.WriteLine($"[X] Loi dang ky: {error}");
        return null;
    }

    /// <summary>
    /// Changes the password for the current user.
    /// </summary>
    /// <param name="oldPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if password changed successfully.</returns>
    public bool ChangePassword(string oldPassword, string newPassword)
    {
        if (!Session.IsLoggedIn || Session.Username == null)
        {
            return false;
        }

        string username = Session.Username;

        if (!passwords.TryGetValue(username, out string? currentPwd) || currentPwd != oldPassword)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 1)
        {
            return false;
        }

        passwords[username] = newPassword;
        SavePasswords();
        return true;
    }

    /// <summary>
    /// Gets the current customer if logged in as customer.
    /// </summary>
    public Customer? GetCurrentCustomer()
    {
        if (Session.IsLoggedIn && Session.Role == UserRole.CUSTOMER && Session.UserId.HasValue)
        {
            return customerManager.FindCustomerById(Session.UserId.Value);
        }
        return null;
    }

    /// <summary>
    /// Gets the current driver if logged in as driver.
    /// </summary>
    public Driver? GetCurrentDriver()
    {
        if (Session.IsLoggedIn && Session.Role == UserRole.DRIVER && Session.UserId.HasValue)
        {
            return driverManager.FindDriverById(Session.UserId.Value);
        }
        return null;
    }
}

