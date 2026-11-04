using Microsoft.EntityFrameworkCore;
using TrainBooking.Domain.Users;
using TrainBooking.Infrastructure.Entity;
using DomainUser = TrainBooking.Domain.Users.User;
using UserRecord = TrainBooking.Infrastructure.Entity.User;

namespace TrainBooking.Infrastructure.Repositories.UserRepository;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>.
/// Reads the scaffolded persistence model and maps it to the domain aggregate.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DomainUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return records.Select(ToDomain).ToList();
    }

    private static DomainUser ToDomain(UserRecord record)
    {
        return DomainUser.Rehydrate(
            record.Id,
            record.Email,
            record.PasswordHash,
            record.FullName,
            record.PhoneNumber,
            ParseRole(record.Role),
            record.CreatedAt);
    }

    private static UserRole ParseRole(string role)
    {
        if (Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Unknown user role '{role}' found in database.");
    }
}
