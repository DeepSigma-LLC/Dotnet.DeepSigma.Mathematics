using Google.OrTools.LinearSolver;

namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// HiGHS — modern open-source LP/MIP solver, integrated into OR-Tools. Strong dual simplex
/// performance, and one of the best free LP solvers as of the late 2020s. Often a good
/// first choice for continuous LP when speed and accuracy both matter.
/// </summary>
/// <remarks>
/// This wrapper drives HiGHS's LP path. Continuous variables only — for MIP problems use
/// <see cref="Cbc"/>. Throws <see cref="ArgumentException"/> if integer / binary variables
/// are supplied.
/// </remarks>
public static class Highs
{
    /// <summary>
    /// Solves <paramref name="program"/> using HiGHS (LP mode).
    /// </summary>
    public static LinearResult Solve(LinearProgram program, LinearProgrammingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(program);
        Glop.EnsureContinuousOnly(program, nameof(Highs));

        (Solver solver, Dictionary<string, Variable> variables) = OrToolsTranslator.Build("HIGHS_LINEAR_PROGRAMMING", program, options);
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
