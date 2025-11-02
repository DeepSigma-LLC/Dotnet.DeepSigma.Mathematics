
using Xunit;
using DeepSigma.Mathematics.Randomization;

namespace DeepSigma.Mathematics.Tests.Tests;

public class WeightedRandom_Test
{
    [Fact]
    public void Test_WeightedRandom_Next()
    {
        // Arrange
        WeightedRandom<string> weightedRandom = new();
        weightedRandom.AddItem("A", 1); // 10%
        weightedRandom.AddItem("B", 2); // 20%
        weightedRandom.AddItem("C", 7); // 70%
        Dictionary<string, int> counts = new()
        {
            { "A", 0 },
            { "B", 0 },
            { "C", 0 }
        };
        int iterations = 100000;
        // Act
        for (int i = 0; i < iterations; i++)
        {
            string item = weightedRandom.Next();
            counts[item]++;
        }

        // Assert
        double percentA = (double)counts["A"] / iterations;
        double percentB = (double)counts["B"] / iterations;
        double percentC = (double)counts["C"] / iterations;
        Assert.InRange(percentA, 0.08, 0.12); // Allowing a margin of error
        Assert.InRange(percentB, 0.18, 0.22);
        Assert.InRange(percentC, 0.68, 0.72);
    }
}

