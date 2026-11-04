using TrainBooking.Domain.Abstractions;

namespace TrainBooking.Domain.Users;

/// <summary>
/// User aggregate root.
/// </summary>
public sealed class User : Entity
{
    private User(
        int id,
        string email,
        string passwordHash,
        string fullName,
        string phoneNumber,
        UserRole role,
        DateTime? createdAt)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Role = role;
        CreatedAt = createdAt;
    }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string FullName { get; private set; }

    public string PhoneNumber { get; private set; }

    public UserRole Role { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    /// <summary>
    /// Rebuilds an existing user from persisted state.
    /// Intended for repositories only; new users must go through a dedicated factory method.
    /// </summary>
    public static User Rehydrate(
        int id,
        string email,
        string passwordHash,
        string fullName,
        string phoneNumber,
        UserRole role,
        DateTime? createdAt)
    {
        return new User(id, email, passwordHash, fullName, phoneNumber, role, createdAt);
    }
}
