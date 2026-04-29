using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// ISRES — Improved Stochastic Ranking Evolution Strategy (Runarsson &amp; Yao 2005). A
/// global, stochastic, derivative-free NLP solver that handles inequality and equality
/// constraints via a stochastic-ranking penalty. Use this when local methods like
/// <see cref="Slsqp"/> and <see cref="Cobyla"/> keep getting stuck in local minima.
/// </summary>
/// <remarks>
/// Bounds are required — ISRES samples uniformly within the search box during
/// initialization. Slower than local methods on smooth single-minimum problems, but the
/// only constrained-NLP option in this package that escapes local minima.
/// <para>
/// <b>Determinism limitation:</b> the NLoptNet binding used here does not expose NLopt's
/// <c>nlopt_srand</c> entry point, so ISRES uses the underlying NLopt RNG with whatever
/// seed it has at process start. <see cref="ConstrainedNlpOptions.Seed"/> is therefore
/// ignored by this algorithm in v1. Convergence to the global optimum is probabilistic —
/// budget a generous <see cref="ConstrainedNlpOptions.MaxIterations"/> on hard problems,
/// and consider running multiple invocations if reliability matters.
/// </para>
/// </remarks>
public static class Isres
{
    /// <summary>
    /// Globally minimizes <paramref name="objective"/> via ISRES.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="lowerBounds"/> or
    /// <paramref name="upperBounds"/> are not supplied. ISRES requires a finite search box.</exception>
    public static ConstrainedNlpResult Solve(
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> initialGuess,
        IReadOnlyList<NlpConstraint>? inequalityConstraints = null,
        IReadOnlyList<NlpConstraint>? equalityConstraints = null,
        ReadOnlySpan<double> lowerBounds = default,
        ReadOnlySpan<double> upperBounds = default,
        ConstrainedNlpOptions? options = null)
    {
        if (lowerBounds.Length == 0 || upperBounds.Length == 0)
        {
            throw new ArgumentException(
                "ISRES is a global solver and requires both lower and upper bounds. Pass finite per-parameter bounds.",
                nameof(lowerBounds));
        }

        return NLoptInvoker.Invoke(
            algorithm: NLoptAlgorithm.GN_ISRES,
            algorithmName: nameof(Isres),
            objective: objective,
            initialGuess: initialGuess,
            inequalityConstraints: inequalityConstraints,
            equalityConstraints: equalityConstraints,
            lowerBounds: lowerBounds,
            upperBounds: upperBounds,
            options: options);
    }
}
