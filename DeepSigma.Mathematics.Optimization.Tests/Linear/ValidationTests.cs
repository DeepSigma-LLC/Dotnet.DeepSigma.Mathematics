namespace DeepSigma.Mathematics.Optimization.Linear.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void Solve_RejectsNullProgram()
    {
        Assert.Throws<ArgumentNullException>(() => Glop.Solve(null!));
    }

    [Fact]
    public void Solve_RejectsEmptyVariableList()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([], ObjectiveSense.Minimize),
            Variables: [],
            Constraints: []);

        Assert.Throws<ArgumentException>(() => Glop.Solve(program));
    }

    [Fact]
    public void Solve_RejectsDuplicateVariableNames()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [
                new LinearVariable("x", 0.0, 10.0),
                new LinearVariable("x", 0.0, 10.0),
            ],
            Constraints: []);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Glop.Solve(program));
        Assert.Contains("Duplicate variable name", ex.Message);
    }

    [Fact]
    public void Solve_RejectsDuplicateConstraintNames()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0)],
            Constraints: [
                new LinearConstraint("c", [new LinearTerm("x", 1.0)], ComparisonKind.LessOrEqual, 5.0),
                new LinearConstraint("c", [new LinearTerm("x", 1.0)], ComparisonKind.LessOrEqual, 6.0),
            ]);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Glop.Solve(program));
        Assert.Contains("Duplicate constraint name", ex.Message);
    }

    [Fact]
    public void Solve_RejectsConstraintReferencingUnknownVariable()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0)],
            Constraints: [
                new LinearConstraint("c", [new LinearTerm("y", 1.0)], ComparisonKind.LessOrEqual, 5.0),
            ]);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Glop.Solve(program));
        Assert.Contains("unknown variable", ex.Message);
    }

    [Fact]
    public void Solve_RejectsObjectiveReferencingUnknownVariable()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("y", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0)],
            Constraints: []);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Glop.Solve(program));
        Assert.Contains("unknown variable", ex.Message);
    }

    [Fact]
    public void Solve_RejectsLowerGreaterThanUpper()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 10.0, 5.0)],
            Constraints: []);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Glop.Solve(program));
        Assert.Contains("Lower=", ex.Message);
    }

    [Fact]
    public void Solve_RejectsNonFiniteCoefficient()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", double.NaN)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0)],
            Constraints: []);

        Assert.Throws<ArgumentException>(() => Glop.Solve(program));
    }

    [Fact]
    public void Solve_RejectsEmptyVariableName()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("", 0.0, 10.0)],
            Constraints: []);

        Assert.Throws<ArgumentException>(() => Glop.Solve(program));
    }

    [Fact]
    public void Solve_RejectsNegativeTimeLimit()
    {
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0)],
            Constraints: []);

        Assert.Throws<ArgumentOutOfRangeException>(() => Glop.Solve(
            program,
            new LinearProgrammingOptions { TimeLimit = TimeSpan.FromSeconds(-1) }));
    }
}
