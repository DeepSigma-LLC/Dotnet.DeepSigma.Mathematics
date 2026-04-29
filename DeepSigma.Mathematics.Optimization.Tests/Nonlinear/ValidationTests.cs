namespace DeepSigma.Mathematics.Optimization.Nonlinear.Tests;

// Class name explicitly disambiguated from the LP ValidationTests in the sibling Linear folder.
public sealed class NlpValidationTests
{
    [Fact]
    public void Solve_RejectsNullObjective()
    {
        Assert.Throws<ArgumentNullException>(() => Slsqp.Solve(null!, [0.0, 0.0]));
    }

    [Fact]
    public void Solve_RejectsEmptyInitialGuess()
    {
        Assert.Throws<ArgumentException>(() => Slsqp.Solve(
            x => 0.0,
            ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Solve_RejectsNonFiniteInitialGuess()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Slsqp.Solve(
            x => 0.0,
            [double.NaN, 1.0]));
    }

    [Fact]
    public void Solve_RejectsMismatchedLowerBoundsLength()
    {
        Assert.Throws<ArgumentException>(() => Slsqp.Solve(
            x => 0.0,
            [0.0, 0.0],
            lowerBounds: [0.0]));
    }

    [Fact]
    public void Solve_RejectsLowerGreaterThanUpper()
    {
        Assert.Throws<ArgumentException>(() => Slsqp.Solve(
            x => 0.0,
            [0.5],
            lowerBounds: [10.0],
            upperBounds: [5.0]));
    }

    [Fact]
    public void Solve_RejectsDuplicateConstraintNames()
    {
        NlpConstraint a = new("c", x => x[0]);
        NlpConstraint b = new("c", x => x[0] - 1.0);
        Assert.Throws<ArgumentException>(() => Slsqp.Solve(
            x => x[0] * x[0],
            [0.5],
            inequalityConstraints: [a, b]));
    }

    [Fact]
    public void Solve_RejectsConstraintWithEmptyName()
    {
        NlpConstraint bad = new("", x => x[0]);
        Assert.Throws<ArgumentException>(() => Slsqp.Solve(
            x => x[0] * x[0],
            [0.5],
            inequalityConstraints: [bad]));
    }

    [Fact]
    public void Solve_ThrowsWhenObjectiveReturnsNaN()
    {
        Assert.Throws<InvalidOperationException>(() => Slsqp.Solve(
            x => double.NaN,
            [0.5, 0.5]));
    }

    [Fact]
    public void Options_RejectsInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Slsqp.Solve(
            x => 0.0,
            [0.5],
            options: new ConstrainedNlpOptions { MaxIterations = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Slsqp.Solve(
            x => 0.0,
            [0.5],
            options: new ConstrainedNlpOptions { FunctionTolerance = -1.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Slsqp.Solve(
            x => 0.0,
            [0.5],
            options: new ConstrainedNlpOptions { ParameterTolerance = 0.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => Slsqp.Solve(
            x => 0.0,
            [0.5],
            options: new ConstrainedNlpOptions { TimeLimit = TimeSpan.Zero }));
    }
}
