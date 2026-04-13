using Xunit;
using DeepSigma.Mathematics.LinearAlgebra;
using DeepSigma.Core.Extensions;

namespace DeepSigma.Mathematics.Tests.Tests;

public class Vector_Test
{
    [Fact]
    public void AddingZeroVector_ReturnsSameVector()
    {
        CustomVector<decimal> x = new(1, 3, 2);
        CustomVector<decimal> y = CustomVector<decimal>.GetZeroVector(3);
        CustomVector<decimal> x_plus_y = x + y;

        Assert.Equal(x, x_plus_y);
    }


    [Fact]
    public void SubtractingZeroVector_ReturnsSameVector()
    {
        CustomVector<decimal> x = [1, 3, 2];
        CustomVector<decimal> y = CustomVector<decimal>.GetZeroVector(3);
        CustomVector<decimal> x_minus_y = x - y;
        Assert.Equal(x, x_minus_y);
    }

    [Fact]
    public void AddingOneVector_ReturnsVectorWithComponentsIncreasedByOne()
    {
        CustomVector<decimal> x = [1, 3, 2];
        CustomVector<decimal> y = CustomVector<decimal>.GetOneVector(3);
        CustomVector<decimal> x_plus_y = x + y;
        Assert.Equal(new CustomVector<decimal>(2, 4, 3), x_plus_y);
    }


    [Fact]
    public void SubtractingOneVector_ReturnsVectorWithComponentsDecreasedByOne()
    {
        CustomVector<decimal> x = [1, 3, 2];
        CustomVector<decimal> y = CustomVector<decimal>.GetOneVector(3);
        CustomVector<decimal> x_minus_y = x - y;
        Assert.Equal(new CustomVector<decimal>(0, 2, 1), x_minus_y);
    }


    [Fact]
    public void GetDimensions_ReturnsNumberOfComponents()
    {
        CustomVector<decimal> x = [1, 3, 2];
        Assert.Equal(3, x.Dimension);
    }


    [Fact]
    public void Indexer_ReturnsCorrectComponent()
    {
        CustomVector<decimal> x = [1, 3, 2];
        Assert.Equal(1, x[0]);
        Assert.Equal(3, x[1]);
        Assert.Equal(2, x[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_ThrowsException() 
    {
        CustomVector<decimal> x = [1, 2, 3];
        Assert.Throws<IndexOutOfRangeException>(() => x[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => x[3]);
    }

    [Fact]
    public void GetLengthOfVector_ReturnsCorrectLength()
    {
        CustomVector<decimal> x = [3, 4];
        double length = x.Length();
        Assert.Equal(5, length);
    }

    [Fact]
    public void GetDotProduct_ReturnsCorrectValue()
    {
        CustomVector<decimal> x = [1, 2];
        CustomVector<decimal> y = [4, 6];
        decimal dotProduct = x.Dot(y);
        Assert.Equal(16, dotProduct);
    }

    [Fact]
    public void GetCosineSimilarity_ReturnsCorrectValue()
    {
        CustomVector<decimal> x = [1, 0];
        CustomVector<decimal> y = [0, 1];
        decimal cosineSimilarity = x.CosineOfAngleBetweenVector(y);
        Assert.Equal(0, cosineSimilarity);
    }

    [Fact]
    public void GetCosineSimilarityOfParallelVectors_ReturnsOne()
    {
        CustomVector<decimal> x = [1, 2];
        CustomVector<decimal> y = [2, 4];
        decimal cosineSimilarity = x.CosineOfAngleBetweenVector(y);
        Assert.Equal(1, cosineSimilarity);
    }

    [Fact]
    public void GetCosineSimilarityOfAntiParallelVectors_ReturnsNegativeOne()
    {
        CustomVector<decimal> x = [1, 2];
        CustomVector<decimal> y = [-1, -2];
        decimal cosineSimilarity = x.CosineOfAngleBetweenVector(y);
        Assert.Equal(-1, cosineSimilarity);
    }

    [Fact]
    public void GetAngleBetweenVectors_ReturnsCorrectValue()
    {
        CustomVector<decimal> x = [1, 0];
        CustomVector<decimal> y = [0, 1];
        decimal angle = x.AngleBetweenVector(y);
        Assert.Equal(Math.PI.ToDecimal() / 2, angle, 12);
    }
}

