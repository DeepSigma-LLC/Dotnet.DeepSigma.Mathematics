namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Domain of a decision variable in a linear program.
/// </summary>
public enum VariableKind
{
    /// <summary>Continuous real-valued variable. Default.</summary>
    Continuous,

    /// <summary>Integer-valued variable. Forces a MIP solver.</summary>
    Integer,

    /// <summary>Binary variable taking values in <c>{0, 1}</c>. Forces a MIP solver.</summary>
    Binary,
}
