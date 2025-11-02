namespace DeepSigma.Mathematics.Randomization;

/// <summary>
/// Represents an item with an associated weight for weighted random selection.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Item"></param>
/// <param name="Weight"></param>
public record WeightedItem<T>(T Item, double Weight);

/// <summary>
/// A class for selecting items based on weighted probabilities. 
/// WeightedRandom allows adding items with associated weights and selecting an item randomly, with the likelihood of selection proportional to its weight.
/// Sampling is done with replacement.
/// </summary>
/// <typeparam name="T"></typeparam>
public class WeightedRandom<T>
{
    private readonly List<WeightedItem<T>> _items = [];
    private readonly Random _random = new();
    private double _totalWeight = 0;

    /// <summary>
    /// Adds an item with the specified weight to the collection.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="weight"></param>
    public void AddItem(T item, double weight)
    {
        if (weight <= 0) return;
        _totalWeight += weight;
        _items.Add(new WeightedItem<T>(item, _totalWeight));
    }

    /// <summary>
    /// Selects and returns an item based on the defined weights.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Next()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("No items to choose from.");

        double r = _random.NextDouble() * _totalWeight;

        int index = _items.BinarySearch(
            new WeightedItem<T>(default, r),
            Comparer<WeightedItem<T>>.Create((a, b) => a.Weight.CompareTo(b.Weight))
        );

        if (index < 0)
            index = ~index;

        return _items[Math.Min(index, _items.Count - 1)].Item;
    }
}
