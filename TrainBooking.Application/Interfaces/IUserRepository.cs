namespace TrainBooking.Application.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<Dtos.UserSummaryResponse>> GetUsersSummaryAsync();
}
