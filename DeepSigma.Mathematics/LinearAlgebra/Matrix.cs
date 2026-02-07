using System.Numerics;

namespace DeepSigma.Mathematics.LinearAlgebra;

/// <summary>
/// Represents a mathematical matrix composed of vectors of a numeric type. The matrix is generic and supports
/// operations on elements of any type that implements <see cref="INumber{T}"/>.
/// </summary>
/// <remarks>Each vector in the matrix represents a row, and all vectors must have the same dimension to form a
/// valid matrix. The matrix provides access to its row and column dimensions, enabling mathematical operations and
/// transformations on numeric data.</remarks>
/// <typeparam name="T">The numeric type of the matrix elements. Must implement <see cref="INumber{T}"/> to support arithmetic operations.</typeparam>
public class Matrix<T>
        where T : INumber<T>
{
    private readonly Vector<T>[] _components;

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix{T}"/> class with the specified components. 
    /// The number of vectors provided determines the dimension of the matrix.
    /// Each vector represents a row of the matrix, and all vectors must have the same dimension to form a valid matrix.
    /// </summary>
    /// <param name="components"></param>
    public Matrix(params Vector<T>[] components)
    {
        _components = components;
    }

    /// <summary>
    /// Provides indexed access to the vectors (rows) of the matrix.
    /// The index is zero-based, meaning that the first vector is accessed with index 0, the second with index 1, and so on.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector<T> this[int index] => _components[index];

    /// <summary>
    /// Gets the number of vectors (columns) in the matrix, which corresponds to its dimension. 
    /// Often referred to as n in an m x n matrix, where m is the number of rows and n is the number of columns.
    /// </summary>
    public int ColumnCount => _components.Length;

    /// <summary>
    /// Gets the number of components in each vector (rows) of the matrix, which corresponds to its dimension.
    /// Often referred to as m in an m x n matrix, where m is the number of rows and n is the number of columns.
    /// </summary>
    public int RowCount => _components[0].Dimension;

    /// <summary>
    /// Gets the rank of the matrix, which is the maximum number of linearly independent row or column vectors in the matrix.
    /// </summary>
    public int Rank => throw new NotImplementedException(); // Need a systematic way to determine the rank of a matrix, which involves checking for linear independence among the rows or columns.

    /// <summary>
    /// Adds two matrices of the same dimensions and returns a new matrix as the result.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        if (a.ColumnCount != b.ColumnCount || a.RowCount != b.RowCount)
            throw new InvalidOperationException("Matrices must have the same dimensions for addition.");

        Vector<T>[] resultComponents = new Vector<T>[a.ColumnCount];
        for (int i = 0; i < a.ColumnCount; i++)
        {
            resultComponents[i] = a._components[i] + b._components[i];
        }
        return new Matrix<T>(resultComponents);
    }

    /// <summary>
    /// Subtracts one matrix from another, provided they have the same dimensions, and returns a new matrix as the result.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
    {
        if (a.ColumnCount != b.ColumnCount || a.RowCount != b.RowCount)
            throw new InvalidOperationException("Matrices must have the same dimensions for subtraction.");
        Vector<T>[] resultComponents = new Vector<T>[a.ColumnCount];
        for (int i = 0; i < a.ColumnCount; i++)
        {
            resultComponents[i] = a._components[i] - b._components[i];
        }
        return new Matrix<T>(resultComponents);
    }

    /// <summary>
    /// Multiplies a matrix by a scalar value, scaling each element of the matrix by the specified scalar.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static Matrix<T> operator *(Matrix<T> a, T scalar)
    {
        Vector<T>[] resultComponents = new Vector<T>[a.ColumnCount];
        for (int i = 0; i < a.ColumnCount; i++)
        {
            T[] scaledComponents = new T[a.RowCount];
            for (int j = 0; j < a.RowCount; j++)
            {
                scaledComponents[j] = a[i][j] * scalar;
            }
            resultComponents[i] = new Vector<T>(scaledComponents);
        }
        return new Matrix<T>(resultComponents);
    }

    /// <summary>
    /// Multiplies a matrix by a scalar value, scaling each element of the matrix by the specified scalar.
    /// </summary>
    /// <param name="scalar"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Matrix<T> operator *(T scalar, Matrix<T> a) => a * scalar;


    /// <summary>
    /// Multiplies two matrices together, provided that the number of rows in the first matrix equals the number of columns in the second matrix.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
    {
        if (a.RowCount != b.ColumnCount)
            throw new InvalidOperationException("The number of rows in the first matrix must equal the number of columns in the second matrix for multiplication.");

        Vector<T>[] resultComponents = new Vector<T>[a.ColumnCount];
        for (int i = 0; i < a.ColumnCount; i++)
        {
            T[] rowComponents = new T[b.RowCount];
            for (int j = 0; j < b.RowCount; j++)
            {
                T sum = T.Zero;
                for (int k = 0; k < a.RowCount; k++)
                {
                    sum += a[i][k] * b[k][j];
                }
                rowComponents[j] = sum;
            }
            resultComponents[i] = new Vector<T>(rowComponents);
        }
        return new Matrix<T>(resultComponents);
    }

    /// <summary>
    /// Creates and returns an identity matrix of the specified dimensions. An identity matrix is a square matrix with ones on the main diagonal and zeros elsewhere.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    public static Matrix<T> BuildIdentityMatrix(int row, int columns)
    {
        Vector<T>[] components = new Vector<T>[row];
        for (int i = 0; i < row; i++)
        {
            T[] rowComponents = new T[columns];
            for (int j = 0; j < columns; j++)
            {
                rowComponents[j] = i == j ? T.One : T.Zero;
            }
            components[i] = new Vector<T>(rowComponents);
        }
        return new Matrix<T>(components);
    }

    /// <summary>
    /// Transposes the given matrix, returning a new matrix where the rows and columns are swapped. 
    /// The resulting matrix will have dimensions that are the inverse of the original matrix (i.e., if the original matrix is m x n, the transposed matrix will be n x m).
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Matrix<T> Transpose(Matrix<T> matrix)
    {
        throw new NotImplementedException();
    }

 
}
