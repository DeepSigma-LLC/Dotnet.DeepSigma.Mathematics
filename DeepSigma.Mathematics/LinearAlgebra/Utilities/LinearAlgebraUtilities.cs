using System.Numerics.Tensors;

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

    public static float Length(float[] vector)
    {
        float sum = vector.Sum();
        return (float)Math.Sqrt(sum);
    }
}
