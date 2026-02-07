
using DeepSigma.General.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.LinearAlgebra;

/// <summary>
/// Represents a mathematical vector of arbitrary dimension, where each component is of a numeric type specified by the generic parameter <typeparamref name="T"/>. 
/// The class provides basic vector operations such as addition and subtraction, and allows for indexed access to its components. 
/// The dimension of the vector is determined by the number of components provided during initialization.
/// </summary>
/// <typeparam name="T"></typeparam>
[CollectionBuilder(typeof(VectorBuilder), nameof(VectorBuilder.Create))]
public class Vector<T>
    : IEnumerable<T>
    where T : INumber<T>
{
    private readonly T[] _components;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector{T}"/> class with the specified components. The number of components provided determines the dimension of the vector.
    /// </summary>
    /// <param name="components"></param>
    public Vector(params T[] components)
    {
        _components = components ?? throw new ArgumentNullException(nameof(components));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector{T}"/> class with the specified components, with an option to copy the input array.
    /// </summary>
    /// <param name="components"></param>
    /// <param name="copy"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Vector(T[] components, bool copy)
    {
        if (components == null)
            throw new ArgumentNullException(nameof(components));

        _components = copy ? (T[])components.Clone() : components;
    }

    /// <summary>
    /// Implicit conversion from T[] to Vector{T}. 
    /// This allows an array of type T to be directly assigned to a variable of type Vector{T}, creating a new vector instance with the array's components.
    /// </summary>
    public static implicit operator Vector<T>(T[] components) =>  new(components);
    
    /// <summary>
    /// Returns a new vector of the specified dimension where all components are initialized to zero. 
    /// The dimension of the resulting vector is determined by the <paramref name="dimension"/> parameter, and each component is set to the default value of type <typeparamref name="T"/> (which is zero for numeric types). 
    /// This method provides a convenient way to create a zero vector of any desired dimension.
    /// </summary>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static Vector<T> GetZeroVector(int dimension)
    {
        T[] zeroComponents = new T[dimension];
        for (int i = 0; i < dimension; i++)
        {
            zeroComponents[i] = T.Zero;
        }
        return new Vector<T>(zeroComponents);
    }

    /// <summary>
    /// Returns a new vector of the specified dimension where all components are initialized to one.
    /// </summary>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static Vector<T> GetOneVector(int dimension)
    {
        T[] oneComponents = new T[dimension];
        for (int i = 0; i < dimension; i++)
        {
            oneComponents[i] = T.One;
        }
        return new Vector<T>(oneComponents);
    }

    /// <summary>
    /// Gets the number of components in the vector, which corresponds to its dimension.
    /// </summary>
    public int Dimension => _components.Length;

    /// <summary>
    /// Provides indexed access to the components of the vector. The index is zero-based, meaning that the first component is accessed with index 0, the second with index 1, and so on.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index] => _components[index];

    /// <summary>
    /// Adds two vectors of the same dimension and returns a new vector as the result.
    /// </summary>
    /// <param name="a"> The first vector in the addition operation. Represents the augend.</param>
    /// <param name="b"> The second vector in the addition operation. Represents the addend.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Vector<T> operator +(Vector<T> a, Vector<T> b)
    {
        if (a.Dimension != b.Dimension)
            throw new InvalidOperationException("Vectors must have the same dimension for addition.");

        T[] resultComponents = new T[a.Dimension];
        for (int i = 0; i < a.Dimension; i++)
        {
            resultComponents[i] = a[i] + b[i];
        }
        return new Vector<T>(resultComponents);
    }

    /// <summary>
    /// Subtracts one vector from another, returning a new vector representing the element-wise difference.
    /// </summary>
    /// <param name="a">The first vector in the subtraction operation. Represents the minuend.</param>
    /// <param name="b">The second vector in the subtraction operation. Represents the subtrahend.</param>
    /// <returns>A new vector whose components are the result of subtracting each component of <paramref name="b"/> from the
    /// corresponding component of <paramref name="a"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="a"/> and <paramref name="b"/> do not have the same dimension.</exception>
    public static Vector<T> operator -(Vector<T> a, Vector<T> b)
    {
        if (a.Dimension != b.Dimension)
            throw new InvalidOperationException("Vectors must have the same dimension for subtraction.");

        T[] resultComponents = new T[a.Dimension];
        for (int i = 0; i < a.Dimension; i++)
        {
            resultComponents[i] = a[i] - b[i];
        }
        return new Vector<T>(resultComponents);
    }

    /// <summary>
    /// Multiplies a scalar value by a vector, returning a new vector where each component is the product of the original component and the scalar.
    /// </summary>
    /// <param name="scalar"></param>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector<T> operator *(T scalar, Vector<T> vector)
    {
        T[] resultComponents = new T[vector.Dimension];
        for (int i = 0; i < vector.Dimension; i++)
        {
            resultComponents[i] = scalar * vector[i];
        }
        return new Vector<T>(resultComponents);
    }

    /// <summary>
    /// Multiplies a vector by a scalar value, returning a new vector where each component is the product of the original component and the scalar.
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static Vector<T> operator *(Vector<T> vector, T scalar) => scalar * vector;

    /// <summary>
    /// Calculates the dot product of two vectors of the same dimension, returning a single value that represents the sum of the products of corresponding components from the two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <remarks>
    /// Summary of the properties of the dot product:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Taking the dot product of a vector with itself yields the square of its length, which is a measure of the vector's magnitude.</description>
    ///   </item>
    ///   <item>
    ///     <description>If the dot product of two vectors is zero, it indicates that the vectors are orthogonal (perpendicular) to each other in the vector space.</description>
    ///   </item>
    ///   <item>
    ///     <description>If the dot product is positive, it indicates that the vectors are pointing in generally the same direction, while a negative dot product indicates that they are pointing in generally opposite directions.</description>
    ///   </item>
    ///   <item>
    ///     <description>If the dot product is equal to the product of the lengths of the two vectors, it indicates that the vectors are parallel and pointing in the same direction. </description>
    ///   </item>
    ///   <item>
    ///     <description>If it is equal to the negative product of their lengths, they are parallel but pointing in opposite directions.</description>
    ///   </item>
    ///   <item>
    ///     <description>Taking the dot product of a vector with a unit vector in a particular direction gives the magnitude of the projection of the original vector onto that direction, which is useful in various applications such as calculating angles between vectors and determining components of vectors along specific axes.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Dot(Vector<T> other)
    {
        if (this.Dimension != other.Dimension)
            throw new InvalidOperationException("Vectors must have the same dimension for dot product.");

        T result = T.Zero;
        for (int i = 0; i < this.Dimension; i++)
        {
            result += this[i] * other[i];
        }
        return result;
    }

    /// <summary>
    /// Calculates the Euclidean length (or magnitude) of the vector, which is defined as the square root of the sum of the squares of its components.
    /// </summary>
    /// <returns></returns>
    public double Length()
    {
        // accumulate in double to avoid overflow in int/long squaring
        double sumSquares = 0d;

        foreach (var c in _components)
        {
            double d = double.CreateChecked(c); // throws if not representable
            sumSquares += d * d;
        }

        return Math.Sqrt(sumSquares);
    }

    /// <summary>
    /// Returns a new vector that has the same direction as the original vector but a length of 1.
    /// We first calculate the length of the original vector, and then divide each component by that length to normalize it.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Vector<decimal> UnitVector()
    {
        double length = Length();
        if (length == 0)
            throw new InvalidOperationException("Cannot compute the unit vector of a zero vector.");

        decimal[] unitComponents = new decimal[Dimension];
        for (int i = 0; i < Dimension; i++)
        {
            unitComponents[i] = decimal.CreateChecked(this[i]) / (decimal)length;
        }
        return new Vector<decimal>(unitComponents);
    }

    /// <summary>
    /// Determines whether the current vector is perpendicular (orthogonal) to another vector.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool ArePerpendicularTo(Vector<T> other)
    {
        return Dot(other) == T.Zero;
    }

    /// <summary>
    /// Calculates the cosine of the angle between the current vector and another vector using the dot product formula.
    /// Aka cosine similarity, which is a measure of the cosine of the angle between two vectors in an inner product space.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public decimal CosineOfAngleBetweenVector(Vector<T> other)
    {
        double lengthA = Length();
        double lengthB = other.Length();
        if (lengthA == 0 || lengthB == 0)
            throw new InvalidOperationException("Cannot compute the cosine of the angle between a zero vector and another vector.");

        double dotProduct = double.CreateChecked(Dot(other));
        double cosTheta = (dotProduct / (lengthA * lengthB));

        return Math.Clamp(cosTheta, -1.0, 1.0).ToDecimal();
    }

    /// <summary>
    /// Calculates the angle in radians between the current vector and another vector using the dot product formula.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public decimal AngleBetween(Vector<T> other)
    {
        decimal cosTheta = CosineOfAngleBetweenVector(other);
        return Math.Acos(cosTheta);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the components of the vector, allowing for enumeration using foreach loops and LINQ queries.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator() => _components.AsEnumerable().GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the components of the vector, allowing for enumeration using foreach loops and LINQ queries.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines whether the specified object is equal to the current vector. 
    /// Two vectors are considered equal if they have the same dimension and all corresponding components are equal.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        if (obj is Vector<T> other)
        {
            if (Dimension != other.Dimension)
                return false;

            for (int i = 0; i < Dimension; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns a hash code for the vector, which is computed based on its dimension and the values of its components.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Dimension, _components);
    }
}
