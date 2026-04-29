namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Relational operator joining a constraint's left-hand side (linear combination of variables)
/// to its right-hand side (constant bound).
/// </summary>
public enum ComparisonKind
{
    /// <summary>Constraint of the form <c>Σ aᵢ xᵢ ≤ b</c>.</summary>
    LessOrEqual,

    /// <summary>Constraint of the form <c>Σ aᵢ xᵢ ≥ b</c>.</summary>
    GreaterOrEqual,

    /// <summary>Constraint of the form <c>Σ aᵢ xᵢ = b</c>.</summary>
    Equal,
}
