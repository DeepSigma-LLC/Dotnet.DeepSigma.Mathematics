namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Shared evaluation wrappers for objective and residual callbacks. Every optimizer that
/// invokes a user-supplied callback funnels through here so that NaN-guarding and
/// non-finite reporting is consistent across the package.
/// </summary>
internal static class EvaluationHelpers
{
    /// <summary>
    /// Invokes a scalar objective and throws <see cref="InvalidOperationException"/> if it
    /// returns NaN. ±∞ is allowed (some optimizers can make progress on unbounded values).
    /// </summary>
    public static double EvaluateScalar(Func<ReadOnlySpan<double>, double> objective, ReadOnlySpan<double> point)
    {
        double value = objective(point);
        if (double.IsNaN(value))
        {
            throw new InvalidOperationException("Objective function returned NaN. Check the objective and parameter range.");
        }
        return value;
    }

    /// <summary>
    /// Invokes a residual function and validates the returned vector is non-null and
    /// elementwise finite. Returns the residual vector unchanged.
    /// </summary>
    public static double[] EvaluateResiduals(Func<ReadOnlySpan<double>, double[]> residuals, ReadOnlySpan<double> point)
    {
        double[] r = residuals(point);
        if (r is null)
        {
            throw new InvalidOperationException("Residual function returned null.");
        }
        for (int i = 0; i < r.Length; i++)
        {
            if (!double.IsFinite(r[i]))
            {
                throw new InvalidOperationException($"Residual function returned non-finite value at index {i}: {r[i]:G}.");
            }
        }
        return r;
    }
}
