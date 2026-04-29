namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Solver configuration knobs that apply uniformly across the LP/MIP backends.
/// </summary>
/// <remarks>
/// Per-backend tuning (Glop's pricing rule, CBC's gap tolerance, etc.) is intentionally
/// not exposed here — add it when a real consumer asks. v1 keeps the surface small and
/// portable across backends.
/// </remarks>
public sealed record LinearProgrammingOptions
{
    /// <summary>
    /// Wall-clock cap for the solver. <c>null</c> leaves the solver's own default in place.
    /// </summary>
    public TimeSpan? TimeLimit { get; init; }

    /// <summary>
    /// Iteration cap for simplex-style methods. Ignored by first-order methods (PDLP) and
    /// MIP heuristics. <c>null</c> leaves the solver's own default in place.
    /// </summary>
    public int? IterationLimit { get; init; }

    /// <summary>
    /// When <c>true</c>, the underlying solver writes progress to stdout. Useful for
    /// diagnosing slow / non-converging runs; default <c>false</c>.
    /// </summary>
    public bool VerboseLogging { get; init; }
}
