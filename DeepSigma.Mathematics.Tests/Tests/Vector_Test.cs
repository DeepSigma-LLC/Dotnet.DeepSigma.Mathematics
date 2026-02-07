using Xunit;
using DeepSigma.Mathematics.LinearAlgebra;

namespace DeepSigma.Mathematics.Tests.Tests;

public class Vector_Test
{
    [Fact]
    public void AddingZeroVector_ReturnsSameVector()
    {
        Vector<decimal> x = new(1, 3, 2);
        Vector<decimal> y = Vector<decimal>.GetZeroVector(3);
        Vector<decimal> x_plus_y = x + y;

        Assert.Equal(x, x_plus_y);
    }


    [Fact]
    public void SubtractingZeroVector_ReturnsSameVector()
    {
        Vector<decimal> x = [1, 3, 2];
        Vector<decimal> y = Vector<decimal>.GetZeroVector(3);
        Vector<decimal> x_minus_y = x - y;
        Assert.Equal(x, x_minus_y);
    }

    [Fact]
    public void AddingOneVector_ReturnsVectorWithComponentsIncreasedByOne()
    {
        Vector<decimal> x = [1, 3, 2];
        Vector<decimal> y = Vector<decimal>.GetOneVector(3);
        Vector<decimal> x_plus_y = x + y;
        Assert.Equal(new Vector<decimal>(2, 4, 3), x_plus_y);
    }


    [Fact]
    public void SubtractingOneVector_ReturnsVectorWithComponentsDecreasedByOne()
    {
        Vector<decimal> x = [1, 3, 2];
        Vector<decimal> y = Vector<decimal>.GetOneVector(3);
        Vector<decimal> x_minus_y = x - y;
        Assert.Equal(new Vector<decimal>(0, 2, 1), x_minus_y);
    }


    [Fact]
    public void GetDimensions_ReturnsNumberOfComponents()
    {
        Vector<decimal> x = [1, 3, 2];
        Assert.Equal(3, x.Dimension);
    }


    [Fact]
    public void Indexer_ReturnsCorrectComponent()
    {
        Vector<decimal> x = [1, 3, 2];
        Assert.Equal(1, x[0]);
        Assert.Equal(3, x[1]);
        Assert.Equal(2, x[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_ThrowsException() 
    {
        Vector<decimal> x = [1, 2, 3];
        Assert.Throws<IndexOutOfRangeException>(() => x[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => x[3]);
    }

    [Fact]
    public void GetLengthOfVector_ReturnsCorrectLength()
    {
        Vector<decimal> x = [3, 4];
        double length = x.Length();
        Assert.Equal(5, length);
    }





}

