namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Levenberg-Marquardt minimizer for nonlinear least squares: minimize
/// <c>SSR(x) = Σ_i r_i(x)²</c> over an unconstrained parameter vector <c>x</c>.
/// </summary>
/// <remarks>
/// LM blends Gauss-Newton (fast near the optimum, exploits the least-squares structure of
/// the Jacobian J) with steepest descent (safe far from the optimum). The trust-region
/// damping parameter λ is increased when a step is rejected and decreased when it is
/// accepted, automatically navigating between the two regimes.
/// <para>
/// At each iteration the algorithm:
/// </para>
/// <list type="number">
///   <item><description>Computes residuals <c>r</c> and a forward-difference Jacobian <c>J</c>.</description></item>
///   <item><description>Solves the regularised normal equations <c>(JᵀJ + λI) Δ = −Jᵀr</c> via Cholesky decomposition.</description></item>
///   <item><description>Accepts the step if <c>SSR(x + Δ)</c> decreases; otherwise increases λ and retries.</description></item>
/// </list>
/// <para>
/// References: Marquardt (1963), Moré (1978) "The Levenberg-Marquardt algorithm: implementation
/// and theory." Implementation here uses additive identity-matrix damping (Marquardt's original
/// form) rather than diagonal-of-JᵀJ damping (Levenberg's variant); the additive form is
/// simpler and behaves well for the calibration use cases this package targets.
/// </para>
/// </remarks>
public static class LevenbergMarquardt
{
    /// <summary>
    /// Solves a nonlinear least-squares problem.
    /// </summary>
    /// <param name="residuals">Function returning the residual vector <c>r(x)</c>. The length
    /// of the returned vector is the residual count <c>m</c>; it must match across all
    /// invocations.</param>
    /// <param name="initialGuess">Starting parameter vector of length <c>n</c>.</param>
    /// <param name="options">Optional configuration; defaults are reasonable for typical
    /// regression-style problems.</param>
    public static LeastSquaresResult Solve(
        Func<ReadOnlySpan<double>, double[]> residuals,
        ReadOnlySpan<double> initialGuess,
        LeastSquaresOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(residuals);
        Guard.NotEmpty(initialGuess, nameof(initialGuess), "Initial guess must have at least one parameter.");
        Guard.AllFinite(initialGuess, nameof(initialGuess));

        LeastSquaresOptions opts = options ?? new LeastSquaresOptions();
        opts.Validate();

        int n = initialGuess.Length;
        double[] x = initialGuess.ToArray();
        double[] r = EvaluationHelpers.EvaluateResiduals(residuals, x);
        if (r.Length == 0)
        {
            throw new InvalidOperationException("Residual function returned an empty vector. There is nothing to fit.");
        }

        int m = r.Length;
        double ssr = SumOfSquares(r);

        double lambda = opts.InitialDamping;
        double[,] jacobian = new double[m, n];
        double[,] jtj = new double[n, n];
        double[] jtr = new double[n];
        double[] step = new double[n];
        double[] candidateX = new double[n];
        double[] candidateR;

        bool converged = false;
        int iteration = 0;

        while (iteration < opts.MaxIterations)
        {
            ComputeJacobian(residuals, x, r, opts.FiniteDifferenceStep, jacobian);
            ComputeJtJ(jacobian, jtj);
            ComputeJtr(jacobian, r, jtr);

            // Gradient convergence: ||Jᵀr||_∞ ≤ tolerance. Catches the case where the residuals
            // are already at the noise floor and no further finite step will strictly decrease
            // SSR — without this check, the inner step-acceptance loop would otherwise burn the
            // entire iteration budget rejecting near-zero candidate steps.
            double gradientInfinityNorm = 0.0;
            for (int i = 0; i < n; i++)
            {
                double absGrad = Math.Abs(jtr[i]);
                if (absGrad > gradientInfinityNorm) gradientInfinityNorm = absGrad;
            }
            if (gradientInfinityNorm <= opts.Tolerance)
            {
                converged = true;
                return Finalize(x, r, ssr, iteration, converged, jtj, m, n, opts);
            }

            // Inner loop: increase lambda until we get an accepted step.
            bool stepAccepted = false;
            while (!stepAccepted && iteration < opts.MaxIterations)
            {
                if (!TrySolveRegularised(jtj, jtr, lambda, step))
                {
                    lambda *= opts.DampingIncreaseFactor;
                    iteration++;
                    if (!double.IsFinite(lambda))
                    {
                        return Finalize(x, r, ssr, iteration, converged: false, jtj, m, n, opts);
                    }
                    continue;
                }

                for (int i = 0; i < n; i++)
                {
                    candidateX[i] = x[i] - step[i]; // Δ solves (JᵀJ + λI)Δ = -Jᵀr but we wrote it as +Jᵀr internally below
                }

                candidateR = EvaluationHelpers.EvaluateResiduals(residuals, candidateX);
                double candidateSsr = SumOfSquares(candidateR);
                iteration++;

                if (candidateSsr < ssr)
                {
                    double relativeReduction = (ssr - candidateSsr) / Math.Max(ssr, double.Epsilon);
                    double relativeStep = RelativeStepNorm(step, x);

                    Array.Copy(candidateX, x, n);
                    r = candidateR;
                    ssr = candidateSsr;

                    lambda *= opts.DampingDecreaseFactor;
                    stepAccepted = true;

                    if (relativeReduction <= opts.Tolerance && relativeStep <= opts.Tolerance)
                    {
                        converged = true;
                        return Finalize(x, r, ssr, iteration, converged, jtj, m, n, opts);
                    }
                }
                else
                {
                    lambda *= opts.DampingIncreaseFactor;
                    if (!double.IsFinite(lambda))
                    {
                        return Finalize(x, r, ssr, iteration, converged: false, jtj, m, n, opts);
                    }
                }
            }
        }

        return Finalize(x, r, ssr, iteration, converged, jtj, m, n, opts);
    }

    private static double SumOfSquares(double[] r)
    {
        double sum = 0.0;
        for (int i = 0; i < r.Length; i++)
        {
            sum += r[i] * r[i];
        }
        return sum;
    }

    private static void ComputeJacobian(
        Func<ReadOnlySpan<double>, double[]> residuals,
        double[] x,
        double[] r0,
        double relativeStep,
        double[,] jacobian)
    {
        int m = r0.Length;
        int n = x.Length;
        double[] xPerturbed = (double[])x.Clone();

        for (int j = 0; j < n; j++)
        {
            double h = relativeStep * Math.Max(Math.Abs(x[j]), 1.0);
            double original = xPerturbed[j];
            xPerturbed[j] = original + h;
            double[] rPlus = EvaluationHelpers.EvaluateResiduals(residuals, xPerturbed);
            xPerturbed[j] = original;

            if (rPlus.Length != m)
            {
                throw new InvalidOperationException(
                    $"Residual function returned vectors of different lengths: {m} vs {rPlus.Length}. The residual count must be constant.");
            }

            for (int i = 0; i < m; i++)
            {
                jacobian[i, j] = (rPlus[i] - r0[i]) / h;
            }
        }
    }

    private static void ComputeJtJ(double[,] j, double[,] jtj)
    {
        int m = j.GetLength(0);
        int n = j.GetLength(1);
        for (int p = 0; p < n; p++)
        {
            for (int q = p; q < n; q++)
            {
                double sum = 0.0;
                for (int i = 0; i < m; i++)
                {
                    sum += j[i, p] * j[i, q];
                }
                jtj[p, q] = sum;
                jtj[q, p] = sum;
            }
        }
    }

    private static void ComputeJtr(double[,] j, double[] r, double[] jtr)
    {
        int m = j.GetLength(0);
        int n = j.GetLength(1);
        for (int p = 0; p < n; p++)
        {
            double sum = 0.0;
            for (int i = 0; i < m; i++)
            {
                sum += j[i, p] * r[i];
            }
            jtr[p] = sum;
        }
    }

    /// <summary>
    /// Solves <c>(JᵀJ + λI) step = Jᵀr</c> via Cholesky decomposition. Returns
    /// <c>false</c> if the regularised matrix is not positive definite (e.g., due to a
    /// negative or non-finite λ leading to a numerically singular system).
    /// </summary>
    private static bool TrySolveRegularised(double[,] jtj, double[] jtr, double lambda, double[] step)
    {
        int n = jtj.GetLength(0);
        double[,] augmented = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = jtj[i, j];
            }
            augmented[i, i] += lambda;
        }

        if (!LinearAlgebra.TryCholeskyDecompose(augmented, out double[,] l))
        {
            return false;
        }

        LinearAlgebra.SolveCholesky(l, jtr, step);
        return true;
    }

    private static double RelativeStepNorm(double[] step, double[] x)
    {
        double stepNorm = 0.0;
        double xNorm = 0.0;
        for (int i = 0; i < step.Length; i++)
        {
            stepNorm += step[i] * step[i];
            xNorm += x[i] * x[i];
        }
        stepNorm = Math.Sqrt(stepNorm);
        xNorm = Math.Sqrt(xNorm);
        return stepNorm / Math.Max(xNorm, 1.0);
    }

    private static LeastSquaresResult Finalize(
        double[] x, double[] r, double ssr, int iterations, bool converged,
        double[,] jtj, int m, int n, LeastSquaresOptions opts)
    {
        double[,]? covariance = null;
        if (opts.ComputeParameterCovariance)
        {
            covariance = TryEstimateCovariance(jtj, ssr, m, n);
        }
        return new LeastSquaresResult(x, r, ssr, iterations, converged, covariance);
    }

    private static double[,]? TryEstimateCovariance(double[,] jtj, double ssr, int m, int n)
    {
        // sigma^2 estimate uses (m - n) degrees of freedom; clamp at 1 to avoid div-by-zero
        // for over-determined or under-determined edge cases.
        double sigmaSquared = ssr / Math.Max(m - n, 1);

        double[,] copy = (double[,])jtj.Clone();
        if (!LinearAlgebra.TryCholeskyDecompose(copy, out double[,] l))
        {
            return null;
        }

        double[,] inverse = LinearAlgebra.InvertFromCholesky(l);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                inverse[i, j] *= sigmaSquared;
            }
        }
        return inverse;
    }
}
