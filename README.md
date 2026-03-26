# Dotnet.DeepSigma.Mathematics

A focused C# mathematics library for **linear algebra**, **statistics**, **randomized selection**, **basic trigonometry**, and an **NLopt-based optimization example**.

The repository currently targets **.NET 10.0** and packages the library as **DeepSigma.Mathematics**. It also includes a small xUnit test project covering the vector API and weighted random sampling behavior.

## What is included

### Linear algebra
- Generic `Vector<T>` for numeric types implementing `INumber<T>`
- Vector addition, subtraction, scalar multiplication, dot product
- Length, unit vector, orthogonality checks
- Cosine similarity and angle between vectors
- Generic `Matrix<T>` with matrix addition, subtraction, scalar multiplication, and matrix multiplication
- `MatrixDimension` helper type
- `VectorBuilder` collection builder support

### Randomization
- `WeightedRandom<T>` for weighted random sampling with replacement
- Cumulative-weight implementation with binary search for selection

### Statistics
`StatisticsUtilities` contains a mix of general and finance-oriented helpers, including:
- Coefficient of variation
- Standard error
- Estimate standard deviation from range
- Normal, log-normal, and Poisson distribution helpers
- Z-score
- Total return and annualized return
- Variance and standard deviation
- Correlation, covariance, beta, R-squared, Jensen's alpha
- Annualized volatility, tracking error, and max drawdown

### Trigonometry
- Degrees-to-radians conversion

### Optimization
- `Optimization.RunTestOptimization(...)` demonstrates constrained nonlinear optimization using **NLoptNet**

## Current maturity

This package is usable today for several small utility scenarios, especially:
- vector arithmetic
- weighted random selection
- finance/statistics helper methods
- simple trigonometric conversion

A few areas are clearly still in progress:
- `Matrix<T>.Rank` is not implemented
- `Matrix<T>.Transpose(...)` is not implemented
- the optimization class currently exposes a demonstration/test optimization routine rather than a broader optimization API
- the automated tests currently cover `Vector<T>` and `WeightedRandom<T>`, but not the full library surface

## Target framework and dependencies

The library targets:

```xml
<TargetFramework>net10.0</TargetFramework>
```

Key package dependencies:
- `DeepSigma.General`
- `MathNet.Numerics`
- `NLoptNet`
- `System.Numerics.Tensors`

## Project structure

```text
Dotnet.DeepSigma.Mathematics/
├── DeepSigma.Mathematics/
│   ├── LinearAlgebra/
│   │   ├── Matrix.cs
│   │   ├── MatrixDimension.cs
│   │   ├── Vector.cs
│   │   └── VectorBuilder.cs
│   ├── Optimization/
│   │   └── Optimization.cs
│   ├── Randomization/
│   │   └── WeightedRandom.cs
│   ├── Statistics/
│   │   └── StatisticsUtilities.cs
│   ├── Trigonometry/
│   │   └── TrigonometryUtilities.cs
│   └── DeepSigma.Mathematics.csproj
└── DeepSigma.Mathematics.Tests/
    └── Tests/
        ├── Vector_Test.cs
        └── WeightedRandom_Test.cs
```

## Getting started

### Clone and build

```bash
git clone https://github.com/DeepSigma-LLC/Dotnet.DeepSigma.Mathematics.git
cd Dotnet.DeepSigma.Mathematics
dotnet build
```

### Run tests

```bash
dotnet test
```

### Reference from another solution

You can add a project reference directly:

```bash
dotnet add reference ./DeepSigma.Mathematics/DeepSigma.Mathematics.csproj
```

## Usage

### Vector operations

```csharp
using DeepSigma.Mathematics.LinearAlgebra;

Vector<int> a = new(1, 2, 3);
Vector<int> b = Vector<int>.GetOneVector(3);

Vector<int> sum = a + b;          // [2, 3, 4]
int dot = a.Dot(b);               // 6
double length = a.Length();       // sqrt(14)
bool orthogonal = a.ArePerpendicularTo(new Vector<int>(0, -3, 2));
```

### Cosine similarity and angle

```csharp
using DeepSigma.Mathematics.LinearAlgebra;

Vector<int> x = new(1, 0);
Vector<int> y = new(0, 1);

decimal cosine = x.CosineOfAngleBetweenVector(y); // 0
decimal angle = x.AngleBetweenVector(y);          // pi / 2
```

### Weighted random sampling

```csharp
using DeepSigma.Mathematics.Randomization;

WeightedRandom<string> sampler = new();
sampler.AddItem("Low", 1);
sampler.AddItem("Medium", 3);
sampler.AddItem("High", 6);

string next = sampler.Next();
```

### Statistics utilities

```csharp
using DeepSigma.Mathematics.Statistics;

decimal[] returns = [0.01m, -0.02m, 0.015m, 0.005m];

decimal totalReturn = StatisticsUtilities.CalculateTotalReturn(returns);
decimal variance = StatisticsUtilities.CalculateSampleVariance(returns);
decimal stdDev = StatisticsUtilities.CalculateSampleStandardDeviation(returns);
decimal maxDrawdown = StatisticsUtilities.CalculateMaxDrawdown(returns);
```

### Degrees to radians

```csharp
using DeepSigma.Mathematics.Trigonometry;

double radians = TrigonometryUtilities.DegreesToRadians(180); // 3.141592653589793
```

### Optimization demo

```csharp
using DeepSigma.Mathematics.Optimization;

Optimization optimization = new();
var result = optimization.RunTestOptimization();

Console.WriteLine(result.output);
Console.WriteLine(string.Join(", ", result.optimized_parameters));
Console.WriteLine(result.final_objective_score);
```

## Notes on implementation

- `Vector<T>` is generic and constrained to numeric types via `INumber<T>`
- `WeightedRandom<T>` ignores non-positive weights when items are added
- The statistical helpers use `decimal` heavily, which is useful for finance-oriented calculations
- Distribution functions are delegated to `MathNet.Numerics`
- Optimization is delegated to `NLoptNet`

## Testing

The repository currently includes tests for:
- zero-vector and one-vector behavior
- vector indexing, length, dot product, cosine similarity, and angle calculations
- weighted random sampling frequencies over repeated trials

These tests provide a good starting point, but there is room to expand coverage for:
- matrix operations
- statistical utility methods
- optimization behavior
- edge-case handling and numeric stability

## Suggested roadmap

Potential next improvements for the library:
1. Implement matrix transpose and rank
2. Add matrix-focused test coverage
3. Expand XML documentation examples
4. Add CI for build and test validation
5. Publish a NuGet package and document installation
6. Add benchmarks for vector, matrix, and sampling operations

## License

MIT

## Repository

GitHub: https://github.com/DeepSigma-LLC/Dotnet.DeepSigma.Mathematics
