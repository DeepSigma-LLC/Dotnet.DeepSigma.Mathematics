using Google.OrTools.LinearSolver;

namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// CBC (Coin-or Branch and Cut) — open-source mixed-integer programming solver. Use this
/// when the program contains <see cref="VariableKind.Integer"/> or
/// <see cref="VariableKind.Binary"/> variables. Handles pure LP too, but for continuous-only
/// problems <see cref="Glop"/> or <see cref="Highs"/> is faster.
/// </summary>
/// <remarks>
/// CBC's MIP search uses branch-and-bound plus cutting planes. On large MIPs solve time
/// can be unbounded — set a meaningful <see cref="LinearProgrammingOptions.TimeLimit"/>
/// and check the returned <see cref="LinearTerminationStatus"/> to handle the
/// <see cref="LinearTerminationStatus.Feasible"/> (best-so-far) outcome.
/// </remarks>
public static class Cbc
{
    /// <summary>
    /// Solves <paramref name="program"/> using CBC.
    /// </summary>
    public static LinearResult Solve(LinearProgram program, LinearProgrammingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(program);

        (Solver solver, Dictionary<string, Variable> variables) = OrToolsTranslator.Build("CBC", program, options);
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
