using System;
using MathNet.Filtering.Kalman;
using MathNet.Numerics.LinearAlgebra;

namespace KfPairs.Algorithm
{
    public class LinearFilter
    {
        private readonly IKalmanFilter _kalmanFilter;

        public double Slope => _kalmanFilter.State[0, 0];
        public double Intercept => _kalmanFilter.State[1, 0];

        public LinearFilter()
        {
            var initStateEstimate = Matrix(new[,]
            {
                {0.0},
                {0.0}
            });

            var initStateCov = Matrix(new[,]
            {
                {1.0, 0.0},
                {0.0, 1.0}
            });

            _kalmanFilter = new SquareRootFilter(initStateEstimate, initStateCov);
        }
        
        public void Update(double x, double y)
        {
            var measurement = Matrix(new[,]
            {
                {y}
            });

            var measurementModel = Matrix(new[,]
            {
                {x, 1.0}
            });

            var cov = Matrix(new[,]
            {
                {1.0}
            });

            _kalmanFilter.Update(measurement, measurementModel, cov);
        }

        public double Predict(double x)
        {
            return (Slope * x) + Intercept;
        }

        private Matrix<double> Matrix(double[,] arr)
        {
            return Matrix<double>.Build.DenseOfArray(arr);
        }
    }
}