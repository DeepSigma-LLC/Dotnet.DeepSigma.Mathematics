namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// A single linear constraint: <c>(Σ Terms[i].Coefficient · Terms[i].VariableName) Kind Bound</c>.
/// </summary>
/// <param name="Name">Constraint name. Must be unique within the program; used in error messages
/// and result diagnostics.</param>
/// <param name="Terms">Coefficient × variable pairs that make up the left-hand-side expression.</param>
/// <param name="Kind">Relational operator joining the LHS to the RHS bound.</param>
/// <param name="Bound">Right-hand-side scalar value.</param>
public sealed record LinearConstraint(
    string Name,
    IReadOnlyList<LinearTerm> Terms,
    ComparisonKind Kind,
    double Bound);
