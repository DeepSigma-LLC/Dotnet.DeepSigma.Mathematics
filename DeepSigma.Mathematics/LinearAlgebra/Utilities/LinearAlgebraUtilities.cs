using DeepSigma.Core.Extensions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System.Globalization;
using System.Numerics.Tensors;
using System.Text;

namespace DeepSigma.Mathematics.LinearAlgebra.Utilities;

/// <summary>
/// Utility class for linear algebra operations.
/// </summary>
public static class LinearAlgebraUtilities
{
    /// <summary>
    /// Calculates the cosine similarity between two vectors.
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static float GetVectorCosineSimilarty(float[] vector1, float[] vector2) => TensorPrimitives.CosineSimilarity(vector1, vector2);

    /// <summary>
    /// Length of a vector.
    /// </summary>
    /// <param name="vector">The vector whose length is to be calculated.</param>
    /// <returns>The length of the vector.</returns>
    public static float Length(float[] vector)
    {
        float sum = vector.Sum();
        return (float)Math.Sqrt(sum);
    }

    /// <summary>
    /// Creates a dense matrix of decimal values from the specified two-dimensional array.
    /// </summary>
    /// <remarks>The returned matrix is dense, meaning all elements are stored explicitly. The input array is
    /// not copied defensively; changes to the array after calling this method do not affect the matrix.</remarks>
    /// <param name="data">A two-dimensional array of decimal values that provides the elements for the matrix. This parameter must not be
    /// null.</param>
    /// <returns>
    /// A Matrix&lt;double&gt; instance containing the values from the specified array.
    /// </returns>
    public static Matrix<double> CreateDenseMatrix(double[,] data) => Matrix<double>.Build.DenseOfArray(data);

    /// <summary>
    /// Creates a dense identity matrix of the specified order.
    /// </summary>
    /// <param name="order">The order of the identity matrix. Must be greater than or equal to 0.</param>
    /// <returns>A dense identity matrix of the specified order.</returns>
    public static Matrix<double> CreateDenseIdentityMatrix(int order) => Matrix<double>.Build.DenseIdentity(order);

    /// <summary>
    /// Creates a sparse matrix with the specified number of rows and columns, setting the diagonal elements to a given
    /// value.
    /// </summary>
    /// <remarks>This method is useful for efficiently creating large diagonal matrices where most elements
    /// are zero, minimizing memory usage.</remarks>
    /// <param name="row">The number of rows in the resulting matrix. Must be greater than or equal to 0.</param>
    /// <param name="column">The number of columns in the resulting matrix. Must be greater than or equal to 0.</param>
    /// <param name="value">The value to assign to each diagonal element. Defaults to 1.0.</param>
    /// <returns>A sparse matrix of the specified dimensions with the diagonal elements set to the specified value and all other
    /// elements set to zero.</returns>
    public static Matrix<double> CreateSparseDiagonalMatrix(int row, int column, double value = 1.0) => Matrix<double>.Build.SparseDiagonal(row, column, value);

    /// <summary>
    /// Creates a dense matrix with the specified number of rows and columns, setting the diagonal elements to a given value.
    /// </summary>
    /// <param name="row">The number of rows in the resulting matrix. Must be greater than or equal to 0.</param>
    /// <param name="column">The number of columns in the resulting matrix. Must be greater than or equal to 0.</param>
    /// <param name="value">The value to assign to each diagonal element. Defaults to 1.0.</param>
    /// <returns>A dense matrix of the specified dimensions with the diagonal elements set to the specified value and all other
    /// elements set to zero.</returns>
    public static Matrix<double> CreateDenseDiagonalMatrix(int row, int column, double value = 1.0) => Matrix<double>.Build.DenseDiagonal(row, column, value);

    /// <summary>
    /// Computes the sample covariance matrix for the given data matrix, where each column represents a variable and each row represents an observation.
    /// The sample covariance is calculated by centering the data (subtracting the mean of each column from the corresponding elements) and then computing the covariance using the formula: Cov(X) = (1 / (n - 1)) * (X_centered^T * X_centered), where n is the number of observations (rows) in the data matrix. 
    /// This method requires at least two observations to compute a valid covariance matrix; otherwise, it throws an ArgumentException.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Matrix<double> ComputeCovarianceMatrix_Sample(Matrix<double> data)
    {
        int row_count = data.RowCount;
        int column_count = data.ColumnCount;

        if (row_count < 2)
            throw new ArgumentException("At least two observations are required.");

        var means = Vector<double>.Build.Dense(column_count);
        for (int j = 0; j < column_count; j++)
            means[j] = data.Column(j).Average();

        var centered = data.Clone();
        for (int i = 0; i < row_count; i++)
        {
            for (int j = 0; j < column_count; j++)
            {
                centered[i, j] -= means[j];
            }
        }

        return centered.TransposeThisAndMultiply(centered) / (row_count - 1.0);
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficients for all pairs of columns in the specified data matrix.
    /// </summary>
    /// <remarks>The resulting correlation matrix is symmetric. Pearson's correlation coefficient measures the
    /// linear relationship between two variables, with values ranging from -1 (perfect negative correlation) to 1
    /// (perfect positive correlation).</remarks>
    /// <param name="data">A matrix where each column represents a variable and each row represents an observation. The matrix must not be
    /// null and must contain at least two columns.</param>
    /// <returns>A square matrix containing the correlation coefficients, where each element at position [i, j] represents the
    /// Pearson correlation between the i-th and j-th columns of the input data. The diagonal elements are always 1.</returns>
    public static Matrix<double> ComputeCorrelationMatrix(Matrix<double> data)
    {
        int column_count= data.ColumnCount;
        var correlation = Matrix<double>.Build.Dense(column_count, column_count); // Square matrix to hold the correlation coefficients

        for (int i = 0; i < column_count; i++)
        {
            double[] colI = data.Column(i).ToArray();

            for (int j = i; j < column_count; j++)
            {
                double[] colJ = data.Column(j).ToArray();
                double corr = Correlation.Pearson(colI, colJ);

                correlation[i, j] = corr;
                correlation[j, i] = corr;
            }
        }
        return correlation;
    }


    /// <summary>
    /// To reduced row echolon form by manual implementation of Gauss-Jordan elimination.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public static Matrix<double> ToRref(Matrix<double> matrix, double tolerance = 1e-10)
    {
        var rref = matrix.Clone();
        int row_count = rref.RowCount;
        int column_count = rref.ColumnCount;

        int lead = 0;

        for (int r = 0; r < row_count; r++)
        {
            if (lead >= column_count)
                break;

            int i = r;
            while (Math.Abs(rref[i, lead]) < tolerance)
            {
                i++;
                if (i == row_count)
                {
                    i = r;
                    lead++;
                    if (lead == column_count) return rref;
                }
            }

            if (i != r)
            {
                var temp = rref.Row(i);
                rref.SetRow(i, rref.Row(r));
                rref.SetRow(r, temp);
            }

            double pivot = rref[r, lead];
            if (Math.Abs(pivot) > tolerance)
                rref.SetRow(r, rref.Row(r) / pivot);

            for (int row = 0; row < row_count; row++)
            {
                if (row == r) continue;

                double factor = rref[row, lead];
                if (Math.Abs(factor) > tolerance)
                    rref.SetRow(row, rref.Row(row) - factor * rref.Row(r));
            }
            lead++;
        }

        return rref.Map(x => Math.Abs(x) < tolerance ? 0.0 : x);
    }


    /// <summary>
    /// Gets matrix summary.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static string GetMatrixSummary(Matrix<double> matrix)
    {
        return matrix.ToMatrixString(upperRows: 10,
        lowerRows: 0,
        leftColumns: 10,
        rightColumns: 0,
        horizontalEllipsis: "...",
        verticalEllipsis: "...",
        diagonalEllipsis: "...",
        columnSeparator: " | ",
        rowSeparator: Environment.NewLine,
        formatValue: x => x.ToString("F3", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts the specified matrix of double-precision values to a comma-separated values (CSV) formatted string.
    /// </summary>
    /// <remarks>Each row of the matrix is written as a line in the CSV output, with values separated by
    /// commas. The method does not perform any escaping for special characters in headers or values.</remarks>
    /// <param name="matrix">The matrix of double values to convert to CSV format.</param>
    /// <param name="headers">An optional array of strings representing the column headers. If provided, the headers are included as the first
    /// line of the output.</param>
    /// <param name="format">A standard or custom numeric format string that defines how each double value is formatted. The default is "G"
    /// (general format).</param>
    /// <param name="provider">An optional format provider that supplies culture-specific formatting information. If null, the invariant
    /// culture is used.</param>
    /// <returns>A string containing the CSV representation of the matrix, including headers if specified.</returns>
    public static string ToCsv(Matrix<double> matrix, string[]? headers = null, string format = "G", IFormatProvider? provider = null)
    {
        provider ??= CultureInfo.InvariantCulture;
        StringBuilder sb = new();

        if (headers != null)
        {
            sb.AppendLine(string.Join(",", headers));
        }

        for (int i = 0; i < matrix.RowCount; i++)
        {
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                if (j > 0)
                {
                    sb.Append(',');
                }
                sb.Append(matrix[i, j].ToString(format, provider));
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

}
