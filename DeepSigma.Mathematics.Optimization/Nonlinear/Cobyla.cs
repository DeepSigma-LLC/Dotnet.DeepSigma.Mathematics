using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// COBYLA — Constrained Optimization BY Linear Approximation (Powell 1994). A
/// derivative-free local NLP solver that builds linear approximations of the objective and
/// constraints from objective evaluations. Use this when the objective or constraints are
/// noisy, non-smooth, or expensive in a way that makes finite differences unreliable.
/// </summary>
/// <remarks>
/// Slower convergence than gradient-based methods like <see cref="Slsqp"/> on smooth
/// problems, but more robust on real-world objectives. Handles inequality constraints
/// natively; equality constraints are accepted but each is internally split into a pair of
/// inequalities, which can hurt convergence — prefer <see cref="Slsqp"/> when equalities
/// dominate.
/// </remarks>
public static class Cobyla
{
    /// <summary>
    /// Minimizes <paramref name="objective"/> via COBYLA.
    /// </summary>
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
            algorithm: NLoptAlgorithm.LN_COBYLA,
            algorithmName: nameof(Cobyla),
            objective: objective,
            initialGuess: initialGuess,
            inequalityConstraints: inequalityConstraints,
            equalityConstraints: equalityConstraints,
            lowerBounds: lowerBounds,
            upperBounds: upperBounds,
            options: options);
    }
}
