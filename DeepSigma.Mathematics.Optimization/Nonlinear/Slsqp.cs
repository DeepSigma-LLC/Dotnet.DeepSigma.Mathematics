using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// SLSQP — Sequential Least-Squares Quadratic Programming. The textbook gradient-based SQP
/// method for smooth constrained nonlinear programs (Kraft 1988). Use this when the
/// objective and constraints are differentiable and you want fast convergence to a local
/// optimum.
/// </summary>
/// <remarks>
/// Gradients are auto-computed by NLopt via finite differences when the caller does not
/// supply them. SLSQP handles equality and inequality constraints natively. For non-smooth
/// or noisy problems prefer <see cref="Cobyla"/>; for global searches prefer
/// <see cref="Isres"/>.
/// </remarks>
public static class Slsqp
{
    /// <summary>
    /// Minimizes <paramref name="objective"/> subject to optional inequality, equality, and
    /// box constraints.
    /// </summary>
    /// <param name="objective">Function to minimize.</param>
    /// <param name="initialGuess">Starting parameter vector.</param>
    /// <param name="inequalityConstraints">Optional list of <c>g(x) ≤ 0</c> constraints.</param>
    /// <param name="equalityConstraints">Optional list of <c>h(x) = 0</c> constraints.</param>
    /// <param name="lowerBounds">Optional per-parameter lower bounds. Must match
    /// <paramref name="initialGuess"/> length if supplied; otherwise leave default.</param>
    /// <param name="upperBounds">Optional per-parameter upper bounds.</param>
    /// <param name="options">Optional configuration; defaults are reasonable for typical problems.</param>
    public static ConstrainedNlpResult Solve(
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> initialGuess,
        IReadOnlyList<NlpConstraint>? inequalityConstraints = null,
        IReadOnlyList<NlpConstraint>? equalityConstraints = null,
        ReadOnlySpan<double> lowerBounds = default,
        ReadOnlySpan<double> upperBounds = default,
        ConstrainedNlpOptions? options = null)
    {
        return NLoptInvoker.Invoke(
            algorithm: NLoptAlgorithm.LD_SLSQP,
            algorithmName: nameof(Slsqp),
            objective: objective,
            initialGuess: initialGuess,
            inequalityConstraints: inequalityConstraints,
            equalityConstraints: equalityConstraints,
            lowerBounds: lowerBounds,
            upperBounds: upperBounds,
            options: options);
    }
}
