namespace DeepSigma.Mathematics.Optimization.Linear.Tests;

public sealed class PdlpTests
{
    [Fact]
    public void Solve_SmallLp_FindsOptimum()
    {
        // Same toy max-LP from the probe: max 3x + 4y s.t. x + 2y <= 14, 3x - y >= 0, x - y <= 2.
        // Optimum at x = 6, y = 4, objective = 34. PDLP is first-order so we use a slightly
        // looser tolerance than GlopTests.
        LinearProgram program = new(
            Objective: new LinearObjective(
                Terms: [
                    new LinearTerm("x", 3.0),
                    new LinearTerm("y", 4.0),
                ],
                Sense: ObjectiveSense.Maximize),
            Variables: [
                new LinearVariable("x", 0.0, double.PositiveInfinity),
                new LinearVariable("y", 0.0, double.PositiveInfinity),
            ],
            Constraints: [
                new LinearConstraint("c1", [
                    new LinearTerm("x", 1.0),
                    new LinearTerm("y", 2.0),
                ], ComparisonKind.LessOrEqual, 14.0),
                new LinearConstraint("c2", [
                    new LinearTerm("x", 3.0),
                    new LinearTerm("y", -1.0),
                ], ComparisonKind.GreaterOrEqual, 0.0),
                new LinearConstraint("c3", [
                    new LinearTerm("x", 1.0),
                    new LinearTerm("y", -1.0),
                ], ComparisonKind.LessOrEqual, 2.0),
            ]);

        LinearResult result = Pdlp.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(6.0, result.VariableValues["x"], precision: 2);
        Assert.Equal(4.0, result.VariableValues["y"], precision: 2);
        Assert.Equal(34.0, result.ObjectiveValue, precision: 2);
    }

    [Fact]
    public void Solve_RejectsIntegerVariables()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0, VariableKind.Integer)],
            Constraints: []);

        Assert.Throws<ArgumentException>(() => Pdlp.Solve(program));
    }
}
