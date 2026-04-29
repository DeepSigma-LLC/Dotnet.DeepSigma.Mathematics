namespace DeepSigma.Mathematics.Optimization.Tests;

public sealed class DifferentialEvolutionTests
{
    [Fact]
    public void Minimize_FindsSphereMinimum()
    {
        // Sphere f(x) = Σ x_i². Trivial unimodal benchmark; DE should find it easily.
        MinimizationResult result = DifferentialEvolution.Minimize(
            objective: parameters =>
            {
                double sum = 0.0;
                foreach (double x in parameters) sum += x * x;
                return sum;
            },
            lowerBounds: [-5.0, -5.0, -5.0, -5.0],
            upperBounds: [5.0, 5.0, 5.0, 5.0],
            new DifferentialEvolutionOptions { Seed = 42, MaxGenerations = 500, AbsoluteTolerance = 1e-10 });

        Assert.True(result.Converged, $"Did not converge in {result.IterationCount} generations.");
        for (int i = 0; i < result.Solution.Length; i++)
        {
            Assert.Equal(0.0, result.Solution[i], precision: 4);
        }
        Assert.True(result.FinalValue < 1e-8);
    }

    [Fact]
    public void Minimize_FindsRosenbrockMinimum()
    {
        // 2D Rosenbrock: global min at (1, 1) with f = 0. DE should find it given enough
        // generations on a generous bracket.
        MinimizationResult result = DifferentialEvolution.Minimize(
            objective: parameters =>
            {
                double a = 1.0 - parameters[0];
                double b = parameters[1] - parameters[0] * parameters[0];
                return a * a + 100.0 * b * b;
            },
            lowerBounds: [-5.0, -5.0],
            upperBounds: [5.0, 5.0],
            new DifferentialEvolutionOptions
            {
                Seed = 7,
                MaxGenerations = 2_000,
                AbsoluteTolerance = 1e-10,
                PopulationSize = 40,
            });

        Assert.True(result.Converged, $"Did not converge in {result.IterationCount} generations.");
        Assert.Equal(1.0, result.Solution[0], precision: 3);
        Assert.Equal(1.0, result.Solution[1], precision: 3);
        Assert.True(result.FinalValue < 1e-6);
    }

    [Fact]
    public void Minimize_FindsRastriginGlobalOptimum()
    {
        // Rastrigin is the canonical multimodal benchmark — many local minima, global at origin.
        // f(x) = 10 n + Σ (x_i² − 10 cos(2π x_i)). Local methods get stuck; DE finds it.
        MinimizationResult result = DifferentialEvolution.Minimize(
            objective: parameters =>
            {
                double sum = 10.0 * parameters.Length;
                foreach (double x in parameters)
                {
                    sum += x * x - 10.0 * Math.Cos(2.0 * Math.PI * x);
                }
                return sum;
            },
            lowerBounds: [-5.12, -5.12],
            upperBounds: [5.12, 5.12],
            new DifferentialEvolutionOptions
            {
                Seed = 123,
                MaxGenerations = 2_000,
                PopulationSize = 50,
                AbsoluteTolerance = 1e-8,
            });

        Assert.True(result.FinalValue < 1e-3, $"Final value {result.FinalValue:G} not near zero.");
        for (int i = 0; i < result.Solution.Length; i++)
        {
            Assert.True(Math.Abs(result.Solution[i]) < 1e-2, $"Coordinate {i} = {result.Solution[i]:G} not near zero.");
        }
    }

    [Fact]
    public void Minimize_FindsAckleyGlobalOptimum()
    {
        // Ackley function: another classical multimodal benchmark. Global min at origin with f = 0.
        MinimizationResult result = DifferentialEvolution.Minimize(
            objective: parameters =>
            {
                double a = 20.0;
                double b = 0.2;
                double c = 2.0 * Math.PI;
                int d = parameters.Length;
                double s1 = 0.0;
                double s2 = 0.0;
                foreach (double x in parameters)
                {
                    s1 += x * x;
                    s2 += Math.Cos(c * x);
                }
                return -a * Math.Exp(-b * Math.Sqrt(s1 / d)) - Math.Exp(s2 / d) + a + Math.E;
            },
            lowerBounds: [-32.768, -32.768],
            upperBounds: [32.768, 32.768],
            new DifferentialEvolutionOptions
            {
                Seed = 99,
                MaxGenerations = 1_500,
                PopulationSize = 50,
                AbsoluteTolerance = 1e-8,
            });

        Assert.True(result.FinalValue < 1e-3, $"Final value {result.FinalValue:G} not near zero.");
        Assert.True(Math.Abs(result.Solution[0]) < 1e-2);
        Assert.True(Math.Abs(result.Solution[1]) < 1e-2);
    }

    [Fact]
    public void Minimize_IsRepeatableUnderSameSeed()
    {
        // Pinning the seed must reproduce the run bit-for-bit. This is critical for diagnosing
        // calibration regressions later.
        DifferentialEvolutionOptions opts = new()
        {
            Seed = 2026,
            MaxGenerations = 50,
            PopulationSize = 20,
            AbsoluteTolerance = 1e-30, // forbid early termination so both runs do identical work
        };
        Func<ReadOnlySpan<double>, double> objective = parameters =>
        {
            double sum = 0.0;
            foreach (double x in parameters) sum += x * x;
            return sum;
        };

        MinimizationResult run1 = DifferentialEvolution.Minimize(objective, [-5.0, -5.0, -5.0], [5.0, 5.0, 5.0], opts);
        MinimizationResult run2 = DifferentialEvolution.Minimize(objective, [-5.0, -5.0, -5.0], [5.0, 5.0, 5.0], opts);

        Assert.Equal(run1.FinalValue, run2.FinalValue);
        for (int i = 0; i < run1.Solution.Length; i++)
        {
            Assert.Equal(run1.Solution[i], run2.Solution[i]);
        }
    }

    [Fact]
    public void Minimize_StaysWithinBounds()
    {
        // Even with a function that "wants" to push outside the box (linear, no minimum in
        // the interior), the returned solution must stay inside the feasible region.
        double[] lower = [1.0, 2.0];
        double[] upper = [3.0, 4.0];
        MinimizationResult result = DifferentialEvolution.Minimize(
            objective: parameters => parameters[0] + parameters[1], // monotone — minimum on lower corner
            lowerBounds: lower,
            upperBounds: upper,
            new DifferentialEvolutionOptions { Seed = 1, MaxGenerations = 200 });

        Assert.InRange(result.Solution[0], lower[0], upper[0]);
        Assert.InRange(result.Solution[1], lower[1], upper[1]);
        // Lower-corner verification — DE on a linear objective should drive toward (1, 2).
        Assert.Equal(1.0, result.Solution[0], precision: 2);
        Assert.Equal(2.0, result.Solution[1], precision: 2);
    }

    [Fact]
    public void Minimize_TerminatesAtMaxGenerationsWhenToleranceTooTight()
    {
        MinimizationResult result = DifferentialEvolution.Minimize(
            objective: parameters =>
            {
                double sum = 0.0;
                foreach (double x in parameters) sum += x * x;
                return sum;
            },
            lowerBounds: [-5.0, -5.0],
            upperBounds: [5.0, 5.0],
            new DifferentialEvolutionOptions { Seed = 0, MaxGenerations = 5, AbsoluteTolerance = 1e-30, PopulationSize = 10 });

        Assert.False(result.Converged);
        Assert.Equal(5, result.IterationCount);
    }

    [Fact]
    public void Minimize_ValidatesInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DifferentialEvolution.Minimize(null!, [0.0], [1.0]));

        Assert.Throws<ArgumentException>(() =>
            DifferentialEvolution.Minimize(p => 0.0, ReadOnlySpan<double>.Empty, ReadOnlySpan<double>.Empty));

        Assert.Throws<ArgumentException>(() =>
            DifferentialEvolution.Minimize(p => 0.0, [0.0, 0.0], [1.0]));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DifferentialEvolution.Minimize(p => 0.0, [0.0], [double.NaN]));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DifferentialEvolution.Minimize(p => 0.0, [1.0], [1.0])); // upper not strictly greater
    }

    [Fact]
    public void Minimize_ThrowsWhenObjectiveReturnsNaN()
    {
        Assert.Throws<InvalidOperationException>(() =>
            DifferentialEvolution.Minimize(p => double.NaN, [-1.0], [1.0],
                new DifferentialEvolutionOptions { Seed = 0 }));
    }

    [Fact]
    public void Options_RejectsInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEvolution.Minimize(
            p => 0.0, [-1.0], [1.0],
            new DifferentialEvolutionOptions { MaxGenerations = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEvolution.Minimize(
            p => 0.0, [-1.0], [1.0],
            new DifferentialEvolutionOptions { AbsoluteTolerance = 0.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEvolution.Minimize(
            p => 0.0, [-1.0], [1.0],
            new DifferentialEvolutionOptions { DifferentialWeight = 0.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEvolution.Minimize(
            p => 0.0, [-1.0], [1.0],
            new DifferentialEvolutionOptions { CrossoverProbability = 1.5 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => DifferentialEvolution.Minimize(
            p => 0.0, [-1.0], [1.0],
            new DifferentialEvolutionOptions { PopulationSize = 3 }));
    }
}
