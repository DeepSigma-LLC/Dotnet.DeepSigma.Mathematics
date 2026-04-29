namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Brent-Dekker 1-D root finder. Combines bisection (safe, guaranteed convergence) with
/// inverse quadratic interpolation and the secant method (fast, super-linear convergence)
/// to deliver both robustness and speed on smooth scalar functions.
/// </summary>
/// <remarks>
/// References: R. P. Brent, <i>Algorithms for Minimization Without Derivatives</i> (1973),
/// chapter 4. The implementation here is the canonical Brent-Dekker algorithm with the
/// safeguards described in Press et al., <i>Numerical Recipes</i> (3rd edition), §9.3.
/// <para>
/// The bracket <c>[lowerBound, upperBound]</c> must enclose a sign change of the function.
/// If <c>f(lowerBound)</c> and <c>f(upperBound)</c> have the same sign the algorithm refuses
/// to start — bisection-style methods cannot be safely used without a guaranteed bracket.
/// Termination is on bracket width <c>|b − a| ≤ AbsoluteTolerance</c> or function value
/// <c>|f(b)| ≤ AbsoluteTolerance</c>.
/// </para>
/// </remarks>
public static class BrentRootFinder
{
    /// <summary>
    /// Finds a root of <paramref name="function"/> within the bracket
    /// <c>[lowerBound, upperBound]</c>.
    /// </summary>
    /// <param name="function">The continuous scalar function whose root we want.</param>
    /// <param name="lowerBound">Lower end of the bracket. Must be finite and strictly less than <paramref name="upperBound"/>.</param>
    /// <param name="upperBound">Upper end of the bracket.</param>
    /// <param name="absoluteTolerance">Termination tolerance on bracket width and function value. Defaults to <c>1e-12</c>.</param>
    /// <param name="maxIterations">Maximum number of function evaluations. Defaults to <c>100</c>.</param>
    /// <exception cref="ArgumentException">If the function does not change sign across the bracket.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the bracket is invalid or arguments are out of range.</exception>
    public static RootFindingResult FindRoot(
        Func<double, double> function,
        double lowerBound,
        double upperBound,
        double absoluteTolerance = 1.0e-12,
        int maxIterations = 100)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (!double.IsFinite(lowerBound))
        {
            throw new ArgumentOutOfRangeException(nameof(lowerBound), lowerBound, "Lower bound must be finite.");
        }

        if (!double.IsFinite(upperBound))
        {
            throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "Upper bound must be finite.");
        }

        if (lowerBound >= upperBound)
        {
            throw new ArgumentOutOfRangeException(nameof(lowerBound), lowerBound, "Lower bound must be strictly less than the upper bound.");
        }

        if (!double.IsFinite(absoluteTolerance) || absoluteTolerance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(absoluteTolerance), absoluteTolerance, "Absolute tolerance must be finite and positive.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxIterations);

        double a = lowerBound;
        double b = upperBound;
        double fa = function(a);
        double fb = function(b);
        ThrowIfNotFinite(fa, nameof(function), a);
        ThrowIfNotFinite(fb, nameof(function), b);

        if (fa == 0.0)
        {
            return new RootFindingResult(a, fa, iterationCount: 0, converged: true);
        }
        if (fb == 0.0)
        {
            return new RootFindingResult(b, fb, iterationCount: 0, converged: true);
        }

        if (Math.Sign(fa) == Math.Sign(fb))
        {
            throw new ArgumentException(
                $"f(lowerBound)={fa:G} and f(upperBound)={fb:G} have the same sign; cannot guarantee a root in [{a:G}, {b:G}].",
                nameof(function));
        }

        // Make |f(a)| >= |f(b)| so that b is always the current best guess.
        if (Math.Abs(fa) < Math.Abs(fb))
        {
            (a, b) = (b, a);
            (fa, fb) = (fb, fa);
        }

        double c = a;
        double fc = fa;
        double d = 0.0;             // last successful step (used by interpolation safeguards)
        bool usedBisection = true;
        int iterations = 0;

        while (iterations < maxIterations)
        {
            if (Math.Abs(fb) <= absoluteTolerance || Math.Abs(b - a) <= absoluteTolerance)
            {
                return new RootFindingResult(b, fb, iterations, converged: true);
            }

            double s;
            if (fa != fc && fb != fc)
            {
                // Inverse quadratic interpolation through (a, fa), (b, fb), (c, fc).
                double term1 = a * fb * fc / ((fa - fb) * (fa - fc));
                double term2 = b * fa * fc / ((fb - fa) * (fb - fc));
                double term3 = c * fa * fb / ((fc - fa) * (fc - fb));
                s = term1 + term2 + term3;
            }
            else
            {
                // Secant fallback when two of fa, fb, fc coincide.
                s = b - fb * (b - a) / (fb - fa);
            }

            double trustLow = (3.0 * a + b) / 4.0;
            double trustHigh = b;
            if (trustLow > trustHigh)
            {
                (trustLow, trustHigh) = (trustHigh, trustLow);
            }

            bool sOutsideTrust = s < trustLow || s > trustHigh;
            bool slowProgressBisect = usedBisection && Math.Abs(s - b) >= 0.5 * Math.Abs(b - c);
            bool slowProgressInterp = !usedBisection && Math.Abs(s - b) >= 0.5 * Math.Abs(c - d);
            bool tinyGapBisect = usedBisection && Math.Abs(b - c) < absoluteTolerance;
            bool tinyGapInterp = !usedBisection && Math.Abs(c - d) < absoluteTolerance;

            if (sOutsideTrust || slowProgressBisect || slowProgressInterp || tinyGapBisect || tinyGapInterp)
            {
                s = 0.5 * (a + b);
                usedBisection = true;
            }
            else
            {
                usedBisection = false;
            }

            double fs = function(s);
            ThrowIfNotFinite(fs, nameof(function), s);
            iterations++;

            d = c;
            c = b;
            fc = fb;

            if (Math.Sign(fa) != Math.Sign(fs))
            {
                b = s;
                fb = fs;
            }
            else
            {
                a = s;
                fa = fs;
            }

            if (Math.Abs(fa) < Math.Abs(fb))
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
            }
        }

        return new RootFindingResult(b, fb, iterations, converged: false);
    }

    private static void ThrowIfNotFinite(double value, string parameterName, double inputForMessage)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                $"{parameterName} returned non-finite value {value:G} at x={inputForMessage:G}. The function must be finite throughout the bracket.");
        }
    }
}
