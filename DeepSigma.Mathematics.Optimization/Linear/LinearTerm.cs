namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// A single coefficient × variable pair in a linear expression
/// (e.g., <c>3.5 * "production"</c>).
/// </summary>
/// <param name="VariableName">Name of the variable being scaled. Must match an entry in
/// <see cref="LinearProgram.Variables"/>.</param>
/// <param name="Coefficient">Scalar multiplying the variable in the expression.</param>
public sealed record LinearTerm(string VariableName, double Coefficient);
