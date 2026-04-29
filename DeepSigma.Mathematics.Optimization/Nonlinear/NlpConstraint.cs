namespace DeepSigma.Mathematics.Optimization.Nonlinear;

/// <summary>
/// A nonlinear constraint <c>g(x)</c> evaluated at a parameter vector. The interpretation
/// depends on the constraint list it appears in: when passed as an inequality, the constraint
/// is satisfied iff <c>g(x) ≤ 0</c>; when passed as an equality, satisfied iff <c>g(x) = 0</c>.
/// </summary>
/// <param name="Name">Human-readable name. Used in result diagnostics — does not affect the
/// solver's behavior. Must be unique within a single constraint list.</param>
/// <param name="Function">Constraint function. Returns a finite real number; the solver
/// will reject NaN / ±∞ values during the search.</param>
public sealed record NlpConstraint(string Name, Func<ReadOnlySpan<double>, double> Function);
