namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Linear objective expression: <c>Σ Terms[i].Coefficient · Terms[i].VariableName + Constant</c>,
/// to be minimized or maximized according to <paramref name="Sense"/>.
/// </summary>
/// <param name="Terms">Coefficient × variable pairs that make up the linear expression.
/// Empty is allowed — useful for pure-feasibility problems where the user only cares about
/// the constraints.</param>
/// <param name="Sense">Whether to minimize or maximize the expression.</param>
/// <param name="Constant">Additive constant. Does not affect the optimal point but is added
/// to the reported objective value.</param>
public sealed record LinearObjective(
    IReadOnlyList<LinearTerm> Terms,
    ObjectiveSense Sense,
    double Constant = 0.0);
