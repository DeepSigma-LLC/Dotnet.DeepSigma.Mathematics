namespace DeepSigma.Mathematics.Optimization.Tests;

public sealed class LevenbergMarquardtTests
{
    [Fact]
    public void Solve_RecoversLinearRegressionExactly()
    {
        // y = 2 + 3 x on a noiseless grid. LM should drive SSR to ~0 and recover (2, 3).
        double[] xs = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0];
        double[] ys = new double[xs.Length];
        for (int i = 0; i < xs.Length; i++) ys[i] = 2.0 + 3.0 * xs[i];

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double a = parameters[0];
                double b = parameters[1];
                double[] r = new double[xs.Length];
                for (int i = 0; i < xs.Length; i++) r[i] = ys[i] - (a + b * xs[i]);
                return r;
            },
            initialGuess: [0.0, 0.0]);

        Assert.True(result.Converged, $"Did not converge in {result.IterationCount} iterations.");
        Assert.Equal(2.0, result.Solution[0], precision: 8);
        Assert.Equal(3.0, result.Solution[1], precision: 8);
        Assert.True(result.FinalSumOfSquares < 1e-16);
    }

    [Fact]
    public void Solve_FitsExponentialDecay()
    {
        // y = 5 exp(-0.7 t) on a noiseless grid. Classic nonlinear problem; LM is the
        // textbook tool for it.
        double trueA = 5.0;
        double trueB = -0.7;
        double[] ts = [0.0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 4.0, 5.0];
        double[] ys = new double[ts.Length];
        for (int i = 0; i < ts.Length; i++) ys[i] = trueA * Math.Exp(trueB * ts[i]);

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double a = parameters[0];
                double b = parameters[1];
                double[] r = new double[ts.Length];
                for (int i = 0; i < ts.Length; i++) r[i] = ys[i] - a * Math.Exp(b * ts[i]);
                return r;
            },
            initialGuess: [1.0, -0.1]);

        Assert.True(result.Converged, $"Did not converge in {result.IterationCount} iterations.");
        Assert.Equal(trueA, result.Solution[0], precision: 6);
        Assert.Equal(trueB, result.Solution[1], precision: 6);
        Assert.True(result.FinalSumOfSquares < 1e-12);
    }

    [Fact]
    public void Solve_FitsQuadratic()
    {
        // y = 1 - 2 x + 0.5 x^2 noiseless. Three parameters, linear-in-parameters but exercised
        // through the same nonlinear-LM path.
        double[] xs = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0];
        double[] ys = new double[xs.Length];
        for (int i = 0; i < xs.Length; i++) ys[i] = 1.0 - 2.0 * xs[i] + 0.5 * xs[i] * xs[i];

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double a = parameters[0];
                double b = parameters[1];
                double c = parameters[2];
                double[] r = new double[xs.Length];
                for (int i = 0; i < xs.Length; i++)
                {
                    double x = xs[i];
                    r[i] = ys[i] - (a + b * x + c * x * x);
                }
                return r;
            },
            initialGuess: [0.0, 0.0, 0.0]);

        Assert.True(result.Converged);
        Assert.Equal(1.0, result.Solution[0], precision: 8);
        Assert.Equal(-2.0, result.Solution[1], precision: 8);
        Assert.Equal(0.5, result.Solution[2], precision: 8);
    }

    [Fact]
    public void Solve_FitsGaussianPeak()
    {
        // y = h exp(-((x - c)/w)^2) — three-parameter Gaussian, the classic NIST-style nonlinear
        // regression test. Noiseless data; LM should recover (h, c, w) cleanly.
        double trueH = 2.5;
        double trueC = 1.2;
        double trueW = 0.4;
        double[] xs = new double[21];
        double[] ys = new double[21];
        for (int i = 0; i < xs.Length; i++)
        {
            xs[i] = i * 0.1;
            double z = (xs[i] - trueC) / trueW;
            ys[i] = trueH * Math.Exp(-z * z);
        }

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double h = parameters[0];
                double c = parameters[1];
                double w = parameters[2];
                double[] r = new double[xs.Length];
                for (int i = 0; i < xs.Length; i++)
                {
                    double z = (xs[i] - c) / w;
                    r[i] = ys[i] - h * Math.Exp(-z * z);
                }
                return r;
            },
            initialGuess: [1.0, 1.0, 0.5]);

        Assert.True(result.Converged, $"Did not converge in {result.IterationCount} iterations.");
        Assert.Equal(trueH, result.Solution[0], precision: 5);
        Assert.Equal(trueC, result.Solution[1], precision: 5);
        Assert.Equal(trueW, result.Solution[2], precision: 5);
    }

    [Fact]
    public void Solve_ProducesFiniteCovarianceWhenNoisy()
    {
        // Linear regression with noise — the asymptotic covariance should be finite and
        // positive on the diagonal. We don't pin exact values (would require a separate
        // closed-form check); this is a sanity test that the matrix is well-formed.
        double[] xs = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0];
        double[] noise = [0.10, -0.08, 0.05, -0.12, 0.07, 0.02, -0.04, 0.09, -0.06, 0.11];
        double[] ys = new double[xs.Length];
        for (int i = 0; i < xs.Length; i++) ys[i] = 2.0 + 3.0 * xs[i] + noise[i];

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double a = parameters[0];
                double b = parameters[1];
                double[] r = new double[xs.Length];
                for (int i = 0; i < xs.Length; i++) r[i] = ys[i] - (a + b * xs[i]);
                return r;
            },
            initialGuess: [0.0, 0.0]);

        Assert.True(result.Converged);
        Assert.NotNull(result.ParameterCovariance);
        Assert.Equal(2, result.ParameterCovariance!.GetLength(0));
        Assert.Equal(2, result.ParameterCovariance.GetLength(1));
        Assert.True(result.ParameterCovariance[0, 0] > 0.0 && double.IsFinite(result.ParameterCovariance[0, 0]));
        Assert.True(result.ParameterCovariance[1, 1] > 0.0 && double.IsFinite(result.ParameterCovariance[1, 1]));
        // Symmetry.
        Assert.Equal(result.ParameterCovariance[0, 1], result.ParameterCovariance[1, 0], precision: 12);
    }

    [Fact]
    public void Solve_OmitsCovarianceWhenDisabled()
    {
        double[] xs = [0.0, 1.0, 2.0];
        double[] ys = [1.0, 2.0, 3.0];

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double a = parameters[0];
                double b = parameters[1];
                double[] r = new double[xs.Length];
                for (int i = 0; i < xs.Length; i++) r[i] = ys[i] - (a + b * xs[i]);
                return r;
            },
            initialGuess: [0.0, 0.0],
            new LeastSquaresOptions { ComputeParameterCovariance = false });

        Assert.True(result.Converged);
        Assert.Null(result.ParameterCovariance);
    }

    [Fact]
    public void Solve_TerminatesAtMaxIterationsWhenToleranceTooTight()
    {
        // Force a budget-exhausted run with an absurd tolerance.
        double[] ts = [0.0, 0.5, 1.0, 1.5, 2.0];
        double[] ys = new double[ts.Length];
        for (int i = 0; i < ts.Length; i++) ys[i] = 5.0 * Math.Exp(-0.7 * ts[i]);

        LeastSquaresResult result = LevenbergMarquardt.Solve(
            residuals: parameters =>
            {
                double a = parameters[0];
                double b = parameters[1];
                double[] r = new double[ts.Length];
                for (int i = 0; i < ts.Length; i++) r[i] = ys[i] - a * Math.Exp(b * ts[i]);
                return r;
            },
            initialGuess: [1.0, -0.1],
            new LeastSquaresOptions { MaxIterations = 3, Tolerance = 1e-30 });

        Assert.False(result.Converged);
        Assert.True(result.IterationCount >= 3);
    }

    [Fact]
    public void Solve_ValidatesInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LevenbergMarquardt.Solve(null!, [0.0, 0.0]));

        Assert.Throws<ArgumentException>(() =>
            LevenbergMarquardt.Solve(p => [0.0], ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Solve_RejectsNonFiniteInitialGuess()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LevenbergMarquardt.Solve(p => [0.0], [double.NaN, 1.0]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LevenbergMarquardt.Solve(p => [0.0], [1.0, double.PositiveInfinity]));
    }

    [Fact]
    public void Solve_ThrowsWhenResidualsReturnsNaN()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LevenbergMarquardt.Solve(p => [double.NaN], [0.0, 0.0]));
    }

    [Fact]
    public void Solve_ThrowsWhenResidualsReturnsEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LevenbergMarquardt.Solve(p => Array.Empty<double>(), [0.0, 0.0]));
    }

    [Fact]
    public void Options_RejectsInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LevenbergMarquardt.Solve(
            p => [0.0], [0.0],
            new LeastSquaresOptions { MaxIterations = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LevenbergMarquardt.Solve(
            p => [0.0], [0.0],
            new LeastSquaresOptions { Tolerance = -1e-6 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LevenbergMarquardt.Solve(
            p => [0.0], [0.0],
            new LeastSquaresOptions { FiniteDifferenceStep = 0.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LevenbergMarquardt.Solve(
            p => [0.0], [0.0],
            new LeastSquaresOptions { InitialDamping = 0.0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LevenbergMarquardt.Solve(
            p => [0.0], [0.0],
            new LeastSquaresOptions { DampingDecreaseFactor = 1.5 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => LevenbergMarquardt.Solve(
            p => [0.0], [0.0],
            new LeastSquaresOptions { DampingIncreaseFactor = 0.5 }));
    }
}
