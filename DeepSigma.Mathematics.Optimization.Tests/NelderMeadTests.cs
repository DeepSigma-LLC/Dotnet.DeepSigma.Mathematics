namespace DeepSigma.Mathematics.Optimization.Tests;

public sealed class NelderMeadTests
{
    [Fact]
    public void Minimize_FindsRosenbrockGlobalMinimum()
    {
        // Rosenbrock function has its global minimum at (1, 1) with f = 0. The starting point
        // (-1.2, 1.0) is the canonical hard initial guess for testing simplex methods because
        // it sits on a curved valley far from the minimum.
        MinimizationResult result = NelderMead.Minimize(
            objective: Rosenbrock,
            initialGuess: [-1.2, 1.0],
            new MinimizationOptions { MaxIterations = 2_000, AbsoluteTolerance = 1e-10 });

        Assert.True(result.Converged, $"Did not converge in {result.IterationCount} iterations.");
        Assert.Equal(1.0, result.Solution[0], precision: 4);
        Assert.Equal(1.0, result.Solution[1], precision: 4);
        Assert.True(result.FinalValue < 1e-8, $"Final value {result.FinalValue:G} not near zero.");
    }

    [Fact]
    public void Minimize_FindsSphereMinimumQuickly()
    {
        // Sphere is f(x) = sum x_i^2. Trivial; convergence should be fast.
        MinimizationResult result = NelderMead.Minimize(
            objective: parameters =>
            {
                double sum = 0.0;
                foreach (double x in parameters) sum += x * x;
                return sum;
            },
            initialGuess: [3.0, -2.0, 5.0, -1.0],
            new MinimizationOptions { MaxIterations = 1_000, AbsoluteTolerance = 1e-12 });

        Assert.True(result.Converged);
        for (int i = 0; i < result.Solution.Length; i++)
        {
            Assert.Equal(0.0, result.Solution[i], precision: 5);
        }
        Assert.True(result.FinalValue < 1e-10);
    }

    [Fact]
    public void Minimize_FindsBealeMinimum()
    {
        // Beale's function: f(x, y) = (1.5 - x + xy)^2 + (2.25 - x + xy^2)^2 + (2.625 - x + xy^3)^2
        // Global min at (3, 0.5) with f = 0.
        MinimizationResult result = NelderMead.Minimize(
            objective: parameters =>
            {
                double x = parameters[0];
                double y = parameters[1];
                double t1 = 1.5 - x + x * y;
                double t2 = 2.25 - x + x * y * y;
                double t3 = 2.625 - x + x * y * y * y;
                return t1 * t1 + t2 * t2 + t3 * t3;
            },
            initialGuess: [1.0, 1.0],
            new MinimizationOptions { MaxIterations = 2_000, AbsoluteTolerance = 1e-12 });

        Assert.True(result.Converged);
        Assert.Equal(3.0, result.Solution[0], precision: 4);
        Assert.Equal(0.5, result.Solution[1], precision: 4);
    }

    [Fact]
    public void Minimize_TerminatesAtMaxIterationsWhenToleranceTooTight()
    {
        // Force a budget-exhausted run by setting an absurd tolerance.
        MinimizationResult result = NelderMead.Minimize(
            objective: Rosenbrock,
            initialGuess: [-1.2, 1.0],
            new MinimizationOptions { MaxIterations = 50, AbsoluteTolerance = 1e-30 });

        Assert.False(result.Converged);
        Assert.True(result.IterationCount >= 50);
    }

    [Fact]
    public void Minimize_ValidatesInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            NelderMead.Minimize(null!, [0.0, 0.0]));

        Assert.Throws<ArgumentException>(() =>
            NelderMead.Minimize(p => 0.0, ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Minimize_RejectsNonFiniteInitialGuess()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NelderMead.Minimize(p => 0.0, [double.NaN, 1.0]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NelderMead.Minimize(p => 0.0, [1.0, double.PositiveInfinity]));
    }

    [Fact]
    public void Minimize_ThrowsWhenObjectiveReturnsNaN()
    {
        Assert.Throws<InvalidOperationException>(() =>
            NelderMead.Minimize(p => double.NaN, [0.0, 0.0]));
    }

    [Fact]
    public void Options_RejectsInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NelderMead.Minimize(
            p => 0.0,
            [0.0],
            new MinimizationOptions { MaxIterations = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => NelderMead.Minimize(
            p => 0.0,
            [0.0],
            new MinimizationOptions { AbsoluteTolerance = -1e-6 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => NelderMead.Minimize(
            p => 0.0,
            [0.0],
            new MinimizationOptions { InitialSimplexEdge = 0.0 }));
    }

    private static double Rosenbrock(ReadOnlySpan<double> parameters)
    {
        double a = 1.0 - parameters[0];
        double b = parameters[1] - parameters[0] * parameters[0];
        return a * a + 100.0 * b * b;
    }
}
