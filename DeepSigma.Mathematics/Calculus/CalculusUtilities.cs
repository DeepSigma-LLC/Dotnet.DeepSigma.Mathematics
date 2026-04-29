
namespace DeepSigma.Mathematics.Calculus;

/// <summary>
/// The CalculusUtilities class provides a collection of static methods and properties that are commonly used in calculus, such as differentiation, integration, limits, and series expansions.
/// </summary>
public class CalculusUtilities
{
    /// <summary>
    /// Approximates the first derivative of a function at a specified point using the central finite difference method.
    /// </summary>
    /// <remarks>This method uses the central difference formula to estimate the derivative, which generally
    /// provides higher accuracy than forward or backward difference methods for sufficiently small step sizes. The
    /// accuracy of the result depends on the choice of the step size parameter <paramref name="h"/> and the behavior of
    /// the function <paramref name="f"/> near <paramref name="x"/>.</remarks>
    /// <param name="f">The function for which to compute the derivative. This delegate represents a real-valued function of a single
    /// variable.</param>
    /// <param name="x">The point at which to evaluate the derivative.</param>
    /// <param name="h">The step size to use in the finite difference calculation. Must be a small, positive value. The default is 1e-5.</param>
    /// <returns>The approximate value of the first derivative of the function at the specified point.</returns>
    public static double FiniteDifferenceDerivative(Func<double, double> f, double x, double h = 1e-5)
    {
        return (f(x + h) - f(x - h)) / (2 * h);
    }

    /// <summary>
    /// Approximates the second derivative of a function at a specified point using the central finite difference
    /// method.
    /// </summary>
    /// <remarks>Smaller values of h generally yield more accurate results, but may increase the effect of
    /// floating-point rounding errors. The function f should be sufficiently smooth near x for the approximation to be
    /// valid.</remarks>
    /// <param name="f">The function for which to compute the second derivative. Must accept a double and return a double.</param>
    /// <param name="x">The point at which to evaluate the second derivative.</param>
    /// <param name="h">The step size to use in the finite difference calculation. Must be a small, positive value. The default is 1e-5.</param>
    /// <returns>The approximate value of the second derivative of the function at the specified point.</returns>
    public static double FiniteDifferenceSecondDerivative(Func<double, double> f, double x, double h = 1e-5)
    {
        return (f(x + h) - 2 * f(x) + f(x - h)) / (h * h);
    }

    /// <summary>
    /// Approximates the definite integral of a univariate function over a specified interval using the trapezoidal
    /// rule.
    /// </summary>
    /// <remarks>Increasing the value of n generally improves the accuracy of the approximation but may
    /// increase computation time. The method assumes that a is less than or equal to b; if a equals b, the result is
    /// zero.</remarks>
    /// <param name="f">The function to integrate. Represents a mathematical function of a single variable.</param>
    /// <param name="a">The lower bound of the integration interval.</param>
    /// <param name="b">The upper bound of the integration interval.</param>
    /// <param name="n">The number of subintervals to use for the approximation. Must be greater than zero. Defaults to 1000.</param>
    /// <returns>The approximate value of the definite integral of the function over the interval [a, b].</returns>
    public static double NumericalIntegration(Func<double, double> f, double a, double b, int n = 1000)
    {
        double h = (b - a) / n;
        double sum = 0.5 * (f(a) + f(b));
        for (int i = 1; i < n; i++)
        {
            sum += f(a + i * h);
        }
        return sum * h;
    }

    /// <summary>
    /// Estimates the change in the value of a function for a small change in its input using a finite difference
    /// approximation.
    /// </summary>
    /// <remarks>This method uses a finite difference derivative to approximate the change in the function's
    /// value. The accuracy of the estimate depends on the choice of deltaX and the behavior of the function near
    /// x.</remarks>
    /// <param name="f">The function for which to estimate the change in value. This delegate represents a real-valued function of a
    /// single real variable.</param>
    /// <param name="x">The point at which to evaluate the function and its estimated change.</param>
    /// <param name="deltaX">The small increment to apply to the input value. Represents the change in the independent variable.</param>
    /// <returns>A double representing the estimated change in the function's value at the specified point for the given
    /// increment.</returns>
    public static double EstimatedChangeInFunctionValue(Func<double, double> f, double x, double deltaX)
    {
        return FiniteDifferenceDerivative(f, x) * deltaX;
    }

    public static double EstimatedChangeInFunctionValue(double derivativeAtX, double deltaX) 
        => derivativeAtX * deltaX;
    

    /// <summary>
    /// Estimates the change in the value of a function at a specified point using both the first and second
    /// derivatives.
    /// </summary>
    /// <remarks>This method combines estimates from the first and second derivatives to provide a more
    /// accurate approximation of the function's change over the specified interval. The accuracy of the estimate
    /// depends on the smoothness of the function and the choice of deltaX.</remarks>
    /// <param name="f">The function for which to estimate the change in value. This delegate represents a real-valued function of a
    /// single variable.</param>
    /// <param name="x">The point at which to evaluate the function and its derivatives.</param>
    /// <param name="deltaX">The increment by which to change the input value. Represents the step size for the estimation.</param>
    /// <returns>A double representing the estimated change in the function's value at the specified point, incorporating both
    /// first and second derivative approximations.</returns>
    public static double EstimatedChangeInFunctionValueWithSecondDerivative(Func<double, double> f, double x, double deltaX)
    {
        return EstimatedChangeInFunctionValue(f, x, deltaX) + EstimatedChangeInFunctionValueSecondDerivative(f, x, deltaX);
    }

    /// <summary>
    /// Estimates the change in a function's value at a point using both the first and second derivatives and a
    /// specified increment.
    /// </summary>
    /// <remarks>This method combines linear and quadratic approximations to provide a more accurate estimate
    /// of the function's change for small increments. It is commonly used in numerical analysis and Taylor series
    /// approximations.</remarks>
    /// <param name="derivativeAtX">The value of the first derivative of the function at the point of interest. Represents the instantaneous rate of
    /// change at that point.</param>
    /// <param name="secondDerivativeAtX">The value of the second derivative of the function at the point of interest. Represents the curvature or rate of
    /// change of the first derivative.</param>
    /// <param name="deltaX">The increment by which the input variable changes. Typically represents a small change from the point of
    /// interest.</param>
    /// <returns>A double representing the estimated change in the function's value, calculated using both the first and second
    /// derivatives and the specified increment.</returns>
    public static double EstimatedChangeInFunctionValueWithSecondDerivative(double derivativeAtX, double secondDerivativeAtX, double deltaX)
        => EstimatedChangeInFunctionValue(derivativeAtX, deltaX) + EstimatedChangeInFunctionValueSecondDerivative(secondDerivativeAtX, deltaX);

    private static double EstimatedChangeInFunctionValueSecondDerivative(Func<double, double> f, double x, double deltaX)
    {
        return 0.5 * FiniteDifferenceSecondDerivative(f, x) * deltaX * deltaX;
    }

    private static double EstimatedChangeInFunctionValueSecondDerivative(double secondDerivativeAtX, double deltaX)
        => 0.5 * secondDerivativeAtX * deltaX * deltaX;
}
