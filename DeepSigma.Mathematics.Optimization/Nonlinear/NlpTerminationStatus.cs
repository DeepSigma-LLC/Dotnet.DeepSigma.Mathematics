namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Reason an NLP solve completed. Independent of which algorithm produced it.
/// </summary>
public enum NlpTerminationStatus
{
    /// <summary>The solver reported success without invoking a more specific tolerance test.</summary>
    Success,

    /// <summary>The configured function-value tolerance was reached.</summary>
    FunctionToleranceReached,

    /// <summary>The configured parameter-step tolerance was reached.</summary>
    ParameterToleranceReached,

    /// <summary>The solver hit the configured iteration cap before convergence.</summary>
    IterationLimit,

    /// <summary>The solver hit the configured wall-clock time limit before convergence.</summary>
    TimeLimit,

    /// <summary>Progress stalled because of floating-point round-off (further accepted steps
    /// would be smaller than machine precision).</summary>
    RoundoffLimited,

    /// <summary>The solver could not satisfy a constraint within working precision.</summary>
    InfeasibleConstraint,

    /// <summary>The solver returned an error code that does not map to any specific reason.</summary>
    Failure,
}
