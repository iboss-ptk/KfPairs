using System;

namespace KfPairs.Algorithm
{
    public class HalfLife
    {
        public decimal Period { get; private set; }

        private readonly LinearFilter _linearFilter;

        public HalfLife()
        {
            _linearFilter =  new LinearFilter();
        }

        public void Update(double prev, double curr)
        {
            _linearFilter.Update(prev, curr - prev);
            Period = Convert.ToDecimal(-Math.Log(2) / _linearFilter.Slope);
        }
    }
}

