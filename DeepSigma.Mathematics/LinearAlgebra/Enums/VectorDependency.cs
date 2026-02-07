
namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Enumeration to represent the linear dependency status of a set of vectors.
/// </summary>
public enum VectorDependency
{
    /// <summary>
    /// The vectors are linearly independent, meaning that no vector in the set can be expressed as a linear combination of the others. 
    /// In this case, the vectors span a space of dimension equal to the number of vectors in the set.
    /// A common example of linear independence is when the vectors are not parallel and do not lie along the same line in the vector space, which means that they cannot be expressed as scalar multiples of each other.
    /// </summary>
    LinearlyIndependent,
    /// <summary>
    /// The vectors are linearly dependent, meaning that at least one vector in the set can be expressed as a linear combination of the others. 
    /// In this case, the vectors do not span a space of dimension equal to the number of vectors in the set, and there is redundancy among the vectors.
    /// A common example of linear dependence is when one vector is a scalar multiple of another vector, which means that the two vectors are parallel and lie along the same line in the vector space.
    /// </summary>
    LinerallyDependent
}
