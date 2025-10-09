using System.Numerics.Tensors;

namespace DeepSigma.Mathematics;

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
    public static float GetVectorCosineSimilarty(float[] vector1, float[] vector2)
    {
        return TensorPrimitives.CosineSimilarity(vector1, vector2);
    }
    
}
