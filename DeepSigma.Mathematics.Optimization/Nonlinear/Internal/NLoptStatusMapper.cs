using NLoptNet;

namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// Translates NLopt's native return codes into our <see cref="NlpTerminationStatus"/> enum.
/// </summary>
internal static class NLoptStatusMapper
{
    public static NlpTerminationStatus Map(NloptResult raw) => raw switch
    {
        NloptResult.SUCCESS => NlpTerminationStatus.Success,
        NloptResult.STOPVAL_REACHED => NlpTerminationStatus.FunctionToleranceReached,
        NloptResult.FTOL_REACHED => NlpTerminationStatus.FunctionToleranceReached,
        NloptResult.XTOL_REACHED => NlpTerminationStatus.ParameterToleranceReached,
        NloptResult.MAXEVAL_REACHED => NlpTerminationStatus.IterationLimit,
        NloptResult.MAXTIME_REACHED => NlpTerminationStatus.TimeLimit,
        NloptResult.ROUNDOFF_LIMITED => NlpTerminationStatus.RoundoffLimited,
        NloptResult.FORCED_STOP => NlpTerminationStatus.Failure,
        NloptResult.INVALID_ARGS => NlpTerminationStatus.Failure,
        NloptResult.OUT_OF_MEMORY => NlpTerminationStatus.Failure,
        NloptResult.FAILURE => NlpTerminationStatus.Failure,
        _ => NlpTerminationStatus.Failure,
    };
}
