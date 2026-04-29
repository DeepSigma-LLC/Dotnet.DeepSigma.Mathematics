namespace DeepSigma.Mathematics.Optimization.Nonlinear.Tests;

public sealed class CobylaTests
{
    [Fact]
    public void Solve_Hs71_DerivativeFreePath_RecoversOptimum()
    {
        // HS71 via COBYLA — derivative-free, so we expect more evaluations than SLSQP needs.
        NlpConstraint productInequality = new(
            "product_ge_25",
            x => 25.0 - x[0] * x[1] * x[2] * x[3]);
        NlpConstraint sumOfSquaresEquality = new(
            "sumsq_eq_40",
            x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3] - 40.0);

        ConstrainedNlpResult result = Cobyla.Solve(
            objective: x => x[0] * x[3] * (x[0] + x[1] + x[2]) + x[2],
            initialGuess: [1.0, 5.0, 5.0, 1.0],
            inequalityConstraints: [productInequality],
            equalityConstraints: [sumOfSquaresEquality],
            lowerBounds: [1.0, 1.0, 1.0, 1.0],
            upperBounds: [5.0, 5.0, 5.0, 5.0],
            options: new ConstrainedNlpOptions { MaxIterations = 5_000, ParameterTolerance = 1e-8 });

        Assert.NotEqual(NlpTerminationStatus.Failure, result.Status);
        Assert.Equal(17.0140, result.FinalValue, precision: 1);
    }

    [Fact]
    public void Solve_BoundedQuadratic_FindsCornerOptimum()
    {
        ConstrainedNlpResult result = Cobyla.Solve(
            objective: x => (x[0] - 3.0) * (x[0] - 3.0) + (x[1] - 4.0) * (x[1] - 4.0),
            initialGuess: [0.5, 0.5],
            lowerBounds: [0.0, 0.0],
            upperBounds: [2.0, 2.0],
            options: new ConstrainedNlpOptions { MaxIterations = 500 });

        Assert.Equal(2.0, result.Solution[0], precision: 3);
        Assert.Equal(2.0, result.Solution[1], precision: 3);
        Assert.Equal(5.0, result.FinalValue, precision: 3);
    }
}
