namespace DeepSigma.Mathematics.Optimization.Nonlinear.Tests;

public sealed class AugLagTests
{
    [Theory]
    [InlineData(AugLagInnerSolver.LBfgs)]
    [InlineData(AugLagInnerSolver.Slsqp)]
    [InlineData(AugLagInnerSolver.Cobyla)]
    public void Solve_Hs71_RecoversOptimum_AcrossInnerSolvers(AugLagInnerSolver innerSolver)
    {
        // Same HS71 problem as in SlsqpTests, run through Augmented Lagrangian with each
        // inner-solver option. Tolerance is looser than SLSQP's because AL is less precise
        // and convergence depends on the inner solver.
        NlpConstraint productInequality = new(
            "product_ge_25",
            x => 25.0 - x[0] * x[1] * x[2] * x[3]);
        NlpConstraint sumOfSquaresEquality = new(
            "sumsq_eq_40",
            x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3] - 40.0);

        ConstrainedNlpResult result = AugLag.Solve(
            objective: x => x[0] * x[3] * (x[0] + x[1] + x[2]) + x[2],
            initialGuess: [1.0, 5.0, 5.0, 1.0],
            inequalityConstraints: [productInequality],
            equalityConstraints: [sumOfSquaresEquality],
            lowerBounds: [1.0, 1.0, 1.0, 1.0],
            upperBounds: [5.0, 5.0, 5.0, 5.0],
            options: new ConstrainedNlpOptions { MaxIterations = 2_000, ParameterTolerance = 1e-8 },
            innerSolver: innerSolver);

        // AL is robust but not as precise as SLSQP — accept any non-failure status that produced
        // a near-optimal solution.
        Assert.NotEqual(NlpTerminationStatus.Failure, result.Status);
        Assert.NotEqual(NlpTerminationStatus.InfeasibleConstraint, result.Status);
        Assert.Equal(17.0140, result.FinalValue, precision: 1);
    }
}
