using MinRide.Models;
using MinRide.Utils;
using MinRide.Algorithms;

namespace MinRide.Managers;

/// <summary>
/// Manages a collection of customers with efficient lookup and district-based indexing.
/// </summary>
public class CustomerManager
{
    /// <summary>
    /// The list of all customers.
    /// </summary>
    private List<Customer> customers;

    /// <summary>
    /// Maps customer ID to list index for O(1) lookup.
    /// </summary>
    private Dictionary<int, int> idToIndex;

    /// <summary>
    /// Maps district name to list of customer references in that district.
    /// </summary>
    private Dictionary<string, List<Customer>> districtIndex;

    /// <summary>
    /// Trie data structure for efficient name-based searches.
    /// </summary>
    private NameTrie nameTrie;

    /// <summary>
    /// The undo stack for reversible operations.
    /// </summary>
    private UndoStack? undoStack;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerManager"/> class.
    /// </summary>
    public CustomerManager()
    {
        customers = new List<Customer>();
        idToIndex = new Dictionary<int, int>();
        districtIndex = new Dictionary<string, List<Customer>>();
        nameTrie = new NameTrie();
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
    /// Adds a new customer to the collection.
    /// </summary>
    /// <param name="customer">The customer to add.</param>
    /// <param name="silent">If true, suppresses console output.</param>
    public void AddCustomer(Customer customer, bool silent = false)
    {
        idToIndex[customer.Id] = customers.Count;
        customer.IsDeleted = false;
        customers.Add(customer);
        nameTrie.Insert(customer.Name, customer.Id);

        // Add to district index
        if (!districtIndex.ContainsKey(customer.District))
        {
            districtIndex[customer.District] = new List<Customer>();
        }
        districtIndex[customer.District].Add(customer);

        if (!silent)
        {
            Console.WriteLine($"[OK] Da them khach hang {customer.Name} (ID: {customer.Id})");
        }
    }

    /// <summary>
    /// Deletes a customer from the collection by ID using lazy deletion.
    /// </summary>
    /// <param name="id">The ID of the customer to delete.</param>
    /// <returns><c>true</c> if the customer was marked as deleted; <c>false</c> if not found.</returns>
    public bool DeleteCustomer(int id)
    {
        if (!idToIndex.TryGetValue(id, out int index))
        {
            return false;
        }

        Customer customer = customers[index];

        // Use lazy deletion - just mark as deleted
        customer.IsDeleted = true;
        nameTrie.Remove(customer.Name, id);

        // Remove from district index
        if (districtIndex.TryGetValue(customer.District, out var districtCustomers))
        {
            districtCustomers.Remove(customer);
            if (districtCustomers.Count == 0)
            {
                districtIndex.Remove(customer.District);
            }
        }

        return true;
    }

    /// <summary>
    /// Finds a customer by their ID using O(1) dictionary lookup.
    /// </summary>
    /// <param name="id">The ID of the customer to find.</param>
    /// <returns>The customer if found and not deleted; otherwise, <c>null</c>.</returns>
    public Customer? FindCustomerById(int id)
    {
        if (idToIndex.TryGetValue(id, out int index))
        {
            Customer customer = customers[index];
            // Return null if the customer is marked as deleted
            return customer.IsDeleted ? null : customer;
        }
        return null;
    }

    /// <summary>
    /// Finds all customers whose name starts with the specified prefix using Trie.
    /// Time complexity: O(L + M) where L is prefix length and M is number of matching customers.
    /// </summary>
    /// <param name="prefix">The name prefix to search for.</param>
    /// <returns>A list of customers matching the prefix and not deleted.</returns>
    public List<Customer> FindCustomersByNamePrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return new List<Customer>();

        List<int> matchingIds = nameTrie.SearchByPrefix(prefix);
        var result = new List<Customer>();

        foreach (int id in matchingIds)
        {
            if (idToIndex.TryGetValue(id, out int index))
            {
                Customer customer = customers[index];
                if (!customer.IsDeleted)
                {
                    result.Add(customer);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Finds all customers whose name contains the specified search string (substring search).
    /// </summary>
    /// <param name="name">The name or partial name to search for.</param>
    /// <returns>A list of customers matching the search criteria and not deleted.</returns>
    public List<Customer> FindCustomersByName(string name)
    {
        return customers
            .Where(c => !c.IsDeleted && c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets the top K customers sorted by ID.
    /// </summary>
    /// <param name="k">The number of customers to return.</param>
    /// <param name="highest">If <c>true</c>, returns highest IDs; otherwise, lowest IDs. Default is highest.</param>
    /// <returns>A list of top K customers.</returns>
    public List<Customer> GetTopK(int k, bool highest = true)
    {
        var activeCustomers = customers.Where(c => !c.IsDeleted).ToList();
        return highest
            ? activeCustomers.OrderByDescending(c => c.Id).Take(k).ToList()
            : activeCustomers.OrderBy(c => c.Id).Take(k).ToList();
    }

    /// <summary>
    /// Gets customers in a specific district with pagination support using optimized index.
    /// </summary>
    /// <param name="district">The district name to filter by.</param>
    /// <param name="skip">The number of customers to skip. Default is 0.</param>
    /// <param name="take">The maximum number of customers to return. Default is 10.</param>
    /// <returns>A list of active customers in the specified district.</returns>
    public List<Customer> GetCustomersByDistrict(string district, int skip = 0, int take = 10)
    {
        if (!districtIndex.TryGetValue(district, out var districtCustomers))
        {
            return new List<Customer>();
        }

        // Filter out deleted customers and apply pagination
        return districtCustomers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Id)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    /// <summary>
    /// Gets the count of active customers in a specific district.
    /// </summary>
    /// <param name="district">The district name to count.</param>
    /// <returns>The number of active customers in the district, or 0 if the district doesn't exist.</returns>
    public int GetDistrictCount(string district)
    {
        if (!districtIndex.TryGetValue(district, out var districtCustomers))
            return 0;
        return districtCustomers.Count(c => !c.IsDeleted);
    }

    /// <summary>
    /// Displays all customers in the collection (excluding deleted ones).
    /// </summary>
    public void DisplayAll()
    {
        foreach (var customer in customers.Where(c => !c.IsDeleted))
        {
            customer.Display();
        }
    }

    /// <summary>
    /// Gets all customers in the collection (excluding deleted ones).
    /// </summary>
    /// <returns>The list of all active customers.</returns>
    public List<Customer> GetAll()
    {
        return customers.Where(c => !c.IsDeleted).ToList();
    }

    /// <summary>
    /// Gets the total count of active customers.
    /// </summary>
    /// <returns>The number of active customers in the collection.</returns>
    public int GetCount()
    {
        return customers.Count(c => !c.IsDeleted);
    }

    /// <summary>
    /// Gets all distinct district names.
    /// </summary>
    /// <returns>List of district names.</returns>
    public List<string> GetAllDistricts()
    {
        return districtIndex.Keys.ToList();
    }

    /// <summary>
    /// Gets the next available ID for a new customer.
    /// </summary>
    /// <returns>The next available customer ID.</returns>
    public int GetNextId()
    {
        if (customers.Count == 0)
            return 1;
        return customers.Max(c => c.Id) + 1;
    }

    /// <summary>
    /// Updates a customer's information with optional parameters.
    /// </summary>
    /// <param name="id">The ID of the customer to update.</param>
    /// <param name="newName">The new name (optional).</param>
    /// <param name="newDistrict">The new district (optional).</param>
    /// <param name="newX">The new X coordinate (optional).</param>
    /// <param name="newY">The new Y coordinate (optional).</param>
    /// <returns><c>true</c> if the customer was updated; <c>false</c> if not found.</returns>
    public bool UpdateCustomer(int id, string? newName = null, string? newDistrict = null, double? newX = null, double? newY = null)
    {
        var customer = FindCustomerById(id);
        if (customer == null)
        {
            return false;
        }

        // Store old values for undo
        string oldName = customer.Name;
        string oldDistrict = customer.District;
        double oldX = customer.Location.X;
        double oldY = customer.Location.Y;

        // Update Trie if name is changing
        if (newName != null && newName != oldName)
        {
            nameTrie.Remove(oldName, id);
            customer.Name = newName;
            nameTrie.Insert(newName, id);
        }

        // Update district index if district is changing
        if (newDistrict != null && newDistrict != oldDistrict)
        {
            // Remove from old district
            if (districtIndex.TryGetValue(oldDistrict, out var oldDistrictList))
            {
                oldDistrictList.Remove(customer);
                if (oldDistrictList.Count == 0)
                {
                    districtIndex.Remove(oldDistrict);
                }
            }

            // Add to new district
            if (!districtIndex.ContainsKey(newDistrict))
            {
                districtIndex[newDistrict] = new List<Customer>();
            }
            districtIndex[newDistrict].Add(customer);

            customer.District = newDistrict;
        }

        // Update other fields
        if (newX.HasValue || newY.HasValue)
        {
            double x = newX ?? customer.Location.X;
            double y = newY ?? customer.Location.Y;
            customer.Location = (x, y);
        }

        // Push undo action
        undoStack?.Push(() =>
        {
            // Restore Trie if name was changed
            if (oldName != customer.Name)
            {
                nameTrie.Remove(customer.Name, id);
                customer.Name = oldName;
                nameTrie.Insert(oldName, id);
            }

            // Restore district index
            if (oldDistrict != customer.District)
            {
                if (districtIndex.TryGetValue(customer.District, out var currentDistrictList))
                {
                    currentDistrictList.Remove(customer);
                    if (currentDistrictList.Count == 0)
                    {
                        districtIndex.Remove(customer.District);
                    }
                }

                if (!districtIndex.ContainsKey(oldDistrict))
                {
                    districtIndex[oldDistrict] = new List<Customer>();
                }
                districtIndex[oldDistrict].Add(customer);
            }

            customer.District = oldDistrict;
            customer.Location = (oldX, oldY);
            Console.WriteLine($"Đã hoàn tác cập nhật khách hàng ID: {id}");
        });

        return true;
    }

    /// <summary>
    /// Adds a customer with full validation.
    /// </summary>
    /// <param name="customer">The customer to add.</param>
    /// <param name="errorMessage">Output error message if validation fails.</param>
    /// <returns><c>true</c> if customer was added successfully; otherwise, <c>false</c>.</returns>
    public bool AddCustomerWithValidation(Customer customer, out string? errorMessage)
    {
        // Check if ID already exists
        if (idToIndex.ContainsKey(customer.Id))
        {
            errorMessage = "ID đã tồn tại";
            return false;
        }

        // Check if Name is empty
        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            errorMessage = "Tên không được để trống";
            return false;
        }

        // Check if District is empty
        if (string.IsNullOrWhiteSpace(customer.District))
        {
            errorMessage = "Quận/Huyện không được để trống";
            return false;
        }

        // Check if Location is valid
        if (double.IsNaN(customer.Location.X) || double.IsInfinity(customer.Location.X) ||
            double.IsNaN(customer.Location.Y) || double.IsInfinity(customer.Location.Y))
        {
            errorMessage = "Tọa độ không hợp lệ";
            return false;
        }

        // All validations passed
        AddCustomer(customer);
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Interactive method to update a customer by ID.
    /// </summary>
    public void UpdateCustomerInteractive()
    {
        Console.Write("Nhập ID khách hàng cần cập nhật: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID không hợp lệ.");
            return;
        }

        var customer = FindCustomerById(id);
        if (customer == null)
        {
            Console.WriteLine("Không tìm thấy khách hàng với ID này.");
            return;
        }

        Console.WriteLine("\n--- THÔNG TIN HIỆN TẠI ---");
        customer.DisplayDetailed();

        Console.WriteLine("\nChọn thông tin cần cập nhật:");
        Console.WriteLine("1. Tên");
        Console.WriteLine("2. Quận/Huyện");
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

        string? newName = null;
        string? newDistrict = null;
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
                Console.Write("Nhập quận/huyện mới: ");
                newDistrict = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(newDistrict))
                {
                    Console.WriteLine("Quận/Huyện không được để trống.");
                    return;
                }
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

                Console.Write("Nhập quận/huyện mới (Enter để giữ nguyên): ");
                string? inputDistrict = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(inputDistrict)) newDistrict = inputDistrict;

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

        if (UpdateCustomer(id, newName, newDistrict, newX, newY))
        {
            Console.WriteLine("\n[OK] Cap nhat thanh cong!");
            Console.WriteLine("--- THÔNG TIN SAU CẬP NHẬT ---");
            customer.DisplayDetailed();
        }
        else
        {
            Console.WriteLine("Cập nhật thất bại.");
        }
    }

    /// <summary>
    /// Displays customers by district with pagination.
    /// </summary>
    /// <param name="district">The district name.</param>
    public void DisplayCustomersByDistrictPaginated(string district)
    {
        int totalCount = GetDistrictCount(district);

        if (totalCount == 0)
        {
            Console.WriteLine("Không có khách hàng trong quận này.");
            return;
        }

        Console.WriteLine($"\nTổng số khách hàng trong {district}: {totalCount}");

        const int pageSize = 10;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        while (true)
        {
            var customers = GetCustomersByDistrict(district, currentPage * pageSize, pageSize);

            Console.WriteLine($"\n--- KHACH HANG QUAN {district.ToUpper()} ---");
            Console.WriteLine("+-----+--------+------------------------+-----------------+");
            Console.WriteLine("| STT |   ID   |          Ten           |     Vi tri      |");
            Console.WriteLine("+-----+--------+------------------------+-----------------+");

            int stt = currentPage * pageSize + 1;
            foreach (var c in customers)
            {
                string name = c.Name.Length > 22 ? c.Name.Substring(0, 19) + "..." : c.Name;
                Console.WriteLine($"| {stt,3} | {c.Id,6} | {name,-22} | ({c.Location.X:F1}, {c.Location.Y:F1}){"",-5} |");
                stt++;
            }
            Console.WriteLine("+-----+--------+------------------------+-----------------+");

            Console.WriteLine($"\nTrang {currentPage + 1} / {totalPages} | Tổng: {totalCount} khách hàng");

            if (totalPages <= 1)
            {
                break;
            }

            Console.WriteLine("\n1. Trang tiếp");
            Console.WriteLine("2. Trang trước");
            Console.WriteLine("3. Quay lại");
            Console.Write("Lựa chọn: ");

            string? navChoice = Console.ReadLine()?.Trim();

            switch (navChoice)
            {
                case "1":
                    if (currentPage < totalPages - 1)
                    {
                        currentPage++;
                    }
                    else
                    {
                        Console.WriteLine("Đã ở trang cuối.");
                    }
                    break;

                case "2":
                    if (currentPage > 0)
                    {
                        currentPage--;
                    }
                    else
                    {
                        Console.WriteLine("Đã ở trang đầu.");
                    }
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }
}

