namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Result of a constrained-NLP solve. Returned by every backend in this package.
/// </summary>
/// <param name="Solution">Best parameter vector the solver found. Populated even when
/// <paramref name="Status"/> indicates non-convergence — many algorithms can return the
/// best-so-far on a budget exhaustion.</param>
/// <param name="FinalValue">Objective value at <paramref name="Solution"/>. <c>NaN</c> only
/// if the solver failed before evaluating any candidate.</param>
/// <param name="IterationCount">Number of objective evaluations performed.</param>
/// <param name="Status">Reason the solver returned. See <see cref="NlpTerminationStatus"/>.</param>
/// <param name="InequalityViolations">For each inequality constraint <c>g_i(x) ≤ 0</c>,
/// holds <c>max(g_i(Solution), 0)</c>. Zero if all are satisfied; positive entries point at
/// constraints the solver could not respect. <c>null</c> if the call had no inequality
/// constraints.</param>
/// <param name="EqualityViolations">For each equality constraint <c>h_j(x) = 0</c>, holds
/// <c>|h_j(Solution)|</c>. <c>null</c> if the call had no equality constraints.</param>
public sealed record ConstrainedNlpResult(
    double[] Solution,
    double FinalValue,
    int IterationCount,
    NlpTerminationStatus Status,
    double[]? InequalityViolations,
    double[]? EqualityViolations);
