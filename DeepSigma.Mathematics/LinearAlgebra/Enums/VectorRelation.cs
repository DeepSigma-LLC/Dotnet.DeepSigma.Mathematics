
namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Enumeration to represent the relationship between two vectors, specifically whether they are parallel, perpendicular, or orthogonal to each other.
/// </summary>
public enum VectorRelation
{
    /// <summary>
    /// Two vectors are parallel if they have the same or opposite direction, meaning that one vector can be expressed as a scalar multiple of the other.
    /// </summary>
    Parallel,
    /// <summary>
    /// Two vectors are perpendicular if the angle between them is 90 degrees, and they intersect at a right angle. 
    /// In this case, the dot product of the two vectors is zero, indicating that they are orthogonal to each other. 
    /// Note: In some contexts, the terms "perpendicular" and "orthogonal" are used interchangeably to describe this relationship between vectors, but in others, "orthogonal" may be used more broadly to refer to any two vectors that are at right angles to each other, regardless of their lengths or directions.
    /// </summary>
    Perpendicular,
    /// <summary>
    /// Two vectors are orthogonal if their dot product is zero, which means that they are at right angles to each other in the vector space. 
    /// Furthermore, orthogonal vectors can be of any length, and they do not necessarily perpendicular to each other. 
    /// Intersecting at a right angle is a specific case of orthogonality, but orthogonal vectors can also be non-perpendicular if they have different lengths or directions.
    /// </summary>
    Orthogonal,
    /// <summary>
    /// Two vectors are intersecting if they share a common point in space, meaning that they have at least one point in common.
    /// </summary>
    Intersecting,
    /// <summary>
    /// Two vectors are skewed if they do not lie in the same plane and do not intersect, meaning that they are not parallel, perpendicular, or orthogonal to each other.
    /// </summary>
    Skewed
}
