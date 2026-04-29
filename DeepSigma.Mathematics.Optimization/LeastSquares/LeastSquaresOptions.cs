namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Options for nonlinear least-squares minimization via Levenberg-Marquardt.
/// </summary>
public sealed record LeastSquaresOptions
{
    /// <summary>
    /// Maximum number of LM iterations (each iteration may require multiple residual
    /// evaluations to find an accepted step).
    /// </summary>
    public int MaxIterations { get; init; } = 100;

    /// <summary>
    /// Termination tolerance on the relative reduction in sum of squared residuals
    /// (<c>|prevSSR − newSSR| / max(prevSSR, ε) ≤ tol</c>) and on the relative parameter
    /// step size. Typical values are <c>1e-8</c> to <c>1e-12</c>.
    /// </summary>
    public double Tolerance { get; init; } = 1.0e-10;

    /// <summary>
    /// Forward-difference step size used to approximate the Jacobian. Each Jacobian column
    /// is computed as <c>(r(x + h_i e_i) − r(x)) / h_i</c> with
    /// <c>h_i = FiniteDifferenceStep · max(|x_i|, 1)</c>.
    /// </summary>
    public double FiniteDifferenceStep { get; init; } = 1.0e-7;

    /// <summary>
    /// Initial Marquardt damping factor λ. Larger values bias the step toward steepest-
    /// descent (slower but safer); smaller values bias toward Gauss-Newton (faster but
    /// fragile when the Jacobian is ill-conditioned). The algorithm adapts λ at runtime.
    /// </summary>
    public double InitialDamping { get; init; } = 1.0e-3;

    /// <summary>
    /// Multiplicative factor for decreasing λ after a successful step (must be in (0, 1)).
    /// Default 0.5: cut λ in half each time the model and the residual reduction agree.
    /// </summary>
    public double DampingDecreaseFactor { get; init; } = 0.5;

    /// <summary>
    /// Multiplicative factor for increasing λ after a rejected step (must be > 1).
    /// Default 2.0: double λ each time the proposed step does not reduce residuals.
    /// </summary>
    public double DampingIncreaseFactor { get; init; } = 2.0;

    /// <summary>
    /// When <c>true</c>, the result includes an estimated parameter-covariance matrix
    /// <c>(JᵀJ)⁻¹ · σ̂²</c> at the optimum, which is the standard asymptotic covariance for
    /// nonlinear least-squares. Free unless an additional matrix solve is needed; default <c>true</c>.
    /// </summary>
    public bool ComputeParameterCovariance { get; init; } = true;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxIterations);
        Guard.PositiveFinite(Tolerance, nameof(Tolerance));
        Guard.PositiveFinite(FiniteDifferenceStep, nameof(FiniteDifferenceStep));
        Guard.PositiveFinite(InitialDamping, nameof(InitialDamping));
        if (!double.IsFinite(DampingDecreaseFactor) || DampingDecreaseFactor <= 0.0 || DampingDecreaseFactor >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(DampingDecreaseFactor), DampingDecreaseFactor, "DampingDecreaseFactor must be in (0, 1).");
        }
        if (!double.IsFinite(DampingIncreaseFactor) || DampingIncreaseFactor <= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(DampingIncreaseFactor), DampingIncreaseFactor, "DampingIncreaseFactor must be greater than 1.");
        }
    }
}
