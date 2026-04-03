
using System.Diagnostics.Metrics;
using System.Runtime.Intrinsics.X86;

namespace DeepSigma.Mathematics.LinearAlgebra.Enums;

/// <summary>
/// Matrix density type enumeration, indicating whether a matrix is sparse or dense.
/// </summary>
public enum MatrixDensityType
{
    /// <summary>
    /// Sparse matrix, where a significant portion of the entries are zero. 
    /// Sparse matrices are often used in applications like graph theory, machine learning, and scientific computing to save memory and computational resources.
    /// Use when working with very large matrices where > 90 - 99 % of entries are zero, or when managing large-scale data in RAG(Retrieval-Augmented Generation) systems for improved performance.
    /// </summary>
    Sparse,
    /// <summary>
    /// Dense matrix, where most of the entries are non-zero. 
    /// Dense matrices are typically used when the matrix is small enough to fit in memory or when a high percentage of entries are non-zero.
    /// Use when a high percentage of entries (often >20%) are non-zero, or if the matrix size is small enough to fit in memory easily.
    /// </summary>
    Dense
}
