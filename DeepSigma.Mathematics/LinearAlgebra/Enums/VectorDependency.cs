
namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Enumeration to represent the linear dependency status of a set of vectors.
/// </summary>
public enum VectorDependency
{
    /// <summary>
    /// The vectors are linearly independent, meaning that no vector in the set can be expressed as a linear combination of the others. 
    /// In this case, the vectors span a space of dimension equal to the number of vectors in the set.
    /// </summary>
    Independent,
    /// <summary>
    /// The vectors are linearly dependent, meaning that at least one vector in the set can be expressed as a linear combination of the others. 
    /// In this case, the vectors do not span a space of dimension equal to the number of vectors in the set, and there is redundancy among the vectors.
    /// </summary>
    Dependent
}
