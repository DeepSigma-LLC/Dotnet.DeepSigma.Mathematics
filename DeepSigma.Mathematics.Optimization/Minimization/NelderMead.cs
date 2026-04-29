namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Nelder-Mead derivative-free simplex minimizer. Robust on smooth low-dimensional problems
/// (typically up to ~10 parameters); does not require derivatives.
/// </summary>
/// <remarks>
/// Implements the canonical algorithm with the Lagarias, Reeds, Wright &amp; Wright (1998)
/// coefficients (reflection α = 1, expansion γ = 2, contraction ρ = 0.5, shrink σ = 0.5).
/// The simplex of <c>n + 1</c> vertices in <c>n</c>-dimensional space is updated each
/// iteration via reflection / expansion / contraction / shrink, depending on how the
/// reflected vertex compares to the current best, second-worst, and worst values.
/// <para>
/// Termination is on the spread of simplex objective values
/// <c>max f − min f ≤ AbsoluteTolerance</c>, or when <c>MaxIterations</c> is reached. The
/// algorithm is a *local* optimizer — it converges to a nearby minimum, not necessarily a
/// global one. For multi-modal problems, run from several starting points.
/// </para>
/// </remarks>
public static class NelderMead
{
    // Lagarias et al. (1998) standard coefficients.
    private const double Reflection = 1.0;
    private const double Expansion = 2.0;
    private const double Contraction = 0.5;
    private const double Shrink = 0.5;

    /// <summary>
    /// Minimizes a scalar objective by Nelder-Mead simplex search starting from
    /// <paramref name="initialGuess"/>.
    /// </summary>
    public static MinimizationResult Minimize(
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> initialGuess,
        MinimizationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(objective);
        Guard.NotEmpty(initialGuess, nameof(initialGuess), "Initial guess must have at least one dimension.");
        Guard.AllFinite(initialGuess, nameof(initialGuess));

        MinimizationOptions opts = options ?? new MinimizationOptions();
        opts.Validate();

        int n = initialGuess.Length;
        double[][] simplex = BuildInitialSimplex(initialGuess, opts);
        double[] values = new double[n + 1];
        for (int i = 0; i <= n; i++)
        {
            values[i] = EvaluationHelpers.EvaluateScalar(objective,simplex[i]);
        }

        double[] centroid = new double[n];
        double[] reflected = new double[n];
        double[] expanded = new double[n];
        double[] contracted = new double[n];

        int iteration = 0;
        bool converged = false;

        while (iteration < opts.MaxIterations)
        {
            // Order: simplex[0] = best, simplex[n] = worst.
            SortSimplexByValue(simplex, values);

            if (values[n] - values[0] <= opts.AbsoluteTolerance)
            {
                converged = true;
                break;
            }

            ComputeCentroid(simplex, n, centroid);

            // Reflection.
            Combine(centroid, simplex[n], Reflection, reflected);
            double reflectedValue = EvaluationHelpers.EvaluateScalar(objective,reflected);
            iteration++;

            if (reflectedValue < values[0])
            {
                // Reflection improved on best: try expansion.
                Combine(centroid, reflected, -Expansion, expanded);
                double expandedValue = EvaluationHelpers.EvaluateScalar(objective,expanded);
                iteration++;

                if (expandedValue < reflectedValue)
                {
                    Array.Copy(expanded, simplex[n], n);
                    values[n] = expandedValue;
                }
                else
                {
                    Array.Copy(reflected, simplex[n], n);
                    values[n] = reflectedValue;
                }
            }
            else if (reflectedValue < values[n - 1])
            {
                // Reflection is acceptable.
                Array.Copy(reflected, simplex[n], n);
                values[n] = reflectedValue;
            }
            else
            {
                // Contraction.
                bool useReflectedAsTarget = reflectedValue < values[n];
                double[] contractionTarget = useReflectedAsTarget ? reflected : simplex[n];
                Combine(centroid, contractionTarget, -Contraction, contracted);
                double contractedValue = EvaluationHelpers.EvaluateScalar(objective,contracted);
                iteration++;

                double comparisonValue = useReflectedAsTarget ? reflectedValue : values[n];
                if (contractedValue < comparisonValue)
                {
                    Array.Copy(contracted, simplex[n], n);
                    values[n] = contractedValue;
                }
                else
                {
                    // Shrink toward the best vertex.
                    for (int i = 1; i <= n; i++)
                    {
                        for (int k = 0; k < n; k++)
                        {
                            simplex[i][k] = simplex[0][k] + Shrink * (simplex[i][k] - simplex[0][k]);
                        }
                        values[i] = EvaluationHelpers.EvaluateScalar(objective,simplex[i]);
                        iteration++;
                        if (iteration >= opts.MaxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }

        SortSimplexByValue(simplex, values);
        return new MinimizationResult(
            solution: simplex[0],
            finalValue: values[0],
            iterationCount: iteration,
            converged: converged);
    }

    private static double[][] BuildInitialSimplex(ReadOnlySpan<double> initialGuess, MinimizationOptions options)
    {
        int n = initialGuess.Length;
        var simplex = new double[n + 1][];
        simplex[0] = initialGuess.ToArray();

        for (int i = 1; i <= n; i++)
        {
            simplex[i] = initialGuess.ToArray();
            double perturbation = options.RelativePerturbation
                ? options.InitialSimplexEdge * Math.Max(Math.Abs(initialGuess[i - 1]), 1.0)
                : options.InitialSimplexEdge;
            simplex[i][i - 1] += perturbation;
        }
        return simplex;
    }

    private static void SortSimplexByValue(double[][] simplex, double[] values)
    {
        // Simple insertion sort — n+1 entries, n typically tiny (< 10).
        for (int i = 1; i < values.Length; i++)
        {
            double currentValue = values[i];
            double[] currentVertex = simplex[i];
            int j = i - 1;
            while (j >= 0 && values[j] > currentValue)
            {
                values[j + 1] = values[j];
                simplex[j + 1] = simplex[j];
                j--;
            }
            values[j + 1] = currentValue;
            simplex[j + 1] = currentVertex;
        }
    }

    private static void ComputeCentroid(double[][] simplex, int worstIndex, double[] destination)
    {
        int n = destination.Length;
        for (int k = 0; k < n; k++)
        {
            double sum = 0.0;
            for (int i = 0; i < worstIndex; i++)
            {
                sum += simplex[i][k];
            }
            destination[k] = sum / worstIndex;
        }
    }

    private static void Combine(double[] centroid, double[] reference, double weight, double[] destination)
    {
        // destination = centroid + weight * (centroid - reference)
        for (int k = 0; k < destination.Length; k++)
        {
            destination[k] = centroid[k] + weight * (centroid[k] - reference[k]);
        }
    }

}
