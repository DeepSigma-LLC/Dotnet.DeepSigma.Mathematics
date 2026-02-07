
namespace DeepSigma.Mathematics.LinearAlgebra;

/// <summary>
/// Struct to represent the dimensions of a matrix, including the number of rows and columns.
/// </summary>
public readonly struct MatrixDimension
{
    /// <summary>
    /// The number of rows in the matrix, often referred to as m in an m x n matrix, where m is the number of rows and n is the number of columns.
    /// </summary>
    public int RowCount { get; }

    /// <summary>
    /// The number of columns in the matrix, often referred to as n in an m x n matrix, where m is the number of rows and n is the number of columns.
    /// </summary>
    public int ColumnCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MatrixDimension"/> struct with the specified row and column counts.
    /// </summary>
    /// <param name="rowCount"></param>
    /// <param name="columnCount"></param>
    public MatrixDimension(int rowCount, int columnCount)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    /// <summary>
    /// Decomposes the matrix dimension into its row and column counts, allowing for easy access to these values when working with matrices.
    /// </summary>
    public void Deconstruct(out int rowCount, out int columnCount)
    {
        rowCount = RowCount;
        columnCount = ColumnCount;
    }


    /// <summary>
    /// Returns a string representation of the matrix dimensions in the format "n X m: RowCount, X ColumnCount)", where RowCount is the number of rows and ColumnCount is the number of columns in the matrix.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"n X m (RowCount X ColumnCount): {RowCount}, X {ColumnCount})";
    }
}
