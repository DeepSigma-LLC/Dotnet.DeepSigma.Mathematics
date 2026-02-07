
namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Enumeration to represent different types of linear combinations of vectors, including the sum of two vectors, the difference of two vectors, a vector in the direction of one vector, and the zero vector.
/// Note: A linear combination of vectors is an expression that involves multiplying each vector by a scalar and then adding the results together.
/// </summary>
/// <remarks>
/// Furthermore, linear combinations of vectors are fundamental in linear algebra and are used to describe various operations and relationships between vectors, such as span, linear independence, and vector spaces.
/// Also, linear combinations fill the plane defined by the vectors, meaning that any vector in the plane can be expressed as a linear combination of the vectors that define the plane.
/// </remarks>
public enum LinearCombinationsOfVectors
{
    /// <summary>
    /// The sum of two vectors is a linear combination of the vectors where each vector is multiplied by the scalar 1 and then added together.
    /// For example, if we have two vectors v and w, their sum can be expressed as 1*v + 1*w, which is a linear combination of the vectors v and w.
    /// </summary>
    SumOfVectors,
    /// <summary>
    /// The difference of two vectors is a linear combination of the vectors where one vector is multiplied by the scalar 1 and the other vector is multiplied by the scalar -1, and then the two resulting vectors are added together.
    /// For example, if we have two vectors v and w, their difference can be expressed as 1*v + (-1)*w, which is a linear combination of the vectors v and w.
    /// </summary>
    DifferenceOfVectors,
    /// <summary>
    /// A vector in the direction of one vector is a linear combination of that vector where the scalar is a positive real number.
    /// For example, if we have two vectors v and w, a vector in the direction of v can be expressed as t*v + 0*w, where t is a positive real number. 
    /// This means that the resulting vector will have the same direction as v but may have a different magnitude depending on the value of t.
    /// </summary>
    VectorInDirectionOfOneVector,
    /// <summary>
    /// The zero vector is a linear combination of any vector, including itself, where all the coefficients in the linear combination are zero.
    /// For example, 0 can be expressed as 0*v1 + 0*v2 + ... + 0*vn, where v1, v2, ..., vn are any vectors in the vector space.
    /// </summary>
    ZeroVector,
}
