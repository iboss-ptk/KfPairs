using System;
using MathNet.Filtering.Kalman;
using MathNet.Numerics.LinearAlgebra;

namespace KfPairs.Algorithm
{
    public class PriceSpread
    {
        public decimal Spread { get; private set; }
        public double FstRation { get; private set; }
        public double SndRation { get; private set; }
        public double Slope { get; private set; }
        public double Intercept { get; private set; }

        private readonly IKalmanFilter _kalmanFilter;

        public PriceSpread()
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

        public void Update(double fstPrice, double sndPrice)
        {
            var measurement = Matrix(new[,]
            {
                {fstPrice}
            });

            var measurementModel = Matrix(new[,]
            {
                {sndPrice, 1.0}
            });

            var cov = Matrix(new double[,]
            {
                {1.0}
            });

            _kalmanFilter.Update(measurement, measurementModel, cov);

            var slope = _kalmanFilter.State[0, 0];
            Slope = slope;

            var intercept = _kalmanFilter.State[1, 0];
            Intercept = intercept;

            var predictedFstPrice = (slope * sndPrice) + intercept;

            FstRation = slope / (1 + slope);
            SndRation = 1 - FstRation;

            Spread = Convert.ToDecimal(fstPrice - predictedFstPrice);
        }

        private Matrix<double> Matrix(double[,] arr)
        {
            return Matrix<double>.Build.DenseOfArray(arr);
        }
    }
}