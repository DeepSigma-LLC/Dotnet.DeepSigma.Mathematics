
namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Enumeration to represent different types of matrices.
/// </summary>
public enum MatrixType
{
    /// <summary>
    /// The identity matrix is a square matrix with ones on the main diagonal and zeros elsewhere. 
    /// It serves as the multiplicative identity in matrix multiplication, meaning that any matrix multiplied by the identity matrix will yield the original matrix unchanged.
    /// </summary>
    Identity,
    /// <summary>
    /// A diagonal matrix is a square matrix where all the entries outside the main diagonal are zero.
    /// </summary>
    Diagonal,
    /// <summary>
    /// An exchange matrix is a square matrix that has ones on the anti-diagonal (the diagonal that runs from the top-right corner to the bottom-left corner) and zeros elsewhere.
    /// It is used to reverse the order of elements in a vector or to swap the rows or columns of a matrix when multiplied by it. 
    /// Note: The exchange matrix is also known as the "permutation matrix" or "reversal matrix" because it permutes the order of elements in a vector or the rows/columns of a matrix when multiplied by it.
    /// Squaring the exchange matrix yields the identity matrix, which means that applying the exchange operation twice will return the original order of elements or rows/columns. 
    /// Said another way, exchanging the exchange matrix will return the original order of elements or rows/columns (aka the identity matrix).
    /// </summary>
    Exchange,
    /// <summary>
    /// A symmetric matrix is a square matrix that is equal to its transpose, meaning that the entries across the main diagonal are the same.
    /// </summary>
    Symmetric,
    /// <summary>
    /// An upper triangular matrix is a square matrix where all the entries below the main diagonal are zero.
    /// </summary>
    UpperTriangular,
    /// <summary>
    /// A lower triangular matrix is a square matrix where all the entries above the main diagonal are zero.
    /// </summary>
    LowerTriangular,
    /// <summary>
    /// A sparse matrix is a matrix that contains a large number of zero entries, which can be efficiently stored and manipulated using specialized data structures and algorithms.
    /// </summary>
    Sparse,
    /// <summary>
    /// A dense matrix is a matrix that contains a large number of non-zero entries, which can be stored and manipulated using standard data structures and algorithms.
    /// </summary>
    Dense
}
