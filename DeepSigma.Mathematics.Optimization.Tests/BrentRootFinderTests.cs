namespace DeepSigma.Mathematics.Optimization.Tests;

public sealed class BrentRootFinderTests
{
    [Fact]
    public void FindRoot_SolvesClassicCubic()
    {
        // Brent's original 1973 textbook example: f(x) = x^3 - 2x - 5.
        // The unique real root is approximately 2.0945514815423265.
        const double expected = 2.0945514815423265;

        RootFindingResult result = BrentRootFinder.FindRoot(
            x => x * x * x - 2.0 * x - 5.0,
            lowerBound: 2.0,
            upperBound: 3.0);

        Assert.True(result.Converged);
        Assert.Equal(expected, result.Root, precision: 10);
        Assert.True(Math.Abs(result.FunctionValue) < 1e-10);
    }

    [Fact]
    public void FindRoot_FindsPiAsRootOfSine()
    {
        RootFindingResult result = BrentRootFinder.FindRoot(
            Math.Sin,
            lowerBound: 3.0,
            upperBound: 4.0);

        Assert.True(result.Converged);
        Assert.Equal(Math.PI, result.Root, precision: 12);
    }

    [Fact]
    public void FindRoot_FindsSquareRootOfTwo()
    {
        RootFindingResult result = BrentRootFinder.FindRoot(
            x => x * x - 2.0,
            lowerBound: 1.0,
            upperBound: 2.0);

        Assert.True(result.Converged);
        Assert.Equal(Math.Sqrt(2.0), result.Root, precision: 12);
    }

    [Fact]
    public void FindRoot_FindsLogarithmRoot()
    {
        // exp(x) - 5 = 0 has root at log(5) ≈ 1.6094379124341003.
        RootFindingResult result = BrentRootFinder.FindRoot(
            x => Math.Exp(x) - 5.0,
            lowerBound: 0.0,
            upperBound: 5.0);

        Assert.True(result.Converged);
        Assert.Equal(Math.Log(5.0), result.Root, precision: 12);
    }

    [Fact]
    public void FindRoot_HandlesEndpointRoot()
    {
        // If f(lowerBound) == 0, return immediately with zero iterations.
        RootFindingResult result = BrentRootFinder.FindRoot(
            x => x - 1.0,
            lowerBound: 1.0,
            upperBound: 5.0);

        Assert.True(result.Converged);
        Assert.Equal(1.0, result.Root, precision: 14);
        Assert.Equal(0, result.IterationCount);
    }

    [Fact]
    public void FindRoot_RejectsBracketWithoutSignChange()
    {
        // f(x) = x^2 + 1 has no real root.
        Assert.Throws<ArgumentException>(() =>
            BrentRootFinder.FindRoot(x => x * x + 1.0, -10.0, 10.0));
    }

    [Fact]
    public void FindRoot_RejectsInvalidBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BrentRootFinder.FindRoot(Math.Sin, 4.0, 3.0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BrentRootFinder.FindRoot(Math.Sin, 3.0, 3.0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BrentRootFinder.FindRoot(Math.Sin, double.NaN, 3.0));
    }

    [Fact]
    public void FindRoot_RejectsBadTolerance()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BrentRootFinder.FindRoot(Math.Sin, 3.0, 4.0, absoluteTolerance: 0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BrentRootFinder.FindRoot(Math.Sin, 3.0, 4.0, absoluteTolerance: -1e-6));
    }

    [Fact]
    public void FindRoot_RejectsBadIterationCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BrentRootFinder.FindRoot(Math.Sin, 3.0, 4.0, maxIterations: 0));
    }

    [Fact]
    public void FindRoot_ThrowsWhenFunctionReturnsNaN()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BrentRootFinder.FindRoot(x => double.NaN, 0.0, 1.0));
    }

    [Fact]
    public void FindRoot_TerminatesWithinIterationBudget()
    {
        // Generous tolerance — should be fast.
        RootFindingResult result = BrentRootFinder.FindRoot(
            Math.Sin,
            lowerBound: 3.0,
            upperBound: 4.0,
            absoluteTolerance: 1e-6,
            maxIterations: 50);

        Assert.True(result.Converged);
        Assert.True(result.IterationCount < 30, $"Expected <30 iterations, got {result.IterationCount}.");
    }
}
