namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Result of a minimization run. Returned by both <see cref="NelderMead"/> and
/// <see cref="DifferentialEvolution"/>.
/// </summary>
public sealed record MinimizationResult
{
    /// <summary>
    /// Initializes a new minimization result.
    /// </summary>
    public MinimizationResult(double[] solution, double finalValue, int iterationCount, bool converged)
    {
        Solution = solution;
        FinalValue = finalValue;
        IterationCount = iterationCount;
        Converged = converged;
    }

    /// <summary>
    /// The best parameter vector found.
    /// </summary>
    public double[] Solution { get; }

    /// <summary>
    /// The objective value at <see cref="Solution"/>.
    /// </summary>
    public double FinalValue { get; }

    /// <summary>
    /// Algorithm-specific iteration counter. For <see cref="NelderMead"/> this is the number
    /// of objective evaluations performed after the initial simplex was built. For
    /// <see cref="DifferentialEvolution"/> this is the number of generations completed.
    /// </summary>
    public int IterationCount { get; }

    /// <summary>
    /// <c>true</c> if the algorithm-specific convergence criterion was met before the
    /// iteration cap was reached. Each algorithm defines this differently — for
    /// <see cref="NelderMead"/> it is the spread of simplex objective values falling below
    /// the configured <c>AbsoluteTolerance</c>; for <see cref="DifferentialEvolution"/> it
    /// is the spread of population objective values falling below the same threshold.
    /// </summary>
    public bool Converged { get; }
}
