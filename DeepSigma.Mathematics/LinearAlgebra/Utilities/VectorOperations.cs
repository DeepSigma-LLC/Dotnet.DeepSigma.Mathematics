using DeepSigma.Core.Extensions;
using System.Numerics;

namespace DeepSigma.Mathematics.LinearAlgebra.Utilities;

/// <summary>
/// Provides operations for vectors, such as calculating the length (magnitude) of a vector.
/// </summary>
public static class VectorOperations
{
    /// <summary>
    /// Calculates the length (magnitude) of a vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static decimal Length(CustomVector<decimal> vector)
    {
        decimal sum = vector.Sum();
        return Math.Sqrt(sum);
    }
}
