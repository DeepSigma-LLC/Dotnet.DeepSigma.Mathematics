namespace DeepSigma.Mathematics.Optimization.Linear.Tests;

public sealed class CbcMipTests
{
    [Fact]
    public void Solve_ZeroOneKnapsack_FindsOptimum()
    {
        // Classical 0-1 knapsack: 5 items, capacity 10.
        //   Items   weight  value
        //     1       2       3
        //     2       3       4
        //     3       4       5
        //     4       5       6
        //     5       6       7
        // Optimum: take items {1, 2, 5} (weight 11)? No — weight 2+3+6 = 11, exceeds 10.
        // Try {2, 3, 4} weight 12 — also exceeds. Try {1, 2, 4} weight 10, value 13. Try {1, 3, 4}
        // weight 11. So {1, 2, 4} weight 10 value 13 is feasible. Try {2, 4} weight 8 value 10.
        // Try {1, 2, 5} weight 11 — infeasible. Try {1, 4, 5} weight 13 — infeasible.
        // Try {3, 4} weight 9 value 11. {2, 5} weight 9 value 11. {1, 2, 4} weight 10 value 13 wins.
        LinearProgram program = new(
            Objective: new LinearObjective(
                Terms: [
                    new LinearTerm("x1", 3.0),
                    new LinearTerm("x2", 4.0),
                    new LinearTerm("x3", 5.0),
                    new LinearTerm("x4", 6.0),
                    new LinearTerm("x5", 7.0),
                ],
                Sense: ObjectiveSense.Maximize),
            Variables: [
                new LinearVariable("x1", 0.0, 1.0, VariableKind.Binary),
                new LinearVariable("x2", 0.0, 1.0, VariableKind.Binary),
                new LinearVariable("x3", 0.0, 1.0, VariableKind.Binary),
                new LinearVariable("x4", 0.0, 1.0, VariableKind.Binary),
                new LinearVariable("x5", 0.0, 1.0, VariableKind.Binary),
            ],
            Constraints: [
                new LinearConstraint("capacity", [
                    new LinearTerm("x1", 2.0),
                    new LinearTerm("x2", 3.0),
                    new LinearTerm("x3", 4.0),
                    new LinearTerm("x4", 5.0),
                    new LinearTerm("x5", 6.0),
                ], ComparisonKind.LessOrEqual, 10.0),
            ]);

        LinearResult result = Cbc.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(13.0, result.ObjectiveValue, precision: 4);
        Assert.Equal(1.0, result.VariableValues["x1"], precision: 4);
        Assert.Equal(1.0, result.VariableValues["x2"], precision: 4);
        Assert.Equal(0.0, result.VariableValues["x3"], precision: 4);
        Assert.Equal(1.0, result.VariableValues["x4"], precision: 4);
        Assert.Equal(0.0, result.VariableValues["x5"], precision: 4);
    }

    [Fact]
    public void Solve_HandlesIntegerVariablesWithMixedBounds()
    {
        // Maximize 2x + 3y subject to x + y <= 5, x, y in {0, 1, 2, 3}.
        // Continuous LP relaxation: x = 0, y = 5 (but y capped at 3) => x = 2, y = 3, obj = 13.
        LinearProgram program = new(
            Objective: new LinearObjective(
                Terms: [
                    new LinearTerm("x", 2.0),
                    new LinearTerm("y", 3.0),
                ],
                Sense: ObjectiveSense.Maximize),
            Variables: [
                new LinearVariable("x", 0.0, 3.0, VariableKind.Integer),
                new LinearVariable("y", 0.0, 3.0, VariableKind.Integer),
            ],
            Constraints: [
                new LinearConstraint("c", [
                    new LinearTerm("x", 1.0),
                    new LinearTerm("y", 1.0),
                ], ComparisonKind.LessOrEqual, 5.0),
            ]);

        LinearResult result = Cbc.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(13.0, result.ObjectiveValue, precision: 4);
        Assert.Equal(2.0, result.VariableValues["x"], precision: 4);
        Assert.Equal(3.0, result.VariableValues["y"], precision: 4);
    }
}
