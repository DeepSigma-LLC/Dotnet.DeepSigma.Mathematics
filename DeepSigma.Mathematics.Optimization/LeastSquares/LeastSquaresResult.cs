namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Result of a nonlinear least-squares minimization.
/// </summary>
public sealed record LeastSquaresResult
{
    /// <summary>
    /// Initializes a least-squares result.
    /// </summary>
    public LeastSquaresResult(
        double[] solution,
        double[] residuals,
        double finalSumOfSquares,
        int iterationCount,
        bool converged,
        double[,]? parameterCovariance)
    {
        Solution = solution;
        Residuals = residuals;
        FinalSumOfSquares = finalSumOfSquares;
        IterationCount = iterationCount;
        Converged = converged;
        ParameterCovariance = parameterCovariance;
    }

    /// <summary>
    /// The fitted parameter vector.
    /// </summary>
    public double[] Solution { get; }

    /// <summary>
    /// Residuals at the fitted solution: <c>r_i(Solution)</c>.
    /// </summary>
    public double[] Residuals { get; }

    /// <summary>
    /// Sum of squared residuals at the solution.
    /// </summary>
    public double FinalSumOfSquares { get; }

    /// <summary>
    /// Number of LM iterations performed (a single iteration may include retries on a rejected step).
    /// </summary>
    public int IterationCount { get; }

    /// <summary>
    /// <c>true</c> if convergence criteria were met before the iteration cap.
    /// </summary>
    public bool Converged { get; }

    /// <summary>
    /// Estimated asymptotic parameter covariance matrix <c>(JᵀJ)⁻¹ · σ̂²</c> at the optimum,
    /// where <c>σ̂² = FinalSumOfSquares / max(1, m − n)</c> for <c>m</c> residuals and
    /// <c>n</c> parameters. <c>null</c> if covariance computation was disabled or the
    /// <c>JᵀJ</c> matrix could not be inverted.
    /// </summary>
    public double[,]? ParameterCovariance { get; }
}
