
namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Enumeration to represent the different methods of visualizing a matrix, specifically the row picture method and the column picture method.
/// These different methods provide different perspectives on how to understand the structure and properties of a matrix (and matrix muliplication), and they can be useful for different purposes in linear algebra and related fields.
/// </summary>
public enum MatrixPictureMethod
{
    /// <summary>
    /// The row picture method comes from the idea of visualizing a matrix as a collection of row vectors, where each row of the matrix is represented as a vector in a coordinate system.
    /// It results from the dot product of the a vector with the row vectors of the matrix, which gives a scalar value for each row.
    /// </summary>
    RowPicture,
    /// <summary>
    /// The column picture method comes from the idea of visualizing a matrix as a collection of column vectors, where each column of the matrix is represented as a vector in a coordinate system.
    /// It results from linear combinations of the column vectors of the matrix. 
    /// For example, if we have a matrix A with column vectors v1, v2, ..., vn, and a vector x with components x1, x2, ..., xn, then the product of the matrix A and the vector x can be expressed as a linear combination of the column vectors of A: A*x = x1*v1 + x2*v2 + ... + xn*vn.
    /// </summary>
    ColumnPicture
}
