using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Augmented Lagrangian — wraps an inner unconstrained solver with a Lagrangian-and-penalty
/// outer loop that drives the constraint residuals toward zero. Robust on a wider class of
/// problems than <see cref="Slsqp"/> at the cost of slower convergence.
/// </summary>
/// <remarks>
/// AL transforms the constrained problem
/// <c>min f(x) s.t. h(x) = 0, g(x) ≤ 0</c>
/// into a sequence of unconstrained minimizations of the augmented Lagrangian
/// <c>L(x, λ, μ) = f(x) + λᵀh(x) + (μ/2) ‖h(x)‖²</c> (plus inequality terms via slack
/// variables). NLopt schedules the multiplier and penalty updates; we wrap the call.
/// <para>
/// Choose the inner solver via <see cref="AugLagInnerSolver"/>. L-BFGS is the default and
/// works well on smooth problems; use Nelder-Mead or COBYLA when gradients are unreliable.
/// </para>
/// </remarks>
public static class AugLag
{
    /// <summary>
    /// Minimizes <paramref name="objective"/> via Augmented Lagrangian. The unconstrained
    /// inner solver run by AL on each outer iteration is selected via
    /// <paramref name="innerSolver"/>; <see cref="AugLagInnerSolver.LBfgs"/> is the default.
    /// </summary>
    /// <param name="objective">Function to minimize.</param>
    /// <param name="initialGuess">Starting parameter vector.</param>
    /// <param name="inequalityConstraints">Optional list of <c>g(x) ≤ 0</c> constraints.</param>
    /// <param name="equalityConstraints">Optional list of <c>h(x) = 0</c> constraints.</param>
    /// <param name="lowerBounds">Optional per-parameter lower bounds.</param>
    /// <param name="upperBounds">Optional per-parameter upper bounds.</param>
    /// <param name="options">Optional configuration; defaults are reasonable for typical problems.</param>
    /// <param name="innerSolver">Unconstrained solver used inside the outer Lagrangian loop.</param>
    public static ConstrainedNlpResult Solve(
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> initialGuess,
        IReadOnlyList<NlpConstraint>? inequalityConstraints = null,
        IReadOnlyList<NlpConstraint>? equalityConstraints = null,
        ReadOnlySpan<double> lowerBounds = default,
        ReadOnlySpan<double> upperBounds = default,
        ConstrainedNlpOptions? options = null,
        AugLagInnerSolver innerSolver = AugLagInnerSolver.LBfgs)
    {
        NLoptAlgorithm child = innerSolver switch
        {
            AugLagInnerSolver.LBfgs => NLoptAlgorithm.LD_LBFGS,
            AugLagInnerSolver.Slsqp => NLoptAlgorithm.LD_SLSQP,
            AugLagInnerSolver.NelderMead => NLoptAlgorithm.LN_NELDERMEAD,
            AugLagInnerSolver.Cobyla => NLoptAlgorithm.LN_COBYLA,
            _ => throw new ArgumentOutOfRangeException(nameof(innerSolver), innerSolver, "Unknown inner solver."),
        };

        return NLoptInvoker.Invoke(
            algorithm: NLoptAlgorithm.AUGLAG,
            algorithmName: nameof(AugLag),
            objective: objective,
            initialGuess: initialGuess,
            inequalityConstraints: inequalityConstraints,
            equalityConstraints: equalityConstraints,
            lowerBounds: lowerBounds,
            upperBounds: upperBounds,
            options: options,
            childAlgorithm: child);
    }
}
