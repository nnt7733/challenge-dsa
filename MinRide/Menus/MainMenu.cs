namespace MinRide.Menus;

using MinRide.Auth;
using MinRide.Managers;
using MinRide.Models;
using MinRide.Utils;

/// <summary>
/// The main entry menu for the application.
/// </summary>
public class MainMenu
{
    private readonly AuthManager authManager;
    private readonly CustomerManager customerManager;
    private readonly DriverManager driverManager;
    private readonly RideManager rideManager;

    private readonly LoginMenu loginMenu;
    private readonly RegisterMenu registerMenu;
    private readonly AdminMenu adminMenu;
    private readonly CustomerMenu customerMenu;
    private readonly DriverMenu driverMenu;

    private const int BoxWidth = 46;

    public MainMenu(AuthManager auth, CustomerManager cm, DriverManager dm, RideManager rm)
    {
        authManager = auth;
        customerManager = cm;
        driverManager = dm;
        rideManager = rm;

        loginMenu = new LoginMenu(authManager);
        registerMenu = new RegisterMenu(authManager);
        adminMenu = new AdminMenu(authManager, customerManager, driverManager, rideManager);
        customerMenu = new CustomerMenu(authManager, customerManager, driverManager, rideManager);
        driverMenu = new DriverMenu(authManager, driverManager, rideManager);
    }

    public void Run()
    {
        bool running = true;

        while (running)
        {
            if (!authManager.Session.IsLoggedIn)
            {
                running = ShowWelcomeMenu();
            }
            else
            {
                ShowRoleMenu();
            }
        }
    }

    private bool ShowWelcomeMenu()
    {
        Console.WriteLine();
        UIHelper.DrawLine(BoxWidth);
        UIHelper.DrawCenteredRow("MINRIDE", BoxWidth);
        UIHelper.DrawCenteredRow("Hệ thống quản lý đặt xe", BoxWidth);
        UIHelper.DrawLine(BoxWidth);
        UIHelper.DrawMenuOption("1", "Đăng nhập", BoxWidth);
        UIHelper.DrawMenuOption("2", "Đăng ký", BoxWidth);
        UIHelper.DrawMenuOption("3", "Thoát", BoxWidth);
        UIHelper.DrawLine(BoxWidth);

        string? choice = UIHelper.PromptChoice("Chọn chức năng: ");

        switch (choice)
        {
            case "1": loginMenu.Show(); break;
            case "2": registerMenu.Show(); break;
            case "3": return false;
            default: UIHelper.Error("Lựa chọn không hợp lệ."); break;
        }

        return true;
    }

    private void ShowRoleMenu()
    {
        switch (authManager.Session.Role)
        {
            case UserRole.ADMIN: adminMenu.Show(); break;
            case UserRole.CUSTOMER: customerMenu.Show(); break;
            case UserRole.DRIVER: driverMenu.Show(); break;
        }
    }
}
