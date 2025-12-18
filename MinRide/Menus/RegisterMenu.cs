namespace MinRide.Menus;

using MinRide.Auth;
using MinRide.Models;
using MinRide.Utils;

/// <summary>
/// Handles user registration.
/// </summary>
public class RegisterMenu
{
    private readonly AuthManager authManager;
    private const int BoxWidth = 46;

    public RegisterMenu(AuthManager auth)
    {
        authManager = auth;
    }

    public void Show()
    {
        Console.WriteLine();
        UIHelper.DrawHeader("ĐĂNG KÝ", BoxWidth);
        UIHelper.DrawRow("Đăng ký làm:", BoxWidth);
        UIHelper.DrawMenuOption("1", "Khách hàng", BoxWidth);
        UIHelper.DrawMenuOption("2", "Tài xế", BoxWidth);
        UIHelper.DrawMenuOption("3", "Quay lại", BoxWidth);
        UIHelper.DrawLine(BoxWidth);

        string? choice = UIHelper.PromptChoice("Chọn: ");

        switch (choice)
        {
            case "1": RegisterCustomer(); break;
            case "2": RegisterDriver(); break;
            case "3": return;
            default: UIHelper.Error("Lựa chọn không hợp lệ."); break;
        }
    }

    private void RegisterCustomer()
    {
        Console.WriteLine("\n--- ĐĂNG KÝ KHÁCH HÀNG ---");

        string? name = UIHelper.Prompt("Nhập họ tên: ");
        if (string.IsNullOrEmpty(name))
        {
            UIHelper.Error("Tên không được để trống.");
            return;
        }

        string? district = UIHelper.Prompt("Nhập quận/huyện: ");
        if (string.IsNullOrEmpty(district))
        {
            UIHelper.Error("Quận/huyện không được để trống.");
            return;
        }

        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ X (vị trí của bạn): "), out double x))
        {
            UIHelper.Error("Tọa độ X không hợp lệ.");
            return;
        }

        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ Y (vị trí của bạn): "), out double y))
        {
            UIHelper.Error("Tọa độ Y không hợp lệ.");
            return;
        }

        var customer = authManager.RegisterCustomer(name, district, x, y);
        if (customer != null)
        {
            Console.WriteLine();
            UIHelper.DrawHeader("[OK] ĐĂNG KÝ THÀNH CÔNG!", BoxWidth);
            UIHelper.DrawLabelValue("ID của bạn:", customer.Id.ToString(), 16, BoxWidth);
            UIHelper.DrawLabelValue("Tên đăng nhập:", customer.Id.ToString(), 16, BoxWidth);
            UIHelper.DrawLabelValue("Mật khẩu:", customer.Id.ToString(), 16, BoxWidth);
            UIHelper.DrawLine(BoxWidth);
            UIHelper.DrawRow("Hãy đổi mật khẩu sau khi đăng nhập!", BoxWidth);
            UIHelper.DrawLine(BoxWidth);
        }
    }

    private void RegisterDriver()
    {
        Console.WriteLine("\n--- ĐĂNG KÝ TÀI XẾ ---");

        string? name = UIHelper.Prompt("Nhập họ tên: ");
        if (string.IsNullOrEmpty(name))
        {
            UIHelper.Error("Tên không được để trống.");
            return;
        }

        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ X (vị trí hiện tại): "), out double x))
        {
            UIHelper.Error("Tọa độ X không hợp lệ.");
            return;
        }

        if (!double.TryParse(UIHelper.Prompt("Nhập tọa độ Y (vị trí hiện tại): "), out double y))
        {
            UIHelper.Error("Tọa độ Y không hợp lệ.");
            return;
        }

        var driver = authManager.RegisterDriver(name, x, y);
        if (driver != null)
        {
            Console.WriteLine();
            UIHelper.DrawHeader("[OK] ĐĂNG KÝ THÀNH CÔNG!", BoxWidth);
            UIHelper.DrawLabelValue("ID của bạn:", driver.Id.ToString(), 16, BoxWidth);
            UIHelper.DrawLabelValue("Tên đăng nhập:", driver.Id.ToString(), 16, BoxWidth);
            UIHelper.DrawLabelValue("Mật khẩu:", driver.Id.ToString(), 16, BoxWidth);
            UIHelper.DrawLabelValue("Đánh giá ban đầu:", "5.0 sao", 16, BoxWidth);
            UIHelper.DrawLine(BoxWidth);
            UIHelper.DrawRow("Hãy đổi mật khẩu sau khi đăng nhập!", BoxWidth);
            UIHelper.DrawLine(BoxWidth);
        }
    }
}
