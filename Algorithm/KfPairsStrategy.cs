using System;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;
using static System.Decimal;

namespace KfPairs.Algorithm
{
    internal enum Direction
    {
        Up,
        Down,
        Flat
    }

    public class KfPairsStrategy : QCAlgorithm
    {
        private PriceSpread _priceSpread;
        private Direction _dir;
        private StandardDeviation _spreadStd;

        private String _fstSym;
        private String _sndSym;
        private decimal _leverage;
        private RollingWindow<decimal> _rollingSpread;
        private int _lookBack;
        private HalfLife _halfLife;
        private DateTime _startTime;

        public override void Initialize()
        {
            SetWarmup(TimeSpan.FromDays(100));
            SetStartDate(2016, 01, 01);
            SetCash(9640);
            SetBrokerageModel(
                BrokerageName.InteractiveBrokersBrokerage,
                AccountType.Margin
            );

            _fstSym = "SYK";
            _sndSym = "BAX";

            var fst = AddEquity(_fstSym, Resolution.Minute);
            var snd = AddEquity(_sndSym, Resolution.Minute);

            fst.SetDataNormalizationMode(DataNormalizationMode.Raw);
            snd.SetDataNormalizationMode(DataNormalizationMode.Raw);

            _leverage = 7.5m;

            fst.SetLeverage(_leverage);
            snd.SetLeverage(_leverage);

            _dir = Direction.Flat;

            InitIndicators();
        }


        public override void OnData(Slice data)
        {
            var fstPrice = ToDouble(Securities[_fstSym].Close);
            var sndPrice = ToDouble(Securities[_sndSym].Close);

            if (data == null) return;

            UpdateIndicators(fstPrice, sndPrice);

            if (!AreIndicatorsReady()) return;

            if (!Portfolio.Invested)
            {
                TakePosition();
            }

            if (Portfolio.Invested)
            {
                ExitPosition();
            }

            PlotState();
            PlotSpread(_priceSpread.Spread, _spreadStd.Current.Value);
            PlotHalfLife();
        }

        private void InitIndicators()
        {
            _lookBack = 1440;

            _rollingSpread = new RollingWindow<decimal>(_lookBack);
            _spreadStd = new StandardDeviation(_lookBack);
            _priceSpread = new PriceSpread();
            _halfLife = new HalfLife();
        }

        private void UpdateIndicators(double fstPrice, double sndPrice)
        {
            _priceSpread.Update(fstPrice, sndPrice);
            _rollingSpread.Add(_priceSpread.Spread);
            _spreadStd.Update(Time, _priceSpread.Spread);

            if (!_rollingSpread.IsReady) return;

            var currSpread = _rollingSpread[0];
            var prevSpread = _rollingSpread[1];

            _halfLife.Update(Convert.ToDouble(prevSpread), Convert.ToDouble(currSpread));
        }

        private bool AreIndicatorsReady()
        {
            return _rollingSpread.IsReady && _spreadStd.IsReady;
        }

        private void PlotState()
        {
            Plot("state", "slope", _priceSpread.Slope);
            Plot("state", "intercept", _priceSpread.Intercept);
        }

        private void PlotSpread(decimal spread, decimal bound)
        {
            Plot("Spread", "spread", spread);
            Plot("Spread", "upper", bound);
            Plot("Spread", "lower", -bound);
        }

        private void PlotHalfLife()
        {
            Plot("half life", "half life", _halfLife.Period);
        }

        private void TakePosition()
        {
            var spread = _priceSpread.Spread;
            var bound = _spreadStd.Current.Value;

            var fstRation = _priceSpread.FstRation * ToDouble(_leverage) * 0.8;
            var sndRation = _priceSpread.SndRation * ToDouble(_leverage) * 0.8;

            if (spread >= bound)
            {
                SetHoldings(_fstSym, -fstRation);
                SetHoldings(_sndSym, sndRation);
                _dir = Direction.Down;
                _startTime = Time;
            }
            else if (spread <= -bound)
            {
                SetHoldings(_fstSym, fstRation);
                SetHoldings(_sndSym, -sndRation);
                _dir = Direction.Up;
                _startTime = Time;
            }
        }


        private void ExitPosition()
        {
            var spread = _priceSpread.Spread;
            var bound = _spreadStd.Current.Value;

            var exitBoundVal = 0.75m * bound;

            var isUpPastMean = _dir == Direction.Up && spread >= -exitBoundVal;
            var isDownPastMean = _dir == Direction.Down && spread <= exitBoundVal;

            var holdingPeriod = Time - _startTime;

            var isHoldingTooLong = Convert.ToDecimal(holdingPeriod.TotalMinutes) > _halfLife.Period * 5m;

            if (isUpPastMean || isDownPastMean || isHoldingTooLong)
            {
                Liquidate();
            }
        }
    }
}