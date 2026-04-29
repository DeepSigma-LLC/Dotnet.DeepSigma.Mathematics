using Google.OrTools.LinearSolver;

namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// PDLP — Google's primal-dual hybrid gradient first-order LP solver. Designed for large
/// LPs where simplex's per-iteration matrix work doesn't fit in memory or simplex stalls
/// on huge degenerate problems.
/// </summary>
/// <remarks>
/// First-order convergence; expect lower precision than simplex without tightening
/// tolerances. PDLP is LP-only. For continuous LPs of typical scale, prefer
/// <see cref="Glop"/> or <see cref="Highs"/> — PDLP's advantage shows up at millions of
/// variables. Throws <see cref="ArgumentException"/> for integer / binary variables.
/// </remarks>
public static class Pdlp
{
    /// <summary>
    /// Solves <paramref name="program"/> using PDLP.
    /// </summary>
    public static LinearResult Solve(LinearProgram program, LinearProgrammingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(program);
        Glop.EnsureContinuousOnly(program, nameof(Pdlp));

        (Solver solver, Dictionary<string, Variable> variables) = OrToolsTranslator.Build("PDLP", program, options);
        try
        {
            return OrToolsTranslator.Solve(solver, variables);
        }
        finally
        {
            solver.Dispose();
        }
    }
}
