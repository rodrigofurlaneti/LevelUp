using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.DTOs;
using LevelUpClone.Domain.Interfaces;

namespace LevelUpClone.Application.Cqrs.Scores;

public sealed class GetDailyScoreQuery : IQuery<DailyScoreDto>
{
    public int UserId { get; init; }
    public string ActivityDate { get; init; } = "";
}

public sealed class GetDailyScoreHandler : IQueryHandler<GetDailyScoreQuery, DailyScoreDto>
{
    private readonly IActivityLogRepository _repo;
    public GetDailyScoreHandler(IActivityLogRepository repo) => _repo = repo;
    public DailyScoreDto Handle(GetDailyScoreQuery q)
    {
        var date = DateOnly.ParseExact(q.ActivityDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var total = _repo.GetDailyScore(q.UserId, date);
        return new DailyScoreDto { TotalPoints = total };
    }
}
