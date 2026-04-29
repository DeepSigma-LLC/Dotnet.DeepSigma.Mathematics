namespace DeepSigma.Mathematics.Optimization;

/// <summary>
/// Internal argument-validation helpers shared across optimizers. Centralizes the
/// <c>finite</c>, <c>positive-finite</c>, and <c>finite-span</c> checks that otherwise
/// litter every public entry point.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws if <paramref name="value"/> is not a finite number (NaN or ±∞).
    /// </summary>
    public static void Finite(double value, string parameterName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be finite.");
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is not finite or is non-positive. Used for
    /// tolerances, step sizes, damping factors, and similar strictly-positive scalars.
    /// </summary>
    public static void PositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0.0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be finite and positive.");
        }
    }

    /// <summary>
    /// Throws if any element of <paramref name="span"/> is not finite. Reports the
    /// offending index in the exception message to aid debugging.
    /// </summary>
    public static void AllFinite(ReadOnlySpan<double> span, string parameterName)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (!double.IsFinite(span[i]))
            {
                throw new ArgumentOutOfRangeException(parameterName, span[i], $"{parameterName} must be finite at every index (failed at index {i}).");
            }
        }
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> if <paramref name="span"/> is empty.
    /// </summary>
    public static void NotEmpty(ReadOnlySpan<double> span, string parameterName, string description)
    {
        if (span.Length == 0)
        {
            throw new ArgumentException(description, parameterName);
        }
    }
}
