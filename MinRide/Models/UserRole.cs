namespace MinRide.Models;

/// <summary>
/// Defines the types of users in the MinRide system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator with full system access.
    /// </summary>
    ADMIN,

    /// <summary>
    /// Customer who can book rides and rate drivers.
    /// </summary>
    CUSTOMER,

    /// <summary>
    /// Driver who can view their information and ride history.
    /// </summary>
    DRIVER
}

