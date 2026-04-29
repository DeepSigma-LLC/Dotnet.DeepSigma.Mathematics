using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Shared translation logic between <see cref="LinearProgram"/> and Google OR-Tools'
/// <see cref="Solver"/>. All four backend wrappers funnel through here so problem
/// validation, variable / constraint construction, and result mapping stay consistent.
/// </summary>
internal static class OrToolsTranslator
{
    /// <summary>
    /// Builds and configures an OR-Tools <see cref="Solver"/> from a <see cref="LinearProgram"/>.
    /// Throws on malformed input; returns the populated solver plus the variable lookup map
    /// for later result extraction.
    /// </summary>
    public static (Solver Solver, Dictionary<string, Variable> Variables) Build(
        string solverId,
        LinearProgram program,
        LinearProgrammingOptions? options)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(program.Objective);
        ArgumentNullException.ThrowIfNull(program.Variables);
        ArgumentNullException.ThrowIfNull(program.Constraints);

        if (program.Variables.Count == 0)
        {
            throw new ArgumentException("LinearProgram must declare at least one variable.", nameof(program));
        }

        Solver? solver = Solver.CreateSolver(solverId);
        if (solver is null)
        {
            throw new InvalidOperationException(
                $"OR-Tools failed to create a solver with id '{solverId}'. The native backend may not be available in this distribution.");
        }

        if (options is { VerboseLogging: true })
        {
            solver.EnableOutput();
        }
        else
        {
            solver.SuppressOutput();
        }

        if (options?.TimeLimit is TimeSpan timeLimit)
        {
            if (timeLimit <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(options), timeLimit, "TimeLimit must be positive.");
            }
            solver.SetTimeLimit((long)timeLimit.TotalMilliseconds);
        }

        Dictionary<string, Variable> variables = new(program.Variables.Count, StringComparer.Ordinal);
        foreach (LinearVariable spec in program.Variables)
        {
            if (string.IsNullOrWhiteSpace(spec.Name))
            {
                throw new ArgumentException("LinearVariable.Name must be non-empty.", nameof(program));
            }
            if (variables.ContainsKey(spec.Name))
            {
                throw new ArgumentException($"Duplicate variable name '{spec.Name}'.", nameof(program));
            }
            if (spec.Lower > spec.Upper)
            {
                throw new ArgumentException(
                    $"Variable '{spec.Name}' has Lower={spec.Lower} > Upper={spec.Upper}.",
                    nameof(program));
            }
            if (double.IsNaN(spec.Lower) || double.IsNaN(spec.Upper))
            {
                throw new ArgumentException(
                    $"Variable '{spec.Name}' has NaN bounds.",
                    nameof(program));
            }

            Variable variable = spec.Kind switch
            {
                VariableKind.Continuous => solver.MakeNumVar(spec.Lower, spec.Upper, spec.Name),
                VariableKind.Integer => solver.MakeIntVar(spec.Lower, spec.Upper, spec.Name),
                VariableKind.Binary => solver.MakeIntVar(0.0, 1.0, spec.Name),
                _ => throw new ArgumentOutOfRangeException(nameof(program), spec.Kind, "Unknown VariableKind."),
            };
            variables[spec.Name] = variable;
        }

        HashSet<string> constraintNames = new(StringComparer.Ordinal);
        foreach (LinearConstraint specConstraint in program.Constraints)
        {
            if (string.IsNullOrWhiteSpace(specConstraint.Name))
            {
                throw new ArgumentException("LinearConstraint.Name must be non-empty.", nameof(program));
            }
            if (!constraintNames.Add(specConstraint.Name))
            {
                throw new ArgumentException(
                    $"Duplicate constraint name '{specConstraint.Name}'.",
                    nameof(program));
            }
            if (double.IsNaN(specConstraint.Bound))
            {
                throw new ArgumentException(
                    $"Constraint '{specConstraint.Name}' has NaN bound.",
                    nameof(program));
            }

            (double lb, double ub) = specConstraint.Kind switch
            {
                ComparisonKind.LessOrEqual => (double.NegativeInfinity, specConstraint.Bound),
                ComparisonKind.GreaterOrEqual => (specConstraint.Bound, double.PositiveInfinity),
                ComparisonKind.Equal => (specConstraint.Bound, specConstraint.Bound),
                _ => throw new ArgumentOutOfRangeException(nameof(program), specConstraint.Kind, "Unknown ComparisonKind."),
            };

            Constraint constraint = solver.MakeConstraint(lb, ub, specConstraint.Name);
            foreach (LinearTerm term in specConstraint.Terms)
            {
                if (!variables.TryGetValue(term.VariableName, out Variable? variable))
                {
                    throw new ArgumentException(
                        $"Constraint '{specConstraint.Name}' references unknown variable '{term.VariableName}'.",
                        nameof(program));
                }
                if (!double.IsFinite(term.Coefficient))
                {
                    throw new ArgumentException(
                        $"Constraint '{specConstraint.Name}' has non-finite coefficient for variable '{term.VariableName}'.",
                        nameof(program));
                }
                constraint.SetCoefficient(variable, term.Coefficient);
            }
        }

        Objective objective = solver.Objective();
        foreach (LinearTerm term in program.Objective.Terms)
        {
            if (!variables.TryGetValue(term.VariableName, out Variable? variable))
            {
                throw new ArgumentException(
                    $"Objective references unknown variable '{term.VariableName}'.",
                    nameof(program));
            }
            if (!double.IsFinite(term.Coefficient))
            {
                throw new ArgumentException(
                    $"Objective has non-finite coefficient for variable '{term.VariableName}'.",
                    nameof(program));
            }
            objective.SetCoefficient(variable, term.Coefficient);
        }
        if (!double.IsFinite(program.Objective.Constant))
        {
            throw new ArgumentException("Objective.Constant must be finite.", nameof(program));
        }
        objective.SetOffset(program.Objective.Constant);

        switch (program.Objective.Sense)
        {
            case ObjectiveSense.Minimize:
                objective.SetMinimization();
                break;
            case ObjectiveSense.Maximize:
                objective.SetMaximization();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(program), program.Objective.Sense, "Unknown ObjectiveSense.");
        }

        return (solver, variables);
    }

    /// <summary>
    /// Runs the configured solver and packages its output into a <see cref="LinearResult"/>.
    /// </summary>
    public static LinearResult Solve(Solver solver, IReadOnlyDictionary<string, Variable> variables)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Solver.ResultStatus rawStatus = solver.Solve();
        stopwatch.Stop();

        LinearTerminationStatus status = MapStatus(rawStatus);

        Dictionary<string, double> values;
        double objectiveValue;
        if (status is LinearTerminationStatus.Optimal or LinearTerminationStatus.Feasible)
        {
            values = new Dictionary<string, double>(variables.Count, StringComparer.Ordinal);
            foreach (KeyValuePair<string, Variable> entry in variables)
            {
                values[entry.Key] = entry.Value.SolutionValue();
            }
            objectiveValue = solver.Objective().Value();
        }
        else
        {
            values = new Dictionary<string, double>(StringComparer.Ordinal);
            objectiveValue = double.NaN;
        }

        return new LinearResult(
            VariableValues: values,
            ObjectiveValue: objectiveValue,
            Status: status,
            IterationCount: (int)Math.Min(solver.Iterations(), int.MaxValue),
            SolveTime: stopwatch.Elapsed);
    }

    private static LinearTerminationStatus MapStatus(Solver.ResultStatus rawStatus) => rawStatus switch
    {
        Solver.ResultStatus.OPTIMAL => LinearTerminationStatus.Optimal,
        Solver.ResultStatus.FEASIBLE => LinearTerminationStatus.Feasible,
        Solver.ResultStatus.INFEASIBLE => LinearTerminationStatus.Infeasible,
        Solver.ResultStatus.UNBOUNDED => LinearTerminationStatus.Unbounded,
        Solver.ResultStatus.ABNORMAL => LinearTerminationStatus.Error,
        Solver.ResultStatus.NOT_SOLVED => LinearTerminationStatus.Error,
        _ => LinearTerminationStatus.Error,
    };
}
