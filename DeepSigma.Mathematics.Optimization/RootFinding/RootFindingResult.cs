namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Result of a 1-D root-finding run.
/// </summary>
public sealed record RootFindingResult
{
    /// <summary>
    /// Initializes a root-finding result.
    /// </summary>
    public RootFindingResult(double root, double functionValue, int iterationCount, bool converged)
    {
        Root = root;
        FunctionValue = functionValue;
        IterationCount = iterationCount;
        Converged = converged;
    }

    /// <summary>
    /// The estimated root.
    /// </summary>
    public double Root { get; }

    /// <summary>
    /// The function value at <see cref="Root"/>; ideally near zero.
    /// </summary>
    public double FunctionValue { get; }

    /// <summary>
    /// Number of function evaluations after the initial bracket check.
    /// </summary>
    public int IterationCount { get; }

    /// <summary>
    /// <c>true</c> if the bracket width fell below the requested tolerance before the
    /// iteration cap; <c>false</c> if the cap was reached first.
    /// </summary>
    public bool Converged { get; }
}
