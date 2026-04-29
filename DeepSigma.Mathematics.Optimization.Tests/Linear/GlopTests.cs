namespace DeepSigma.Mathematics.Optimization.Linear.Tests;

public sealed class GlopTests
{
    [Fact]
    public void Solve_DietProblem_FindsKnownOptimum()
    {
        // Stigler's diet (toy variant): minimize cost of bread/milk subject to nutrient floors.
        //   minimize  0.50 bread + 0.30 milk
        //   s.t.   2 bread + 1 milk >= 8   (calories)
        //          1 bread + 3 milk >= 6   (vitamins)
        //          bread, milk >= 0
        // Optimum: bread = 3.6, milk = 0.8, cost = 2.04.
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

        LinearResult result = Glop.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(3.6, result.VariableValues["bread"], precision: 4);
        Assert.Equal(0.8, result.VariableValues["milk"], precision: 4);
        Assert.Equal(2.04, result.ObjectiveValue, precision: 4);
    }

    [Fact]
    public void Solve_TransportationProblem_FindsKnownOptimum()
    {
        // Two factories (F1, F2) supplying two warehouses (W1, W2).
        //   Supply: F1=20, F2=30. Demand: W1=25, W2=25.
        //   Costs: F1->W1=4, F1->W2=6, F2->W1=5, F2->W2=3.
        // Optimum: x_F1W1=20, x_F1W2=0, x_F2W1=5, x_F2W2=25, total cost = 4*20 + 5*5 + 3*25 = 180.
        LinearProgram program = new(
            Objective: new LinearObjective(
                Terms: [
                    new LinearTerm("x_F1W1", 4.0),
                    new LinearTerm("x_F1W2", 6.0),
                    new LinearTerm("x_F2W1", 5.0),
                    new LinearTerm("x_F2W2", 3.0),
                ],
                Sense: ObjectiveSense.Minimize),
            Variables: [
                new LinearVariable("x_F1W1", 0.0, double.PositiveInfinity),
                new LinearVariable("x_F1W2", 0.0, double.PositiveInfinity),
                new LinearVariable("x_F2W1", 0.0, double.PositiveInfinity),
                new LinearVariable("x_F2W2", 0.0, double.PositiveInfinity),
            ],
            Constraints: [
                new LinearConstraint("supply_F1", [
                    new LinearTerm("x_F1W1", 1.0),
                    new LinearTerm("x_F1W2", 1.0),
                ], ComparisonKind.LessOrEqual, 20.0),
                new LinearConstraint("supply_F2", [
                    new LinearTerm("x_F2W1", 1.0),
                    new LinearTerm("x_F2W2", 1.0),
                ], ComparisonKind.LessOrEqual, 30.0),
                new LinearConstraint("demand_W1", [
                    new LinearTerm("x_F1W1", 1.0),
                    new LinearTerm("x_F2W1", 1.0),
                ], ComparisonKind.Equal, 25.0),
                new LinearConstraint("demand_W2", [
                    new LinearTerm("x_F1W2", 1.0),
                    new LinearTerm("x_F2W2", 1.0),
                ], ComparisonKind.Equal, 25.0),
            ]);

        LinearResult result = Glop.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(180.0, result.ObjectiveValue, precision: 4);
        Assert.Equal(20.0, result.VariableValues["x_F1W1"], precision: 4);
        Assert.Equal(0.0, result.VariableValues["x_F1W2"], precision: 4);
        Assert.Equal(5.0, result.VariableValues["x_F2W1"], precision: 4);
        Assert.Equal(25.0, result.VariableValues["x_F2W2"], precision: 4);
    }

    [Fact]
    public void Solve_DetectsInfeasibility()
    {
        // x >= 5 and x <= 3 — no feasible point.
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, double.PositiveInfinity)],
            Constraints: [
                new LinearConstraint("c1", [new LinearTerm("x", 1.0)], ComparisonKind.GreaterOrEqual, 5.0),
                new LinearConstraint("c2", [new LinearTerm("x", 1.0)], ComparisonKind.LessOrEqual, 3.0),
            ]);

        LinearResult result = Glop.Solve(program);

        Assert.Equal(LinearTerminationStatus.Infeasible, result.Status);
    }

    [Fact]
    public void Solve_DetectsNonOptimalForUnboundedProblem()
    {
        // Maximize x with no upper bound and no constraints. Different solvers report this
        // as Unbounded, Infeasible, or Error depending on internal conventions — the contract
        // we care about is "not Optimal" (no fictitious finite answer is returned).
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Maximize),
            Variables: [new LinearVariable("x", 0.0, double.PositiveInfinity)],
            Constraints: []);

        LinearResult result = Glop.Solve(program);

        Assert.NotEqual(LinearTerminationStatus.Optimal, result.Status);
        Assert.NotEqual(LinearTerminationStatus.Feasible, result.Status);
    }

    [Fact]
    public void Solve_RespectsObjectiveConstant()
    {
        // Minimize x + 100 with x in [0, 10]. Optimum x = 0, objective = 100.
        LinearProgram program = new(
            Objective: new LinearObjective(
                Terms: [new LinearTerm("x", 1.0)],
                Sense: ObjectiveSense.Minimize,
                Constant: 100.0),
            Variables: [new LinearVariable("x", 0.0, 10.0)],
            Constraints: []);

        LinearResult result = Glop.Solve(program);

        Assert.Equal(LinearTerminationStatus.Optimal, result.Status);
        Assert.Equal(0.0, result.VariableValues["x"], precision: 6);
        Assert.Equal(100.0, result.ObjectiveValue, precision: 6);
    }

    [Fact]
    public void Solve_RejectsIntegerVariables()
    {
        // Glop is LP-only; should reject MIP problems with a clear error.
        LinearProgram program = new(
            Objective: new LinearObjective([new LinearTerm("x", 1.0)], ObjectiveSense.Minimize),
            Variables: [new LinearVariable("x", 0.0, 10.0, VariableKind.Integer)],
            Constraints: []);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => Glop.Solve(program));
        Assert.Contains("Glop", ex.Message);
        Assert.Contains("LP-only", ex.Message);
    }
}
