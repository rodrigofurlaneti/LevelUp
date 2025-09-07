namespace LevelUpClone.Domain.Services;

public static class ScoreCalculator
{
    public static int SumDaily(IEnumerable<int> points) => points.Sum();
}
