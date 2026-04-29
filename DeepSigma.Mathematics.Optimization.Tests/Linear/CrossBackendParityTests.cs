namespace DeepSigma.Mathematics.Optimization.Linear.Tests;

public sealed class CrossBackendParityTests
{
    [Fact]
    public void DietProblem_GivesSameOptimumAcrossBackends()
    {
        // Same diet problem solved through Glop, Highs, and Pdlp must agree on the optimum.
        // PDLP's first-order convergence means we use a slightly looser tolerance for it.
        LinearProgram program = new(
            Objective: new LinearObjective(
                Terms: [
                    new LinearTerm("bread", 0.50),
                    new LinearTerm("milk", 0.30),
                ],
                Sense: ObjectiveSense.Minimize),
            Variables: [
                new LinearVariable("bread", 0.0, double.PositiveInfinity),
                new LinearVariable("milk", 0.0, double.PositiveInfinity),
            ],
            Constraints: [
                new LinearConstraint("calories", [
                    new LinearTerm("bread", 2.0),
                    new LinearTerm("milk", 1.0),
                ], ComparisonKind.GreaterOrEqual, 8.0),
                new LinearConstraint("vitamins", [
                    new LinearTerm("bread", 1.0),
                    new LinearTerm("milk", 3.0),
                ], ComparisonKind.GreaterOrEqual, 6.0),
            ]);

        LinearResult glop = Glop.Solve(program);
        LinearResult highs = Highs.Solve(program);
        LinearResult pdlp = Pdlp.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, glop.Status);
        Assert.Equal(LinearTerminationStatus.Optimal, highs.Status);
        Assert.Equal(LinearTerminationStatus.Optimal, pdlp.Status);

        // Simplex methods should agree to high precision.
        Assert.Equal(glop.ObjectiveValue, highs.ObjectiveValue, precision: 6);
        Assert.Equal(glop.VariableValues["bread"], highs.VariableValues["bread"], precision: 6);
        Assert.Equal(glop.VariableValues["milk"], highs.VariableValues["milk"], precision: 6);

        // PDLP is first-order; allow looser tolerance on the parity check.
        Assert.Equal(glop.ObjectiveValue, pdlp.ObjectiveValue, precision: 2);
    }
}
