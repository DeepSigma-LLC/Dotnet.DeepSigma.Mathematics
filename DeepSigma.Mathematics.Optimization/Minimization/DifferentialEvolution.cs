namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Differential Evolution global minimizer (DE/rand/1/bin scheme).
/// </summary>
/// <remarks>
/// DE is a population-based stochastic optimizer that maintains <c>NP</c> candidate vectors
/// and produces a trial vector for each by combining three randomly-chosen distinct members
/// of the population (Storn &amp; Price, 1997). It does not require derivatives and copes well
/// with rugged, multimodal, or discontinuous objectives where gradient methods like
/// Levenberg-Marquardt or local methods like Nelder-Mead fail.
/// <para>
/// The implementation follows DE/rand/1/bin: the donor is <c>a + F · (b − c)</c> for three
/// distinct random members <c>a, b, c</c>, the trial vector inherits each component from the
/// donor with probability <c>CR</c> (and at least one component is forced from the donor to
/// avoid degenerate copies of the target), and the trial replaces the target if it has a
/// strictly lower objective value. Out-of-bounds trial components are reflected back into the
/// search box.
/// </para>
/// <para>
/// Population initialization is uniform on the search box. The RNG is seedable via
/// <see cref="DifferentialEvolutionOptions.Seed"/>; pinning the seed produces bit-identical
/// runs.
/// </para>
/// </remarks>
public static class DifferentialEvolution
{
    /// <summary>
    /// Minimizes <paramref name="objective"/> on the box defined by
    /// <paramref name="lowerBounds"/> and <paramref name="upperBounds"/>.
    /// </summary>
    /// <param name="objective">Function to minimize.</param>
    /// <param name="lowerBounds">Per-dimension lower bounds (inclusive).</param>
    /// <param name="upperBounds">Per-dimension upper bounds (inclusive). Must have the same
    /// length as <paramref name="lowerBounds"/>, with <c>upper[i] &gt; lower[i]</c>.</param>
    /// <param name="options">Optional DE configuration.</param>
    public static MinimizationResult Minimize(
        Func<ReadOnlySpan<double>, double> objective,
        ReadOnlySpan<double> lowerBounds,
        ReadOnlySpan<double> upperBounds,
        DifferentialEvolutionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(objective);
        Guard.NotEmpty(lowerBounds, nameof(lowerBounds), "Bounds must have at least one dimension.");
        if (lowerBounds.Length != upperBounds.Length)
        {
            throw new ArgumentException(
                $"Lower and upper bounds must have the same length (got {lowerBounds.Length} and {upperBounds.Length}).",
                nameof(upperBounds));
        }
        Guard.AllFinite(lowerBounds, nameof(lowerBounds));
        Guard.AllFinite(upperBounds, nameof(upperBounds));
        for (int i = 0; i < lowerBounds.Length; i++)
        {
            if (upperBounds[i] <= lowerBounds[i])
            {
                throw new ArgumentOutOfRangeException(
                    nameof(upperBounds),
                    $"Upper bound {upperBounds[i]:G} at index {i} must be strictly greater than lower bound {lowerBounds[i]:G}.");
            }
        }

        DifferentialEvolutionOptions opts = options ?? new DifferentialEvolutionOptions();
        opts.Validate();

        int n = lowerBounds.Length;
        int np = opts.ResolvePopulationSize(n);
        double f = opts.DifferentialWeight;
        double cr = opts.CrossoverProbability;
        Random rng = opts.Seed is int seed ? new Random(seed) : new Random();

        double[] lower = lowerBounds.ToArray();
        double[] upper = upperBounds.ToArray();

        double[][] population = new double[np][];
        double[] values = new double[np];
        for (int i = 0; i < np; i++)
        {
            double[] member = new double[n];
            for (int j = 0; j < n; j++)
            {
                member[j] = lower[j] + rng.NextDouble() * (upper[j] - lower[j]);
            }
            population[i] = member;
            values[i] = EvaluationHelpers.EvaluateScalar(objective,member);
        }

        double[] trial = new double[n];
        bool converged = false;
        int generation = 0;

        for (; generation < opts.MaxGenerations; generation++)
        {
            for (int i = 0; i < np; i++)
            {
                int a = PickDistinct(rng, np, i, -1, -1);
                int b = PickDistinct(rng, np, i, a, -1);
                int c = PickDistinct(rng, np, i, a, b);

                int forcedIndex = rng.Next(n);
                for (int j = 0; j < n; j++)
                {
                    bool useDonor = j == forcedIndex || rng.NextDouble() < cr;
                    double candidate = useDonor
                        ? population[a][j] + f * (population[b][j] - population[c][j])
                        : population[i][j];
                    trial[j] = ReflectIntoBounds(candidate, lower[j], upper[j]);
                }

                double trialValue = EvaluationHelpers.EvaluateScalar(objective,trial);
                if (trialValue < values[i])
                {
                    Array.Copy(trial, population[i], n);
                    values[i] = trialValue;
                }
            }

            double minValue = values[0];
            double maxValue = values[0];
            for (int i = 1; i < np; i++)
            {
                if (values[i] < minValue) minValue = values[i];
                if (values[i] > maxValue) maxValue = values[i];
            }
            if (maxValue - minValue <= opts.AbsoluteTolerance)
            {
                converged = true;
                generation++;
                break;
            }
        }

        int bestIndex = 0;
        for (int i = 1; i < np; i++)
        {
            if (values[i] < values[bestIndex]) bestIndex = i;
        }

        return new MinimizationResult(
            solution: population[bestIndex],
            finalValue: values[bestIndex],
            iterationCount: generation,
            converged: converged);
    }

    private static int PickDistinct(Random rng, int np, int excludeA, int excludeB, int excludeC)
    {
        while (true)
        {
            int candidate = rng.Next(np);
            if (candidate != excludeA && candidate != excludeB && candidate != excludeC)
            {
                return candidate;
            }
        }
    }

    private static double ReflectIntoBounds(double value, double lower, double upper)
    {
        // One reflection per side handles the typical DE overshoot. For the rare extreme step
        // that would still be out of bounds after one reflection, clamp to the violated side
        // so the trial is always feasible.
        if (value < lower)
        {
            double reflected = lower + (lower - value);
            return reflected > upper ? lower : reflected;
        }
        if (value > upper)
        {
            double reflected = upper - (value - upper);
            return reflected < lower ? upper : reflected;
        }
        return value;
    }
}
