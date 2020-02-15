using System;

namespace KfPairs.Algorithm
{
    public class PriceSpread
    {
        private readonly LinearFilter _linearFilter;
        
        public double Slope => _linearFilter.Slope;
        public double Intercept => _linearFilter.Intercept;

        public double FstRation => Slope / (1 + Slope);
        public double SndRation => 1 - FstRation;
        public decimal Spread { get; private set; }
        
        public PriceSpread()
        {
            _linearFilter = new LinearFilter();
        }

        public void Update(double fstPrice, double sndPrice)
        {
            _linearFilter.Update(sndPrice, fstPrice);
            Spread = Convert.ToDecimal(fstPrice - _linearFilter.Predict(sndPrice));
        }
    }
}