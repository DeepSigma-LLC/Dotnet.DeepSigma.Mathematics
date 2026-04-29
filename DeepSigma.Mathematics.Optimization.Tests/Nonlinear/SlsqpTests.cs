namespace DeepSigma.Mathematics.Optimization.Nonlinear.Tests;

public sealed class SlsqpTests
{
    [Fact]
    public void Solve_Hs1_RosenbrockUnconstrained_RecoversOptimum()
    {
        // Hock-Schittkowski HS1: min 100*(x2 - x1^2)^2 + (1 - x1)^2 with x2 >= -1.5.
        // Optimum: (1, 1), f = 0.
        ConstrainedNlpResult result = Slsqp.Solve(
            objective: x =>
            {
                double a = 1.0 - x[0];
                double b = x[1] - x[0] * x[0];
                return a * a + 100.0 * b * b;
            },
            initialGuess: [-2.0, 1.0],
            lowerBounds: [double.NegativeInfinity, -1.5],
            options: new ConstrainedNlpOptions { MaxIterations = 500, ParameterTolerance = 1e-10 });

        Assert.True(result.Status is NlpTerminationStatus.Success or
                                     NlpTerminationStatus.FunctionToleranceReached or
                                     NlpTerminationStatus.ParameterToleranceReached,
            $"Status was {result.Status}.");
        // SLSQP with finite-difference gradients converges to ~3 decimals; analytical
        // gradients would push this further but we don't expose that path in v1.
        Assert.Equal(1.0, result.Solution[0], precision: 3);
        Assert.Equal(1.0, result.Solution[1], precision: 3);
        Assert.True(result.FinalValue < 1e-4, $"Final value {result.FinalValue:G} not near zero.");
    }

    [Fact]
    public void Solve_Hs71_EqualityAndInequalityConstraints_RecoversOptimum()
    {
        // HS71: min x1*x4*(x1+x2+x3) + x3
        //   s.t.  x1*x2*x3*x4 >= 25     (rewritten: 25 - prod <= 0)
        //         x1^2 + x2^2 + x3^2 + x4^2 = 40
        //         1 <= xi <= 5
        // Optimum: x* ≈ (1.0, 4.7430, 3.8211, 1.3794), f* ≈ 17.0140.
        NlpConstraint productInequality = new(
            "product_ge_25",
            x => 25.0 - x[0] * x[1] * x[2] * x[3]);
        NlpConstraint sumOfSquaresEquality = new(
            "sumsq_eq_40",
            x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3] - 40.0);

        ConstrainedNlpResult result = Slsqp.Solve(
            objective: x => x[0] * x[3] * (x[0] + x[1] + x[2]) + x[2],
            initialGuess: [1.0, 5.0, 5.0, 1.0],
            inequalityConstraints: [productInequality],
            equalityConstraints: [sumOfSquaresEquality],
            lowerBounds: [1.0, 1.0, 1.0, 1.0],
            upperBounds: [5.0, 5.0, 5.0, 5.0],
            options: new ConstrainedNlpOptions { MaxIterations = 500, ParameterTolerance = 1e-10 });

        Assert.True(result.Status is NlpTerminationStatus.Success or
                                     NlpTerminationStatus.FunctionToleranceReached or
                                     NlpTerminationStatus.ParameterToleranceReached,
            $"Status was {result.Status}.");
        Assert.Equal(17.0140, result.FinalValue, precision: 3);
        Assert.NotNull(result.InequalityViolations);
        Assert.NotNull(result.EqualityViolations);
        Assert.True(result.InequalityViolations![0] < 1e-4);
        Assert.True(result.EqualityViolations![0] < 1e-4);
    }

    [Fact]
    public void Solve_Hs76_QuadraticWithLinearInequalities_RecoversOptimum()
    {
        // HS76: min x1^2 + 0.5*x2^2 + x3^2 + 0.5*x4^2 - x1*x3 + x3*x4 - x1 - 3*x2 + x3 - x4
        //   s.t.  x1 + 2*x2 + x3 + x4 <= 5
        //         3*x1 + x2 + 2*x3 - x4 <= 4
        //         -x2 - 4*x3 + 1.5 <= 0   (i.e., x2 + 4*x3 >= 1.5)
        //         xi >= 0
        // Optimum: x* ≈ (0.2727, 2.0909, 0, 0.5454), f* ≈ -4.6818.
        NlpConstraint c1 = new("c1", x => x[0] + 2.0 * x[1] + x[2] + x[3] - 5.0);
        NlpConstraint c2 = new("c2", x => 3.0 * x[0] + x[1] + 2.0 * x[2] - x[3] - 4.0);
        NlpConstraint c3 = new("c3", x => 1.5 - x[1] - 4.0 * x[2]);

        ConstrainedNlpResult result = Slsqp.Solve(
            objective: x =>
                x[0] * x[0] + 0.5 * x[1] * x[1] + x[2] * x[2] + 0.5 * x[3] * x[3]
                - x[0] * x[2] + x[2] * x[3] - x[0] - 3.0 * x[1] + x[2] - x[3],
            initialGuess: [0.5, 0.5, 0.5, 0.5],
            inequalityConstraints: [c1, c2, c3],
            lowerBounds: [0.0, 0.0, 0.0, 0.0],
            options: new ConstrainedNlpOptions { MaxIterations = 500, ParameterTolerance = 1e-10 });

        Assert.True(result.Status is NlpTerminationStatus.Success or
                                     NlpTerminationStatus.FunctionToleranceReached or
                                     NlpTerminationStatus.ParameterToleranceReached);
        Assert.Equal(-4.6818, result.FinalValue, precision: 3);
        // All inequalities satisfied.
        foreach (double v in result.InequalityViolations!)
        {
            Assert.True(v < 1e-4);
        }
    }

    [Fact]
    public void Solve_BoundedQuadratic_FindsCornerOptimum()
    {
        // min (x0 - 3)^2 + (x1 - 4)^2  s.t.  0 <= xi <= 2
        // Unbounded optimum at (3, 4) — projected to corner (2, 2). f* = 1 + 4 = 5.
        ConstrainedNlpResult result = Slsqp.Solve(
            objective: x => (x[0] - 3.0) * (x[0] - 3.0) + (x[1] - 4.0) * (x[1] - 4.0),
            initialGuess: [0.5, 0.5],
            lowerBounds: [0.0, 0.0],
            upperBounds: [2.0, 2.0]);

        Assert.Equal(2.0, result.Solution[0], precision: 4);
        Assert.Equal(2.0, result.Solution[1], precision: 4);
        Assert.Equal(5.0, result.FinalValue, precision: 4);
    }
}
