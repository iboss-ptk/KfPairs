using System;
using MathNet.Filtering.Kalman;
using MathNet.Numerics.LinearAlgebra;

namespace KfPairs.Algorithm
{
    public class HalfLife
    {
        public decimal Period { get; private set; }

        private readonly IKalmanFilter _kalmanFilter;

        public HalfLife()
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

        public void Update(double prevPrice, double curPrice)
        {
            var measurement = Matrix(new double[,]
            {
                {curPrice - prevPrice}
            });

            var measurementModel = Matrix(new double[,]
            {
                {prevPrice, 1.0}
            });

            var cov = Matrix(new double[,]
            {
                {1.0}
            });

            _kalmanFilter.Update(measurement, measurementModel, cov);

            var theta = _kalmanFilter.State[0, 0];

            Period = Convert.ToDecimal(-Math.Log(2) / theta);
        }

        private Matrix<double> Matrix(double[,] arr)
        {
            return Matrix<double>.Build.DenseOfArray(arr);
        }
    }
}

