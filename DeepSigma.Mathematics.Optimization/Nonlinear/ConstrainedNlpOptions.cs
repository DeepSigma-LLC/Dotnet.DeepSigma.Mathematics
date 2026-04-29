namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Solver configuration knobs that apply uniformly across the NLP backends in this package.
/// </summary>
/// <remarks>
/// Per-algorithm tuning (population size for ISRES, asymptote heuristics for MMA, etc.) is
/// intentionally not exposed here — add it when a real consumer asks. v1 keeps the surface
/// small and portable across algorithms.
/// </remarks>
public sealed record ConstrainedNlpOptions
{
    /// <summary>
    /// Maximum number of objective evaluations the solver will perform. Most NLP algorithms
    /// will terminate earlier on a tolerance test; this is the safety cap.
    /// </summary>
    public int MaxIterations { get; init; } = 200;

    /// <summary>
    /// Absolute tolerance on the change in the objective value between consecutive accepted
    /// steps. The solver stops when the change drops below this.
    /// </summary>
    public double FunctionTolerance { get; init; } = 1.0e-8;

    /// <summary>
    /// Relative tolerance on the parameter step size. The solver stops when the largest
    /// component of the step (relative to the current parameter magnitude) drops below this.
    /// </summary>
    public double ParameterTolerance { get; init; } = 1.0e-8;

    /// <summary>
    /// Wall-clock cap for the solver. <c>null</c> leaves the solver's own default in place.
    /// </summary>
    public TimeSpan? TimeLimit { get; init; }

    /// <summary>
    /// Seed for any stochastic algorithm (e.g. <c>Isres</c>). Pin this for reproducible runs.
    /// Leave <c>null</c> to use OS-clock randomness. Ignored by deterministic algorithms.
    /// </summary>
    public int? Seed { get; init; }

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxIterations);
        Guard.PositiveFinite(FunctionTolerance, nameof(FunctionTolerance));
        Guard.PositiveFinite(ParameterTolerance, nameof(ParameterTolerance));
        if (TimeLimit is TimeSpan tl && tl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(TimeLimit), tl, "TimeLimit must be positive.");
        }
    }
}
