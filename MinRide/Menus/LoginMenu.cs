namespace MinRide.Menus;

using MinRide.Auth;
using MinRide.Models;
using MinRide.Utils;

/// <summary>
/// Handles user login.
/// </summary>
public class LoginMenu
{
    private readonly AuthManager authManager;
    private const int BoxWidth = 46;

    public LoginMenu(AuthManager auth)
    {
        authManager = auth;
    }

    public void Show()
    {
        Console.WriteLine();
        UIHelper.DrawHeader("ĐĂNG NHẬP", BoxWidth);
        UIHelper.DrawRow("Chọn loại tài khoản:", BoxWidth);
        UIHelper.DrawMenuOption("1", "Admin", BoxWidth);
        UIHelper.DrawMenuOption("2", "Khách hàng", BoxWidth);
        UIHelper.DrawMenuOption("3", "Tài xế", BoxWidth);
        UIHelper.DrawMenuOption("4", "Quay lại", BoxWidth);
        UIHelper.DrawLine(BoxWidth);

        string? choice = UIHelper.PromptChoice("Chọn: ");

        UserRole role;
        string roleLabel;

        switch (choice)
        {
            case "1": role = UserRole.ADMIN; roleLabel = "Admin"; break;
            case "2": role = UserRole.CUSTOMER; roleLabel = "Khách hàng"; break;
            case "3": role = UserRole.DRIVER; roleLabel = "Tài xế"; break;
            case "4": return;
            default: UIHelper.Error("Lựa chọn không hợp lệ."); return;
        }

        Console.WriteLine($"\n--- Đăng nhập {roleLabel} ---");

        string prompt = role == UserRole.ADMIN ? "Tên đăng nhập: " : "Nhập ID của bạn: ";
        string? username = UIHelper.Prompt(prompt);

        if (string.IsNullOrEmpty(username))
        {
            UIHelper.Error("Tên đăng nhập không được để trống.");
            return;
        }

        Console.Write("Mật khẩu: ");
        string? password = ReadPassword();

        if (string.IsNullOrEmpty(password))
        {
            UIHelper.Error("Mật khẩu không được để trống.");
            return;
        }

        if (authManager.Login(role, username, password))
        {
            Console.WriteLine();
            UIHelper.Success($"Đăng nhập thành công! Xin chào, {roleLabel}!");
        }
        else
        {
            Console.WriteLine();
            UIHelper.Error("Đăng nhập thất bại. Sai tên đăng nhập hoặc mật khẩu.");
        }
    }

    private string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}
