namespace DeepSigma.Mathematics.Optimization.Nonlinear.Tests;

public sealed class IsresTests
{
    [Fact]
    public void Solve_BoundedRastrigin_FindsGlobalOptimum()
    {
        // 2D Rastrigin: many local minima, global at origin where f = 0.
        ConstrainedNlpResult result = Isres.Solve(
            objective: x =>
            {
                double s = 10.0 * x.Length;
                foreach (double xi in x) s += xi * xi - 10.0 * Math.Cos(2.0 * Math.PI * xi);
                return s;
            },
            initialGuess: [3.0, -2.5],
            lowerBounds: [-5.12, -5.12],
            upperBounds: [5.12, 5.12],
            options: new ConstrainedNlpOptions { MaxIterations = 20_000, ParameterTolerance = 1e-8 });

        // ISRES is stochastic and the NLoptNet binding doesn't expose seeding, so don't pin
        // exact convergence. Demand the global region (within ~0.5 of zero, f below the local
        // minima floor of ~5).
        Assert.NotEqual(NlpTerminationStatus.Failure, result.Status);
        Assert.True(result.FinalValue < 5.0, $"Final value {result.FinalValue:G} is no better than a local minimum.");
        Assert.True(Math.Abs(result.Solution[0]) < 0.6, $"x0 = {result.Solution[0]:G} not near zero.");
        Assert.True(Math.Abs(result.Solution[1]) < 0.6, $"x1 = {result.Solution[1]:G} not near zero.");
    }

    [Fact]
    public void Solve_RejectsUnboundedSearch()
    {
        Assert.Throws<ArgumentException>(() => Isres.Solve(
            objective: x => x[0] * x[0],
            initialGuess: [0.5]));
    }
}
