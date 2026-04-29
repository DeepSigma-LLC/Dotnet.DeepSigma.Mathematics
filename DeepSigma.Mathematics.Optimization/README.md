# DeepSigma.Mathematics.Optimization

Numerical optimization for the DeepSigma mathematics family. Single package covering:

- **Pure-BCL local optimizers** — Nelder-Mead simplex, Levenberg-Marquardt nonlinear least squares, Differential Evolution, Brent root finding
- **LP/MIP wrappers over Google OR-Tools** — Glop, PDLP, HiGHS, CBC
- **Constrained NLP wrappers over NLopt** — SLSQP, AUGLAG, COBYLA, MMA, ISRES

Consumer code never imports `Google.OrTools` or `NLoptNet` types directly — small static facades (`Glop.Solve`, `Slsqp.Solve`, etc.) hide the underlying APIs behind simple records.

## Contents

- [When to use this package](#when-to-use-this-package)
- [Choosing an algorithm](#choosing-an-algorithm)
- [Algorithm comparison](#algorithm-comparison)
- [Pure-BCL algorithms](#pure-bcl-algorithms)
  - [NelderMead](#neldermead)
  - [LevenbergMarquardt](#levenbergmarquardt)
  - [DifferentialEvolution](#differentialevolution)
  - [BrentRootFinder](#brentrootfinder)
- [LP / MIP via OR-Tools (`Linear` namespace)](#lp--mip-via-or-tools-linear-namespace)
- [Constrained NLP via NLopt (`Nonlinear` namespace)](#constrained-nlp-via-nlopt-nonlinear-namespace)
- [Common pitfalls across all algorithms](#common-pitfalls-across-all-algorithms)
- [Native binary footprint](#native-binary-footprint)
- [References](#references)

## When To Use This Package

Use this package whenever you need to find a parameter vector (or scalar) that minimizes a function, fits a model to data, zeros a residual, solves a linear program, or solves a constrained nonlinear program. Common consumers in this solution:

- **Calibration of stochastic-volatility models.** `Finance.Calibration` (planned) uses `LevenbergMarquardt` (with parameter transforms) or `Slsqp` (with native constraints) to fit Heston/Bates parameters to a market quote grid.
- **Implied volatility solvers.** `BlackScholes.ImpliedVolatility` already uses an inlined safeguarded Newton iteration; new implied-quantity solvers should reach for `BrentRootFinder` first.
- **Linear / mixed-integer optimization.** Portfolio construction with linear constraints, transportation, scheduling, knapsack-style selection. Reach for `Glop` (LP) or `Cbc` (MIP).
- **Diagnostic / sample fitting.** Any time you have residuals and want best-fit parameters with covariance — e.g. fitting a regression curve to Monte Carlo outputs.

The package is deliberately separate from the Monte Carlo namespace (`DeepSigma.Mathematics.Optimization`, not `DeepSigma.Mathematics.MonteCarlo.Optimization`). It is a sibling of `Core`. Don't add MC-specific helpers here.

## Choosing An Algorithm

```
What problem are you solving?
│
├── Find a root of a 1-D function on a sign-changing bracket
│   └── BrentRootFinder
│
├── Linear objective + linear constraints (LP)
│   ├── Mostly continuous, small/medium ──→ Glop
│   ├── Continuous, very large ────────────→ Pdlp
│   ├── Continuous, want modern fast ──────→ Highs
│   └── Has integer or binary variables ──→ Cbc
│
├── Nonlinear least squares (Σ rᵢ(x)²)
│   └── LevenbergMarquardt
│
├── Smooth nonlinear with constraints
│   ├── Equality + inequality, gradient-based ──→ Slsqp
│   ├── Inequality only, many constraints ──────→ Mma
│   ├── Stuck in local minima ──────────────────→ Isres (global)
│   ├── Noisy / non-smooth, derivative-free ────→ Cobyla
│   └── Want robust general-purpose ────────────→ AugLag
│
├── Smooth unconstrained, low dim, no derivatives
│   └── NelderMead
│
└── Multimodal global on a box, no derivatives
    └── DifferentialEvolution
```

If you're fitting a model to data and the objective decomposes as a sum of squared residuals, prefer **`LevenbergMarquardt`** — it's faster and gives you free covariance estimates. If your problem is genuinely multimodal, prefer a global method (`DifferentialEvolution` for unconstrained, `Isres` for constrained).

## Algorithm Comparison

| Aspect | NelderMead | LevenbergMarquardt | DifferentialEvolution | BrentRootFinder | Linear (Glop/HiGHS/PDLP/CBC) | Nonlinear (Slsqp/Cobyla/Mma/Isres/AugLag) |
| --- | --- | --- | --- | --- | --- | --- |
| **Problem** | min `f(x)` | min `Σ rᵢ²` | min `f(x)` on box | `f(x) = 0` | LP / MIP | Constrained NLP |
| **Constraints** | None | None | Box | Bracket | Linear | Nonlinear (eq + ineq) + bounds |
| **Locality** | Local | Local | Global | Bracketed | Optimal (LP) | Mostly local; Isres is global |
| **Derivatives** | No | Auto FD | No | No | N/A | Auto FD for gradient methods |
| **Free covariance** | No | Yes | No | N/A | No | No |
| **Native deps** | None | None | None | None | OR-Tools | NLopt |

## Pure-BCL Algorithms

These algorithms are pure managed code, no native dependencies. If you only use this section of the API, the OR-Tools and NLopt native binaries will still ship with the package but are never loaded.

### NelderMead

```csharp
using DeepSigma.Mathematics.Optimization;

MinimizationResult result = NelderMead.Minimize(
    objective: p =>
    {
        double a = 1.0 - p[0];
        double b = p[1] - p[0] * p[0];
        return a * a + 100.0 * b * b; // Rosenbrock
    },
    initialGuess: [-1.2, 1.0],
    new MinimizationOptions { MaxIterations = 1_000, AbsoluteTolerance = 1e-10 });
```

Maintains a simplex of `n+1` vertices and updates it via reflection / expansion / contraction / shrink each iteration (Lagarias–Reeds–Wright–Wright 1998). Derivative-free, robust, simple. Best for smooth low-dimensional (≤10 parameters) single-minimum problems. **Local only** — multi-start for multimodal landscapes.

### LevenbergMarquardt

```csharp
using DeepSigma.Mathematics.Optimization;

double[] xs = [0.0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0];
double[] ys = new double[xs.Length];
for (int i = 0; i < xs.Length; i++) ys[i] = 5.0 * Math.Exp(-0.7 * xs[i]);

LeastSquaresResult fit = LevenbergMarquardt.Solve(
    residuals: p =>
    {
        double[] r = new double[xs.Length];
        for (int i = 0; i < xs.Length; i++) r[i] = ys[i] - p[0] * Math.Exp(p[1] * xs[i]);
        return r;
    },
    initialGuess: [1.0, -0.1]);

if (fit.ParameterCovariance is not null)
{
    double seA = Math.Sqrt(fit.ParameterCovariance[0, 0]);
    double seB = Math.Sqrt(fit.ParameterCovariance[1, 1]);
}
```

Alternates between Gauss-Newton (fast near optimum) and steepest descent (safe far away) via additive identity damping. Forward-difference Jacobian, Cholesky-solved normal equations, asymptotic covariance estimate. Best for nonlinear regression. Reparameterize bounded parameters (`log`, `tanh`/sigmoid) — LM is unbounded.

### DifferentialEvolution

```csharp
using DeepSigma.Mathematics.Optimization;

MinimizationResult result = DifferentialEvolution.Minimize(
    objective: p =>
    {
        double s = 10.0 * p.Length;
        foreach (double x in p) s += x * x - 10.0 * Math.Cos(2.0 * Math.PI * x);
        return s; // Rastrigin — multimodal, global at origin
    },
    lowerBounds: [-5.12, -5.12],
    upperBounds: [5.12, 5.12],
    new DifferentialEvolutionOptions { Seed = 42, MaxGenerations = 2_000 });
```

Population-based stochastic optimizer (DE/rand/1/bin, Storn & Price 1997). Use for multimodal, rugged, or discontinuous objectives where local methods get trapped. **Box bounds required** — every parameter needs finite `lower < upper`. Pin `Seed` for reproducible runs.

### BrentRootFinder

```csharp
using DeepSigma.Mathematics.Optimization;

RootFindingResult result = BrentRootFinder.FindRoot(
    function: x => x * x - 2.0,
    lowerBound: 0.0,
    upperBound: 2.0,
    absoluteTolerance: 1e-12);
// result.Root ≈ √2
```

Brent-Dekker (1973): combines bisection (safe), secant, and inverse quadratic interpolation. Guaranteed convergence on a sign-changing bracket. 1-D only.

## LP / MIP via OR-Tools (`Linear` namespace)

```csharp
using DeepSigma.Mathematics.Optimization.Linear;

LinearProgram program = new(
    Objective: new LinearObjective(
        Terms: [new LinearTerm("bread", 0.50), new LinearTerm("milk", 0.30)],
        Sense: ObjectiveSense.Minimize),
    Variables: [
        new LinearVariable("bread", 0.0, double.PositiveInfinity),
        new LinearVariable("milk",  0.0, double.PositiveInfinity),
    ],
    Constraints: [
        new LinearConstraint("calories",
            [new LinearTerm("bread", 2.0), new LinearTerm("milk", 1.0)],
            ComparisonKind.GreaterOrEqual, 8.0),
        new LinearConstraint("vitamins",
            [new LinearTerm("bread", 1.0), new LinearTerm("milk", 3.0)],
            ComparisonKind.GreaterOrEqual, 6.0),
    ]);

LinearResult result = Glop.Solve(program);
// result.Status == LinearTerminationStatus.Optimal
// result.VariableValues["bread"] == 3.6, ["milk"] == 0.8, ObjectiveValue == 2.04
```

**Backends:** `Glop` (default simplex), `Pdlp` (large LP, first-order), `Highs` (modern simplex), `Cbc` (MIP via branch-and-cut). LP backends reject MIP problems with a clear error pointing at `Cbc`.

**Status semantics:**
- `Optimal` — solver proved this is optimal.
- `Feasible` — feasible solution returned but optimality unproved (common with MIP under time limit).
- `Infeasible` / `Unbounded` — problem has no solution.
- `IterationLimit` / `TimeLimit` — solver hit configured cap.
- `Error` — numerical or internal solver issue.

`VariableValues` and `ObjectiveValue` are only meaningful when status is `Optimal` or `Feasible`. **Always set `TimeLimit` for MIP solves** and check the status — proving optimality can take orders of magnitude longer than finding the optimum.

## Constrained NLP via NLopt (`Nonlinear` namespace)

```csharp
using DeepSigma.Mathematics.Optimization.Nonlinear;

// HS71: min x1*x4*(x1+x2+x3) + x3
//   s.t.  x1*x2*x3*x4 >= 25     (i.e., 25 - prod <= 0)
//         x1^2 + x2^2 + x3^2 + x4^2 = 40
//         1 <= xi <= 5
NlpConstraint productInequality = new(
    "product_ge_25",
    x => 25.0 - x[0] * x[1] * x[2] * x[3]);
NlpConstraint sumOfSquaresEquality = new(
    "sumsq_eq_40",
    x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3] - 40.0);

ConstrainedNlpResult result = Slsqp.Solve(
    objective: x => x[0] * x[3] * (x[0] + x[1] + x[2]) + x[2],
    initialGuess: [1.0, 5.0, 5.0, 1.0],
    inequalityConstraints: [productInequality],
    equalityConstraints: [sumOfSquaresEquality],
    lowerBounds: [1.0, 1.0, 1.0, 1.0],
    upperBounds: [5.0, 5.0, 5.0, 5.0]);
// result.FinalValue ≈ 17.014
// result.InequalityViolations and result.EqualityViolations ≈ 0
```

**Algorithms:**

| Wrapper | NLopt algorithm | When to use |
| --- | --- | --- |
| `Slsqp` | `LD_SLSQP` | Smooth objective + smooth constraints. The textbook choice for clean constrained NLP. Gradient-based. |
| `AugLag` | `AUGLAG` (configurable inner) | Augmented Lagrangian — robust on a wider class of problems than SLSQP, slower. Inner solver: L-BFGS / SLSQP / Nelder-Mead / COBYLA. |
| `Cobyla` | `LN_COBYLA` | Derivative-free constrained. Use when objective / constraints are noisy or non-smooth. Slower than gradient-based methods but more robust. |
| `Mma` | `LD_MMA` | Method of Moving Asymptotes. Strong on engineering problems with many inequality constraints. **Inequality-only.** |
| `Isres` | `GN_ISRES` | **Global** stochastic constrained NLP — escapes local minima. Bounds required. |

Gradients are auto-computed via forward differences when the algorithm asks for them; you don't have to provide derivatives. The wrapper computes `g_i = (f(x + h·e_i) − f(x)) / h` with `h = 1e-7 · max(|x_i|, 1)`.

**Constraint conventions:** inequalities are `g(x) ≤ 0`; equalities are `h(x) = 0`. The result's `InequalityViolations[i] = max(g_i(x*), 0)` and `EqualityViolations[j] = |h_j(x*)|` — both should be ≤ tolerance for a successfully constrained solve.

**Status mapping:** NLopt return codes map onto `NlpTerminationStatus` (`Success`, `FunctionToleranceReached`, `ParameterToleranceReached`, `IterationLimit`, `TimeLimit`, `RoundoffLimited`, `InfeasibleConstraint`, `Failure`).

**Determinism note:** NLoptNet 1.4.3 does not expose NLopt's `nlopt_srand`, so `ConstrainedNlpOptions.Seed` is currently ignored by `Isres`. Stochastic runs use the underlying NLopt RNG with whatever seed it has at process start.

## Common Pitfalls Across All Algorithms

### NaN handling

Every algorithm throws `InvalidOperationException` if your callback returns NaN. Wrap your objective to return a large finite penalty for infeasible regions instead.

### Parameter scaling

Forward-difference gradients (LM, NLP wrappers) and DE box-search are tuned for unit-magnitude parameters. If one parameter is `1e-9` and another is `1e6`, reparameterize (`log`, `logit`) before passing to the optimizer.

### Local vs global

NelderMead, LevenbergMarquardt, all NLopt algorithms except `Isres`, and the LP simplex methods are local. For multimodal problems use `DifferentialEvolution` (unconstrained) or `Isres` (constrained), or run multiple seeds.

### Reproducibility

NelderMead, LevenbergMarquardt, BrentRootFinder, and the LP backends are fully deterministic. DifferentialEvolution is deterministic with `Seed`. `Isres` is currently non-deterministic due to the NLoptNet binding limitation noted above.

### Iteration count semantics

`IterationCount` semantics differ across algorithms — see each algorithm's section. Apples-to-apples comparison across optimizers requires looking at total objective evaluations, which the result type does not directly expose for all algorithms.

## Native Binary Footprint

This package transitively depends on `Google.OrTools` (~50–80 MB native binaries) and `NLoptNet` (~5–10 MB native). RID-aware deployment (`runtimes/win-x64/native/`, etc.) means publishing for a specific platform picks up only that platform's binaries. If you only use the pure-BCL algorithms (NelderMead, LevenbergMarquardt, DifferentialEvolution, BrentRootFinder), the native libraries ship with the package but are never loaded into your process — they cost disk, not memory.

## References

- Lagarias, J. C., Reeds, J. A., Wright, M. H., Wright, P. E. (1998). *Convergence Properties of the Nelder–Mead Simplex Method in Low Dimensions.* SIAM Journal on Optimization 9, 112–147.
- Marquardt, D. W. (1963). *An Algorithm for Least-Squares Estimation of Nonlinear Parameters.* SIAM Journal 11, 431–441.
- Moré, J. J. (1978). *The Levenberg–Marquardt Algorithm: Implementation and Theory.* Numerical Analysis, Lecture Notes in Mathematics 630.
- Storn, R., Price, K. (1997). *Differential Evolution — A Simple and Efficient Heuristic for Global Optimization over Continuous Spaces.* Journal of Global Optimization 11, 341–359.
- Brent, R. P. (1973). *Algorithms for Minimization Without Derivatives.* Prentice-Hall.
- Press, W. H., Teukolsky, S. A., Vetterling, W. T., Flannery, B. P. (2007). *Numerical Recipes,* 3rd edition. Cambridge University Press.
- Kraft, D. (1988). *A software package for sequential quadratic programming.* DFVLR-FB 88-28. (SLSQP.)
- Powell, M. J. D. (1994). *A direct search optimization method that models the objective and constraint functions by linear interpolation.* (COBYLA.)
- Svanberg, K. (2002). *A class of globally convergent optimization methods based on conservative convex separable approximations.* SIAM Journal on Optimization 12, 555–573. (MMA.)
- Runarsson, T. P., Yao, X. (2005). *Search biases in constrained evolutionary optimization.* IEEE Transactions on Systems, Man, and Cybernetics, 35(2), 233–243. (ISRES.)
- Hestenes, M. R. (1969). *Multiplier and gradient methods.* Journal of Optimization Theory and Applications 4, 303–320. (Augmented Lagrangian.)
- Google OR-Tools documentation: <https://developers.google.com/optimization>
- NLopt documentation: <https://nlopt.readthedocs.io/>
