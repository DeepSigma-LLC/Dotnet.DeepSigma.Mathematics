namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Options for Differential Evolution global minimization.
/// </summary>
/// <remarks>
/// Defaults follow Storn &amp; Price (1997) practice: <see cref="DifferentialWeight"/> = 0.8 and
/// <see cref="CrossoverProbability"/> = 0.9 are well-tested across a range of benchmark problems.
/// Population size of <c>10·n</c> (n = parameter dimension) is the usual rule of thumb; for
/// rugged multimodal landscapes increase to <c>15·n</c> or <c>20·n</c>.
/// </remarks>
public sealed record DifferentialEvolutionOptions
{
    /// <summary>
    /// Number of trial vectors per generation. If <c>null</c>, defaults to <c>max(10·n, 20)</c>
    /// where <c>n</c> is the parameter dimension. DE requires at least four distinct
    /// population members per mutation step, so any explicit value below 4 is rejected.
    /// </summary>
    public int? PopulationSize { get; init; }

    /// <summary>
    /// Differential weight <c>F</c> applied to the donor vector difference in DE/rand/1:
    /// <c>v = a + F · (b − c)</c>. Typical values are in <c>(0, 2]</c>; 0.5–1.0 covers most
    /// practical problems. Default 0.8.
    /// </summary>
    public double DifferentialWeight { get; init; } = 0.8;

    /// <summary>
    /// Crossover probability <c>CR</c> used by the binomial crossover step: each component of
    /// the trial vector is taken from the donor with probability <c>CR</c>, otherwise from the
    /// current target. Must lie in <c>[0, 1]</c>. Default 0.9.
    /// </summary>
    public double CrossoverProbability { get; init; } = 0.9;

    /// <summary>
    /// Maximum number of generations. Each generation evaluates the objective once per
    /// population member (so total objective evaluations ≈ <c>MaxGenerations · PopulationSize</c>
    /// plus the initial population).
    /// </summary>
    public int MaxGenerations { get; init; } = 1_000;

    /// <summary>
    /// Termination tolerance on the spread of objective values across the population:
    /// the optimizer stops when <c>max f(pop_i) − min f(pop_i) ≤ AbsoluteTolerance</c>.
    /// </summary>
    public double AbsoluteTolerance { get; init; } = 1e-8;

    /// <summary>
    /// Seed for the deterministic RNG that drives population initialization and mutation
    /// selection. Pin this for reproducible runs; leave <c>null</c> to draw a random seed.
    /// </summary>
    public int? Seed { get; init; }

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxGenerations);
        Guard.PositiveFinite(AbsoluteTolerance, nameof(AbsoluteTolerance));
        Guard.PositiveFinite(DifferentialWeight, nameof(DifferentialWeight));
        if (!double.IsFinite(CrossoverProbability) || CrossoverProbability < 0.0 || CrossoverProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(CrossoverProbability), CrossoverProbability, "Crossover probability must lie in [0, 1].");
        }
        if (PopulationSize is int explicitSize && explicitSize < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(PopulationSize), explicitSize, "Population size must be at least 4 (DE/rand/1 requires four distinct members).");
        }
    }

    internal int ResolvePopulationSize(int dimension)
    {
        if (PopulationSize is int explicitSize) return explicitSize;
        return Math.Max(10 * dimension, 20);
    }
}
