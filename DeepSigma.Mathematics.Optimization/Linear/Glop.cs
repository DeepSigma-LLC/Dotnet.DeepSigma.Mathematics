using Google.OrTools.LinearSolver;

namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Glop — Google's open-source primal/dual simplex LP solver. Default for small to medium
/// continuous LPs. Robust, fast on well-conditioned problems, and the in-house solver
/// driving Google's internal LP workloads.
/// </summary>
/// <remarks>
/// Glop is LP-only. If the program contains any
/// <see cref="VariableKind.Integer"/> or <see cref="VariableKind.Binary"/> variables, this
/// throws <see cref="ArgumentException"/>; use <see cref="Cbc"/> for MIP.
/// </remarks>
public static class Glop
{
    /// <summary>
    /// Solves <paramref name="program"/> using Glop.
    /// </summary>
    public static LinearResult Solve(LinearProgram program, LinearProgrammingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(program);
        EnsureContinuousOnly(program, nameof(Glop));

        (Solver solver, Dictionary<string, Variable> variables) = OrToolsTranslator.Build("GLOP", program, options);
        try
        {
            return OrToolsTranslator.Solve(solver, variables);
        }
        finally
        {
            solver.Dispose();
        }
    }

    internal static void EnsureContinuousOnly(LinearProgram program, string backendName)
    {
        foreach (LinearVariable variable in program.Variables)
        {
            if (variable.Kind != VariableKind.Continuous)
            {
                throw new ArgumentException(
                    $"{backendName} is an LP-only backend; variable '{variable.Name}' has Kind={variable.Kind}. Use Cbc for MIP problems.",
                    nameof(program));
            }
        }
    }
}
