using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Dtos;
using TrainBooking.Application.Interfaces;
using TrainBooking.Infrastructure.Entity;

namespace TrainBooking.Infrastructure.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserSummaryResponse>> GetUsersSummaryAsync()
    {
        return await _context.Users
            .Select(u => new UserSummaryResponse
            {
                FullName = u.FullName,
                Role = u.Role
            })
            .ToListAsync();
    }
}
