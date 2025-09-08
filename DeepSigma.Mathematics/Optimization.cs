using NLoptNet;

namespace DeepSigma.Mathematics
{
    /// <summary>
    /// Optimization utilities.
    /// </summary>
    public class Optimization
    {
        /// <summary>
        /// Test optimization: min (x−0.2)² + (y−0.8)² subject to x + y = 1 | 0 ≤ x,y ≤ 1
        /// </summary>
        public (string output, double[] optimized_parameters, double? final_objective_score) RunTestOptimization(int maximum_iterations = 1000)
        {
            using NLoptSolver solver = new NLoptSolver(NLoptAlgorithm.LD_SLSQP, 2, 1e-6, maximum_iterations);
            solver.SetLowerBounds([0.0, 0.0]);
            solver.SetUpperBounds([1.0, 1.0]);

            // Equality constraint: x + y - 1 = 0 (with tolerance)
            solver.AddEqualZeroConstraint((variable, partial_differential) => {
                if (partial_differential != null) // Gradients may be required for some algorithms. If so, set them here. 
                {
                    partial_differential[0] = 1.0;
                    partial_differential[1] = 1.0; 
                }
                return variable[0] + variable[1] - 1.0;
            }, 1e-8);

            // Objective (with gradient)
            solver.SetMinObjective((variable, partial_differential) => 
            {
                if (partial_differential != null) // Gradients may be required for some algorithms. If so, set them here. 
                {
                    partial_differential[0] = 2 * (variable[0] - 0.2);
                    partial_differential[1] = 2 * (variable[1] - 0.8); 
                }
                return Math.Pow(variable[0] - 0.2, 2) + Math.Pow(variable[1] - 0.8, 2);
            });

            double[] inital_values = [0.5, 0.5];
            NloptResult result = solver.Optimize(inital_values, out double? final_objective_score);

            return (result.ToString(), inital_values, final_objective_score);
        }
    }
}
