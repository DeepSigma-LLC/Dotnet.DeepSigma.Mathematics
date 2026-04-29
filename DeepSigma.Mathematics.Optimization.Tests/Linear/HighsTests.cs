namespace DeepSigma.Mathematics.Optimization.Linear.Tests;

public sealed class HighsTests
{
    [Fact]
    public void Solve_DietProblem_FindsKnownOptimum()
    {
        // Same diet problem as GlopTests; HiGHS should land on the same optimum.
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

        LinearResult result = Highs.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(3.6, result.VariableValues["bread"], precision: 4);
        Assert.Equal(0.8, result.VariableValues["milk"], precision: 4);
        Assert.Equal(2.04, result.ObjectiveValue, precision: 4);
    }

    [Fact]
    public void Solve_RejectsIntegerVariables()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0, VariableKind.Binary)],
            Constraints: []);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Highs.Solve(program));
        Assert.Contains("Highs", ex.Message);
    }
}
