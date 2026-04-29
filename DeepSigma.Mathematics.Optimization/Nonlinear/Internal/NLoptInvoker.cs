using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Shared NLopt setup and invocation logic. Each algorithm wrapper (Slsqp, AugLag, Cobyla,
/// Mma, Isres) funnels through here so input validation, bounds / constraint translation,
/// option mapping, and result packaging stay consistent across algorithms.
/// </summary>
internal static class NLoptInvoker
{
    public static ConstrainedNlpResult Invoke(
        NLoptAlgorithm algorithm,
        string algorithmName,
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> initialGuess,
        IReadOnlyList<NlpConstraint>? inequalityConstraints,
        IReadOnlyList<NlpConstraint>? equalityConstraints,
        ReadOnlySpan<double> lowerBounds,
        ReadOnlySpan<double> upperBounds,
        ConstrainedNlpOptions? options,
        NLoptAlgorithm? childAlgorithm = null)
    {
        ArgumentNullException.ThrowIfNull(objective);
        Guard.NotEmpty(initialGuess, nameof(initialGuess), "Initial guess must have at least one parameter.");
        Guard.AllFinite(initialGuess, nameof(initialGuess));

        ConstrainedNlpOptions opts = options ?? new ConstrainedNlpOptions();
        opts.Validate();

        int n = initialGuess.Length;

        if (lowerBounds.Length != 0 && lowerBounds.Length != n)
        {
            throw new ArgumentException(
                $"Lower bounds length ({lowerBounds.Length}) must match initial guess length ({n}).",
                nameof(lowerBounds));
        }
        if (upperBounds.Length != 0 && upperBounds.Length != n)
        {
            throw new ArgumentException(
                $"Upper bounds length ({upperBounds.Length}) must match initial guess length ({n}).",
                nameof(upperBounds));
        }

        ValidateConstraintList(inequalityConstraints, "inequalityConstraints");
        ValidateConstraintList(equalityConstraints, "equalityConstraints");

        // Snapshot bounds into local arrays so they survive past the span scope.
        double[]? lower = lowerBounds.Length == n ? lowerBounds.ToArray() : null;
        double[]? upper = upperBounds.Length == n ? upperBounds.ToArray() : null;
        if (lower is not null && upper is not null)
        {
            for (int i = 0; i < n; i++)
            {
                if (lower[i] > upper[i])
                {
                    throw new ArgumentException(
                        $"Lower bound {lower[i]:G} at index {i} is greater than upper bound {upper[i]:G}.",
                        nameof(lowerBounds));
                }
            }
        }

        using NLoptSolver solver = new(
            algorithm: algorithm,
            numVariables: (uint)n,
            relativeStoppingTolerance: opts.ParameterTolerance,
            maximumIterations: opts.MaxIterations,
            childAlgorithm: childAlgorithm);

        if (lower is not null) solver.SetLowerBounds(lower);
        if (upper is not null) solver.SetUpperBounds(upper);

        // Track evaluation counts ourselves — NLoptNet does not expose a counter.
        // We count only the user-facing objective evaluations (not the extra calls made for
        // finite-difference gradients), to keep IterationCount semantics aligned with the
        // other optimizers in this package.
        int evaluations = 0;

        solver.SetMinObjective(WrapWithFiniteDifferenceGradient(
            objective,
            $"Objective for {algorithmName}",
            countCall: () => evaluations++));

        if (inequalityConstraints is not null)
        {
            foreach (NlpConstraint constraint in inequalityConstraints)
            {
                NlpConstraint captured = constraint; // avoid lambda-capture surprise
                solver.AddLessOrEqualZeroConstraint(
                    WrapWithFiniteDifferenceGradient(
                        captured.Function,
                        $"Inequality '{captured.Name}'",
                        countCall: null),
                    opts.FunctionTolerance);
            }
        }

        if (equalityConstraints is not null)
        {
            foreach (NlpConstraint constraint in equalityConstraints)
            {
                NlpConstraint captured = constraint;
                solver.AddEqualZeroConstraint(
                    WrapWithFiniteDifferenceGradient(
                        captured.Function,
                        $"Equality '{captured.Name}'",
                        countCall: null),
                    opts.FunctionTolerance);
            }
        }

        double[] x = initialGuess.ToArray();
        NloptResult rawStatus = solver.Optimize(x, out double? finalValue);
        NlpTerminationStatus status = NLoptStatusMapper.Map(rawStatus);

        double[]? inequalityViolations = inequalityConstraints is null
            ? null
            : ComputeInequalityViolations(inequalityConstraints, x);
        double[]? equalityViolations = equalityConstraints is null
            ? null
            : ComputeEqualityViolations(equalityConstraints, x);

        return new ConstrainedNlpResult(
            Solution: x,
            FinalValue: finalValue ?? double.NaN,
            IterationCount: evaluations,
            Status: status,
            InequalityViolations: inequalityViolations,
            EqualityViolations: equalityViolations);
    }

    private static double EvaluateChecked(Func<ReadOnlySpan<double>, double> function, double[] variables, string callerDescription)
    {
        double value = function(variables);
        if (double.IsNaN(value))
        {
            throw new InvalidOperationException($"{callerDescription} returned NaN. Check the function and parameter range.");
        }
        return value;
    }

    /// <summary>
    /// Wraps a user-supplied scalar function in NLopt's expected
    /// <c>(variables, gradient) => double</c> signature, computing the gradient via forward
    /// differences when NLopt requests one (gradient buffer is non-null and non-empty).
    /// Derivative-free algorithms (COBYLA, ISRES, Nelder-Mead) pass a null gradient buffer
    /// and so skip the extra evaluations.
    /// </summary>
    private static Func<double[], double[], double> WrapWithFiniteDifferenceGradient(
        Func<ReadOnlySpan<double>, double> function,
        string callerDescription,
        Action? countCall)
    {
        return (variables, gradient) =>
        {
            countCall?.Invoke();
            double f = EvaluateChecked(function, variables, callerDescription);

            if (gradient is { Length: > 0 })
            {
                double[] perturbed = (double[])variables.Clone();
                for (int i = 0; i < variables.Length; i++)
                {
                    double h = 1.0e-7 * Math.Max(Math.Abs(variables[i]), 1.0);
                    perturbed[i] = variables[i] + h;
                    double fPlus = EvaluateChecked(function, perturbed, callerDescription);
                    perturbed[i] = variables[i];
                    gradient[i] = (fPlus - f) / h;
                }
            }

            return f;
        };
    }

    private static void ValidateConstraintList(IReadOnlyList<NlpConstraint>? list, string paramName)
    {
        if (list is null) return;
        HashSet<string> names = new(StringComparer.Ordinal);
        for (int i = 0; i < list.Count; i++)
        {
            NlpConstraint c = list[i];
            if (c is null)
            {
                throw new ArgumentException($"Constraint at index {i} is null.", paramName);
            }
            if (string.IsNullOrWhiteSpace(c.Name))
            {
                throw new ArgumentException($"Constraint at index {i} has empty name.", paramName);
            }
            if (!names.Add(c.Name))
            {
                throw new ArgumentException($"Duplicate constraint name '{c.Name}'.", paramName);
            }
            if (c.Function is null)
            {
                throw new ArgumentException($"Constraint '{c.Name}' has null Function.", paramName);
            }
        }
    }

    private static double[] ComputeInequalityViolations(IReadOnlyList<NlpConstraint> constraints, double[] solution)
    {
        double[] violations = new double[constraints.Count];
        for (int i = 0; i < constraints.Count; i++)
        {
            double g = constraints[i].Function(solution);
            violations[i] = double.IsFinite(g) ? Math.Max(g, 0.0) : double.NaN;
        }
        return violations;
    }

    private static double[] ComputeEqualityViolations(IReadOnlyList<NlpConstraint> constraints, double[] solution)
    {
        double[] violations = new double[constraints.Count];
        for (int i = 0; i < constraints.Count; i++)
        {
            double h = constraints[i].Function(solution);
            violations[i] = double.IsFinite(h) ? Math.Abs(h) : double.NaN;
        }
        return violations;
    }
}
