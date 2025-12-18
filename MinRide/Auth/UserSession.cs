namespace MinRide.Auth;

using MinRide.Models;

/// <summary>
/// Represents the current user session in the MinRide system.
/// </summary>
public class UserSession
{
    /// <summary>
    /// Gets whether a user is currently logged in.
    /// </summary>
    public bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Gets the role of the currently logged in user.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Gets the ID of the currently logged in user (null for Admin).
    /// </summary>
    public int? UserId { get; private set; }

    /// <summary>
    /// Gets the username of the currently logged in user.
    /// </summary>
    public string? Username { get; private set; }

    /// <summary>
    /// Logs in a user with the specified role and optional user ID.
    /// </summary>
    /// <param name="role">The role of the user.</param>
    /// <param name="username">The username used to login.</param>
    /// <param name="userId">The ID of the user (null for Admin).</param>
    public void Login(UserRole role, string username, int? userId = null)
    {
        IsLoggedIn = true;
        Role = role;
        Username = username;
        UserId = userId;
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    public void Logout()
    {
        IsLoggedIn = false;
        UserId = null;
        Username = null;
    }
}

