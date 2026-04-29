namespace DeepSigma.Mathematics.Optimization.Nonlinear.Tests;

public sealed class MmaTests
{
    [Fact]
    public void Solve_Hs76_RecoversOptimum()
    {
        // HS76 has only inequality constraints — a good fit for MMA.
        NlpConstraint c1 = new("c1", x => x[0] + 2.0 * x[1] + x[2] + x[3] - 5.0);
        NlpConstraint c2 = new("c2", x => 3.0 * x[0] + x[1] + 2.0 * x[2] - x[3] - 4.0);
        NlpConstraint c3 = new("c3", x => 1.5 - x[1] - 4.0 * x[2]);

        ConstrainedNlpResult result = Mma.Solve(
            objective: x =>
                x[0] * x[0] + 0.5 * x[1] * x[1] + x[2] * x[2] + 0.5 * x[3] * x[3]
                - x[0] * x[2] + x[2] * x[3] - x[0] - 3.0 * x[1] + x[2] - x[3],
            initialGuess: [0.5, 0.5, 0.5, 0.5],
            inequalityConstraints: [c1, c2, c3],
            lowerBounds: [0.0, 0.0, 0.0, 0.0],
            options: new ConstrainedNlpOptions { MaxIterations = 1_000, ParameterTolerance = 1e-8 });

        Assert.NotEqual(NlpTerminationStatus.Failure, result.Status);
        Assert.Equal(-4.6818, result.FinalValue, precision: 2);
    }

    [Fact]
    public void Solve_RejectsEqualityConstraints()
    {
        NlpConstraint equality = new("eq", x => x[0] - 1.0);
        ArgumentException ex = Assert.Throws<ArgumentException>(() => Mma.Solve(
            objective: x => x[0] * x[0],
            initialGuess: [0.5],
            equalityConstraints: [equality],
            lowerBounds: [0.0],
            upperBounds: [2.0]));
        Assert.Contains("equality", ex.Message);
    }
}
