using System.Numerics;

namespace DeepSigma.Mathematics.LinearAlgebra;

/// <summary>
/// Provides utility methods for creating vectors from various data sources, such as arrays or spans.
/// </summary>
public static class VectorBuilder
{
    /// <summary>
    /// Creates a vector from an array of components.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static CustomVector<T> Create<T>(ReadOnlySpan<T> items)
        where T : INumber<T>
    {
        return new CustomVector<T>(items.ToArray());
    }
}