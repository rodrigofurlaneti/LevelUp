using FluentAssertions;
using LevelUpClone.Domain.Services;
using Xunit;

namespace LevelUpClone.Tests;

public sealed class ScoreCalculatorTests
{
    [Fact]
    public void SumDaily_ShouldSumPoints()
    {
        var total = ScoreCalculator.SumDaily(new[] { 1, 2, -2, 1 }); // IDE0300 ok
        total.Should().Be(2);
    }
}
