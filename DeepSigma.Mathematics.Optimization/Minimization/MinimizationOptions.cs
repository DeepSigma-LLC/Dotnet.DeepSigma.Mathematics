namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Options for derivative-free minimization.
/// </summary>
public sealed record MinimizationOptions
{
    /// <summary>
    /// Maximum number of objective evaluations after the initial simplex is built.
    /// </summary>
    public int MaxIterations { get; init; } = 1_000;

    /// <summary>
    /// Termination tolerance on the spread of objective values across the simplex. The
    /// optimizer stops when <c>max f(simplex_i) − min f(simplex_i) ≤ AbsoluteTolerance</c>.
    /// Typical values are <c>1e-6</c> to <c>1e-10</c>.
    /// </summary>
    public double AbsoluteTolerance { get; init; } = 1e-8;

    /// <summary>
    /// Edge length used to construct the initial simplex around <c>initialGuess</c>. Each
    /// non-pivot vertex perturbs one coordinate by either an additive or relative amount;
    /// see <see cref="RelativePerturbation"/>.
    /// </summary>
    public double InitialSimplexEdge { get; init; } = 0.05;

    /// <summary>
    /// When <c>true</c>, the i-th non-pivot simplex vertex perturbs coordinate <c>i</c> by
    /// <c>InitialSimplexEdge · max(|guess[i]|, 1)</c>. When <c>false</c>, the perturbation
    /// is purely additive (<c>InitialSimplexEdge</c>). Relative perturbation is more robust
    /// when parameters span very different scales.
    /// </summary>
    public bool RelativePerturbation { get; init; } = true;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxIterations);
        Guard.PositiveFinite(AbsoluteTolerance, nameof(AbsoluteTolerance));
        Guard.PositiveFinite(InitialSimplexEdge, nameof(InitialSimplexEdge));
    }
}
