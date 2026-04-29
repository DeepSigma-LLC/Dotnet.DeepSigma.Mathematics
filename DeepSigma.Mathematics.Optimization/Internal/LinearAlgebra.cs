namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Internal dense linear-algebra primitives used by the least-squares solver. Scope is
/// deliberately minimal — just what Levenberg-Marquardt needs (Cholesky factorization,
/// triangular solves, SPD inversion). If a second consumer appears, broaden the surface
/// then; do not pre-emptively ship a generic matrix library.
/// </summary>
internal static class LinearAlgebra
{
    /// <summary>
    /// Cholesky decomposition of a symmetric positive-definite matrix <paramref name="a"/>.
    /// On success, <paramref name="lower"/> is the lower-triangular factor <c>L</c> such
    /// that <c>L Lᵀ = a</c>; the strict upper triangle is left as zero. Returns
    /// <c>false</c> if the matrix is not positive-definite (a non-positive or non-finite
    /// pivot was encountered) — in that case <paramref name="lower"/> is non-null but its
    /// contents are partial and should not be used.
    /// </summary>
    public static bool TryCholeskyDecompose(double[,] a, out double[,] lower)
    {
        int n = a.GetLength(0);
        lower = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = a[i, j];
                for (int k = 0; k < j; k++)
                {
                    sum -= lower[i, k] * lower[j, k];
                }
                if (i == j)
                {
                    if (sum <= 0.0 || !double.IsFinite(sum))
                    {
                        return false;
                    }
                    lower[i, j] = Math.Sqrt(sum);
                }
                else
                {
                    lower[i, j] = sum / lower[j, j];
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Solves <c>L Lᵀ x = rhs</c> in-place into <paramref name="solution"/>, given a
    /// Cholesky factor <paramref name="lower"/>. Allocates a length-<c>n</c> workspace for
    /// the forward-substitution intermediate.
    /// </summary>
    public static void SolveCholesky(double[,] lower, double[] rhs, double[] solution)
    {
        int n = lower.GetLength(0);
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = rhs[i];
            for (int k = 0; k < i; k++)
            {
                sum -= lower[i, k] * y[k];
            }
            y[i] = sum / lower[i, i];
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = y[i];
            for (int k = i + 1; k < n; k++)
            {
                sum -= lower[k, i] * solution[k];
            }
            solution[i] = sum / lower[i, i];
        }
    }

    /// <summary>
    /// Inverts a symmetric positive-definite matrix given its Cholesky factor by solving
    /// <c>L Lᵀ X = I</c> column by column. Returns the dense inverse.
    /// </summary>
    public static double[,] InvertFromCholesky(double[,] lower)
    {
        int n = lower.GetLength(0);
        double[,] inverse = new double[n, n];

        for (int col = 0; col < n; col++)
        {
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                double rhs = i == col ? 1.0 : 0.0;
                double sum = rhs;
                for (int k = 0; k < i; k++)
                {
                    sum -= lower[i, k] * y[k];
                }
                y[i] = sum / lower[i, i];
            }
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = y[i];
                for (int k = i + 1; k < n; k++)
                {
                    sum -= lower[k, i] * inverse[k, col];
                }
                inverse[i, col] = sum / lower[i, i];
            }
        }
        return inverse;
    }
}
