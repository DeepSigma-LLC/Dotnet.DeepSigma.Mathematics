namespace DeepSigma.Mathematics.Optimization.Linear;

/// <summary>
/// A decision variable in a linear program: a name, a domain (continuous / integer / binary),
/// and lower / upper bounds.
/// </summary>
/// <param name="Name">Variable name. Must be unique within the program; used to identify the
/// variable in objective and constraint <see cref="LinearTerm"/> entries.</param>
/// <param name="Lower">Inclusive lower bound. Use <see cref="double.NegativeInfinity"/> for none.</param>
/// <param name="Upper">Inclusive upper bound. Use <see cref="double.PositiveInfinity"/> for none.</param>
/// <param name="Kind">Domain of the variable. Defaults to <see cref="VariableKind.Continuous"/>.
/// Setting any variable to <see cref="VariableKind.Integer"/> or <see cref="VariableKind.Binary"/>
/// turns the program into a MIP and forces use of a MIP solver (e.g., <c>Cbc</c>).</param>
public sealed record LinearVariable(
    string Name,
    double Lower,
    double Upper,
    VariableKind Kind = VariableKind.Continuous);
