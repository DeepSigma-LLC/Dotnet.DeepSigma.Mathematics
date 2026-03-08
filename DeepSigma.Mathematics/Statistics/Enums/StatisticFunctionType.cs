
namespace DeepSigma.Mathematics.Statistics.Enums;

/// <summary>
/// StatisticFunctionType is an enumeration that represents different types of statistical functions.
/// </summary>
public enum StatisticFunctionType
{
    /// <summary>
    /// Density function is a statistical function that describes the probability distribution of a continuous random variable. 
    /// It provides the relative likelihood of the random variable taking on a specific value or falling within a certain range. 
    /// The density function is often represented as a curve, where the area under the curve corresponds to probabilities.
    /// </summary>
    Density,
    /// <summary>
    /// Distribution function is a statistical function that describes the probability that a random variable takes on a value less than or equal to a given point.
    /// It provides a cumulative measure of the likelihood of different outcomes and is often represented as a curve that increases from 0 to 1.
    /// </summary>
    Distribution,
}
