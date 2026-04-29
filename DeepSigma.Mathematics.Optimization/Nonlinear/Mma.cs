using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// MMA — Method of Moving Asymptotes (Svanberg 2002). Gradient-based local NLP solver
/// designed for problems with many inequality constraints. Approximates the objective and
/// constraints with separable convex functions (the "moving asymptotes") around each
/// iterate and minimizes the approximation, repeating until convergence.
/// </summary>
/// <remarks>
/// Strong on engineering / structural-design problems. Equality constraints are not
/// supported by NLopt's MMA implementation — pass them via <see cref="Slsqp"/> or
/// <see cref="AugLag"/> instead. Gradients are auto-computed via finite differences when
/// the caller does not supply them.
/// </remarks>
public static class Mma
{
    /// <summary>
    /// Minimizes <paramref name="objective"/> via MMA.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="equalityConstraints"/> is non-empty:
    /// MMA does not support equality constraints.</exception>
    public static ConstrainedNlpResult Solve(
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> initialGuess,
        IReadOnlyList<NlpConstraint>? inequalityConstraints = null,
        IReadOnlyList<NlpConstraint>? equalityConstraints = null,
        ReadOnlySpan<double> lowerBounds = default,
        ReadOnlySpan<double> upperBounds = default,
        ConstrainedNlpOptions? options = null)
    {
        if (equalityConstraints is { Count: > 0 })
        {
            throw new ArgumentException(
                "MMA does not support equality constraints. Use Slsqp or AugLag instead.",
                nameof(equalityConstraints));
        }

        return NLoptInvoker.Invoke(
            algorithm: NLoptAlgorithm.LD_MMA,
            algorithmName: nameof(Mma),
            objective: objective,
            initialGuess: initialGuess,
            inequalityConstraints: inequalityConstraints,
            equalityConstraints: null,
            lowerBounds: lowerBounds,
            upperBounds: upperBounds,
            options: options);
    }
}
