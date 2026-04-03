using DeepSigma.Mathematics.LinearAlgebra.Utilities;
using Xunit;

namespace DeepSigma.Mathematics.Tests;

public class LinearAlgebraTests
{
    [Fact]
    public void Test_MatrixBuild()
    {
        double[,] data = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 7.0, 8.0, 9.0 }
        };
        var matrix = LinearAlgebraUtilities.CreateDenseMatrix(data);

        Assert.Equal(3, matrix.RowCount);
        Assert.Equal(3, matrix.ColumnCount);
        Assert.Equal(1.0, matrix[0, 0]);
        Assert.Equal(5.0, matrix[1, 1]);
        Assert.Equal(9.0, matrix[2, 2]);
    }


    [Fact]
    public void Test_CovarianceMatrix()
    {
        double[,] data = new double[,]
        {
            { 1.0, 1.0 },
            { 3.0, -2 },
            { -1.0, -1.0 }
        };
        var matrix = LinearAlgebraUtilities.CreateDenseMatrix(data);
        var covarianceMatrix = LinearAlgebraUtilities.ComputeCovarianceMatrix_Sample(matrix);

        Assert.Equal(2, covarianceMatrix.RowCount);
        Assert.Equal(2, covarianceMatrix.ColumnCount);
        Assert.Equal(4, covarianceMatrix[0, 0], 9); // Variance of the first variable
        Assert.Equal(-1, covarianceMatrix[1, 0], 9); // Covariance between the two variables, which should be 2/3 for this perfectly correlated data
        Assert.Equal(7.0/3.0, covarianceMatrix[1, 1], 9); // Variance of the second variable
        Assert.Equal(-1, covarianceMatrix[0, 1], 9); // Covariance between the two variables, which should be 2/3 for this perfectly correlated data
    }

    [Fact]
    public void Test_CorrelationMatrix()
    {
        double[,] data = new double[,]
        {
            { 1.0, -1.0 },
            { 3.0, -3.0 },
            { 5.0, -5.0 },
            { 1.0, -1.0 },
            {-4, 4 },
        };
        var matrix = LinearAlgebraUtilities.CreateDenseMatrix(data);
        var correlationMatrix = LinearAlgebraUtilities.ComputeCorrelationMatrix(matrix);
        Assert.Equal(2, correlationMatrix.RowCount);
        Assert.Equal(2, correlationMatrix.ColumnCount);
        Assert.Equal(1.0, correlationMatrix[0, 0], 9); // Correlation of a variable with itself is always 1
        Assert.Equal(-1.0, correlationMatrix[1, 0], 9); // Since the data is perfectly negatively correlated, the correlation should be -1
        Assert.Equal(1.0, correlationMatrix[1, 1], 9); // Correlation of a variable with itself is always 1
        Assert.Equal(-1.0, correlationMatrix[0, 1], 9); // Since the data is perfectly negatively correlated, the correlation should be -1
    }

    [Fact]
    public void Test_MatrixPrinting()
    {
        double[,] data = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 7.0, 8.0, 9.0 }
        };
        var matrix = LinearAlgebraUtilities.CreateDenseMatrix(data);
        string matrixString = LinearAlgebraUtilities.GetMatrixSummary(matrix);
        Assert.Contains("1.000 | 2.000 | 3.000\r\n4.000 | 5.000 | 6.000\r\n7.000 | 8.000 | 9.000\r\n", matrixString);
    }

    [Fact]
    public void Test_BuildSparseMatrix()
    {
        int row_count = 5;
        int column_count = 5;
        var sparseMatrix = LinearAlgebraUtilities.CreateSparseDiagonalMatrix(row_count, column_count);

        Assert.Equal(row_count, sparseMatrix.RowCount);
        Assert.Equal(column_count, sparseMatrix.ColumnCount);
        for (int i = 0; i < row_count; i++)
        {
            for (int j = 0; j < column_count; j++)
            {
                if (i == j)
                {
                    Assert.Equal(1.0, sparseMatrix[i, j]);
                }
                else
                {
                    Assert.Equal(0.0, sparseMatrix[i, j]);
                }
            }
        }
    }

    [Fact]
    public void Test_BuildDenseDiagonalMatrix()
    {
        int row_count = 5;
        int col_count = 5;
        var denseDiagonalMatrix = LinearAlgebraUtilities.CreateDenseDiagonalMatrix(row_count, col_count);
        Assert.Equal(row_count, denseDiagonalMatrix.RowCount);
        Assert.Equal(col_count, denseDiagonalMatrix.ColumnCount);
        for (int i = 0; i < row_count; i++)
        {
            for (int j = 0; j < col_count; j++)
            {
                if (i == j)
                {
                    Assert.Equal(1.0, denseDiagonalMatrix[i, j]);
                }
                else
                {
                    Assert.Equal(0.0, denseDiagonalMatrix[i, j]);
                }
            }
        }
    }
}
