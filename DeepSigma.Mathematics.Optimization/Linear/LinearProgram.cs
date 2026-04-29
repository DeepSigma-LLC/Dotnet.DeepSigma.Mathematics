namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// Complete description of a linear program (LP) or mixed-integer program (MIP): the
/// objective to minimize / maximize, the decision variables, and the linear constraints.
/// </summary>
/// <param name="Objective">Linear objective expression and sense.</param>
/// <param name="Variables">Decision variables. Names must be unique. The presence of any
/// integer or binary variable turns the program into a MIP.</param>
/// <param name="Constraints">Linear constraints. May be empty.</param>
/// <remarks>
/// This record carries data only — solving is done by the static <c>Solve</c> method on a
/// backend class (<c>Glop</c>, <c>Pdlp</c>, <c>Highs</c>, or <c>Cbc</c>). The record itself
/// has no dependency on Google OR-Tools and can be constructed, serialized, or inspected
/// without loading any native libraries.
/// </remarks>
public sealed record LinearProgram(
    LinearObjective Objective,
    IReadOnlyList<LinearVariable> Variables,
    IReadOnlyList<LinearConstraint> Constraints);
