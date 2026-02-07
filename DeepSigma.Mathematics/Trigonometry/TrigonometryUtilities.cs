

namespace DeepSigma.Mathematics.Trigonometry;

/// <summary>
/// Utility class for common trigonometric operations.
/// </summary>
public static class TrigonometryUtilities
{
    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
    public static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
