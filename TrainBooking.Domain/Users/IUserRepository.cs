namespace TrainBooking.Domain.Users;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by id, or <c>null</c> when no such user exists.
    /// </summary>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
