namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Result of an LP/MIP solve. Returned by every backend.
/// </summary>
/// <param name="VariableValues">Map from variable name to its solution value. Populated for
/// <see cref="LinearTerminationStatus.Optimal"/> and <see cref="LinearTerminationStatus.Feasible"/>;
/// for other statuses the contents are unspecified.</param>
/// <param name="ObjectiveValue">Objective value at the solution, including the objective's
/// constant. Meaningful only when <paramref name="Status"/> indicates a solution was produced.</param>
/// <param name="Status">Reason the solver returned. See <see cref="LinearTerminationStatus"/>.</param>
/// <param name="IterationCount">Number of iterations the backend reported, where applicable.
/// Zero for backends that don't expose an iteration counter (PDLP, some CBC heuristics).</param>
/// <param name="SolveTime">Wall-clock time the backend reported for the solve phase.
/// Excludes problem translation overhead.</param>
public sealed record LinearResult(
    IReadOnlyDictionary<string, double> VariableValues,
    double ObjectiveValue,
    LinearTerminationStatus Status,
    int IterationCount,
    TimeSpan SolveTime);
