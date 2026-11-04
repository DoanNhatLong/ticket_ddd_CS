using MediatR;

namespace TrainBooking.Application.Users.Queries.GetUsersSummary;

public sealed record GetUsersSummaryQuery : IRequest<IReadOnlyList<UserSummaryResponse>>;
