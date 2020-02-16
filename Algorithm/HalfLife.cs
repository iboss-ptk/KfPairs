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

            Period = _linearFilter.Slope == 0
                ? Decimal.MaxValue
                : Convert.ToDecimal(-Math.Log(2) / _linearFilter.Slope);
        }
    }
}

