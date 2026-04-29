namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Inner unconstrained solver used by the <see cref="AugLag"/> Augmented Lagrangian outer
/// loop. AL transforms the constrained problem into a sequence of unconstrained ones, each
/// solved by the inner algorithm.
/// </summary>
public enum AugLagInnerSolver
{
    /// <summary>L-BFGS quasi-Newton (gradient-based, fast on smooth problems). Default.</summary>
    LBfgs,

    /// <summary>SLSQP. Useful when constraints have already been linearized externally and
    /// only the inner sub-problem remains.</summary>
    Slsqp,

    /// <summary>Nelder-Mead simplex (derivative-free, slow but robust on noisy objectives).</summary>
    NelderMead,

    /// <summary>COBYLA (derivative-free, designed for problems where gradients are
    /// unavailable or unreliable).</summary>
    Cobyla,
}
