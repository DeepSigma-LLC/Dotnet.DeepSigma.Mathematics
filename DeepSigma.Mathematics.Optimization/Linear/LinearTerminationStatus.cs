namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Reason an LP/MIP solve completed. Independent of which backend produced it.
/// </summary>
public enum LinearTerminationStatus
{
    /// <summary>An optimal solution was found.</summary>
    Optimal,

    /// <summary>The problem is infeasible — no assignment satisfies all constraints.</summary>
    Infeasible,

    /// <summary>The objective is unbounded in the direction of optimization.</summary>
    Unbounded,

    /// <summary>The solver hit the configured iteration limit before convergence.</summary>
    IterationLimit,

    /// <summary>The solver hit the configured time limit before convergence.</summary>
    TimeLimit,

    /// <summary>A feasible (but not provably optimal) solution was returned. Common with MIP heuristics.</summary>
    Feasible,

    /// <summary>The solver could not produce a definitive answer (numerical issues, internal error).</summary>
    Error,
}
